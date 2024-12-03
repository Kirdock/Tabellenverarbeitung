using DataTableConverter.Extensions;
using DataTableConverter.Properties;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace DataTableConverter.Assisstant.importers
{
    internal class Renaming
    {
        internal string OldPath;
        internal string ParentPath;
        internal string Property;
    }

    internal static class XmlImporter
    {

        // Assumption: First XML-Element is the root object and this one contains a list. e.g. <Members><Member></Member></Members>
        internal static void Import(string path, DatabaseHelper databaseHelper, ProgressBar progressBar, Form1 mainForm, string tableName)
        {
            HashSet<string> overallColumns = new HashSet<string>();
            int counter = 0;
            SQLiteCommand insertCommand = null;
            Dictionary<string, string> rowData = new Dictionary<string, string>();
            HashSet<string> isArrayDict = new HashSet<string>();
            Dictionary<string, string> staticColumns = new Dictionary<string, string>();
            List<Renaming> renamings = new List<Renaming>();
            databaseHelper.CreateTable(new List<string>(), tableName);

            XmlReaderSettings settings = new XmlReaderSettings
            {
                Async = true,
                IgnoreWhitespace = true,
                IgnoreComments = true,
            };
            using (XmlReader reader = XmlReader.Create(path, settings))
            {
                reader.Read();

                // skip header information
                if (reader.NodeType == XmlNodeType.XmlDeclaration)
                {
                    reader.Read();
                }

                // read static columns
                if (reader.HasAttributes)
                {
                    while (reader.MoveToNextAttribute())
                    {
                        string columnName = reader.LocalName.Replace('-', '_');
                        staticColumns[columnName] = reader.Value;
                        rowData[columnName] = reader.Value;
                        overallColumns.Add(columnName);
                        databaseHelper.AddColumn(tableName, columnName);
                    }

                }

                // reading rows. Without the EndElement check there will be an empty row at the end
                while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
                {
                    // revert everything to empty string and keep columns, so that the SQL command does not need to be re-created and can be optimized
                    foreach (var key in rowData.Keys.ToList()) // two list because on enumerable there can't be edits in a foreach
                    {
                        rowData[key] = string.Empty;
                    }
                    // add static attributes
                    foreach (var pair in staticColumns)
                    {
                        rowData[pair.Key] = pair.Value;
                    }

                    HashSet<string> rowColumns = new HashSet<string>();
                    LoadRowData(reader, rowData, rowColumns, isArrayDict, overallColumns, databaseHelper, tableName, renamings);

                    foreach (var columnName in rowColumns)
                    {
                        bool added = overallColumns.Add(columnName);
                        if (added)
                        {
                            databaseHelper.AddColumn(tableName, columnName);
                        }
                    }

                    insertCommand = databaseHelper.InsertRow(rowData, tableName, insertCommand);
                    mainForm?.SetWorkflowText($"{++counter} Zeilen gelesen");
                }

                // Rename the first items of an array. From LIST_ITEM_ID to ITEM_1_ID
                foreach (var renaming in renamings)
                {
                    foreach (string from in overallColumns.Where(c => c.StartsWith(renaming.OldPath)))
                    {
                        string newTo = MergeColumnName(renaming.Property, renaming.ParentPath, 1, true, true);
                        string to = from.Replace(renaming.OldPath, newTo);
                        to = Settings.Default.ImportHeaderUpperCase ? to.ToUpper() : to;

                        databaseHelper.RenameAlias(from, to, tableName);
                    }
                }
                mainForm?.SetWorkflowText(string.Empty);
            }
        }

        internal static void LoadRowData(XmlReader rowReader, Dictionary<string, string> rowData, HashSet<string> rowColumns, HashSet<string> isArrayDict, HashSet<string> overallColumns, DatabaseHelper databaseHelper, string tableName, List<Renaming> renamings, string parentPath = "")
        {
            string rowElementName = rowReader.LocalName;
            bool isEmptyElement = rowReader.IsEmptyElement;
            if (rowReader.NodeType == XmlNodeType.EndElement)
            {
                return;
            }
            if (rowReader.HasAttributes)
            {
                while (rowReader.MoveToNextAttribute())
                {
                    string columnName = MergeColumnName(rowReader.LocalName, parentPath);
                    rowData[columnName] = rowReader.Value;
                    rowColumns.Add(columnName);
                }

            }

            if (isEmptyElement)
            {
                rowData[parentPath] = string.Empty;
                rowColumns.Add(parentPath);
                return;
            }

            // Read Cells
            string oldParentPath = parentPath;
            int itemNumber = 1;
            string previousElement = null;

            while (rowReader.Read() && rowReader.LocalName != rowElementName && rowReader.NodeType != XmlNodeType.EndElement)
            {
                bool isArray = isArrayDict.Contains(oldParentPath);
                bool isArrayInternal = previousElement == rowReader.LocalName;
                bool isParentList = isArray || isArrayInternal;
                previousElement = rowReader.LocalName;

                if (itemNumber != 1 && isParentList && parentPath == oldParentPath)
                {
                    parentPath = AlignArrayItemsAndPath(oldParentPath, rowElementName);
                }

                // rename existing or already added rows. It may be that ón the first iteration an array was not seen as an array because it contained only one item
                if (isArrayInternal && !isArray)
                {
                    isArrayDict.Add(oldParentPath);
                    string oldPrefix = $"{oldParentPath}_{rowReader.LocalName}";
                    renamings.Add(new Renaming() { OldPath = oldPrefix, ParentPath = AlignArrayItemsAndPath(oldParentPath, rowElementName), Property = rowReader.LocalName });
                }

                string columnName = MergeColumnName(rowReader.LocalName, parentPath, itemNumber, isParentList);
                if (rowReader.NodeType == XmlNodeType.Text)
                {
                    rowData[columnName] = rowReader.Value;
                    rowColumns.Add(columnName);
                }
                else
                {
                    LoadRowData(rowReader, rowData, rowColumns, isArrayDict, overallColumns, databaseHelper, tableName, renamings, columnName);
                }
                itemNumber++;
            }

            if (itemNumber == 1)
            {
                // empty value
                rowData[parentPath] = string.Empty;
                rowColumns.Add(parentPath);
            }
        }

        private static string AlignArrayItemsAndPath(string parentPath, string parentNodeName)
        {
            int endIndex = parentPath.Length - parentNodeName.Length - 1;
            return endIndex < 0 ? string.Empty : parentPath.Substring(0, endIndex);
        }

        private static string MergeColumnName(string columnName, string parentName, int itemNumber = 0, bool isParentList = false, bool forceOne = false)
        {
            bool allowOne = itemNumber != 1 || forceOne;
            if (parentName == string.Empty)
            {
                if (isParentList && allowOne)
                {
                    return $"{columnName}_{itemNumber}";
                }
                return columnName;
            }
            if (isParentList && allowOne)
            {
                return $"{parentName}_{itemNumber}_{columnName}";
            }
            if (columnName == string.Empty)
            {
                // value of an element
                return parentName;
            }
            return $"{parentName}_{columnName}";
        }
    }
}
