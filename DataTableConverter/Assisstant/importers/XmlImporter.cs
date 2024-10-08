﻿using DataTableConverter.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace DataTableConverter.Assisstant.importers
{
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
            databaseHelper.CreateTable(new List<string>(), tableName);

            XmlReaderSettings settings = new XmlReaderSettings
            {
                Async = true,
                IgnoreWhitespace = true,
                IgnoreComments = true,
            };
            XmlReader reader = XmlReader.Create(path, settings);
            reader.Read();

            // skip header information
            if(reader.NodeType == XmlNodeType.XmlDeclaration)
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
                LoadRowData(reader, rowData, rowColumns, isArrayDict, overallColumns, databaseHelper, tableName);

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
            mainForm?.SetWorkflowText(string.Empty);
        }

        internal static HashSet<string> LoadRowData(XmlReader rowReader, Dictionary<string, string> rowData, HashSet<string> rowColumns, HashSet<string> isArrayDict, HashSet<string> overallColumns, DatabaseHelper databaseHelper, string tableName, string parentPath = "")
        {
            HashSet<string> newCols = new HashSet<string>();
            string rowElementName = rowReader.LocalName;
            bool isEmptyElement = rowReader.IsEmptyElement;
            if (rowReader.NodeType == XmlNodeType.EndElement)
            {
                return newCols;
            }
            if (rowReader.HasAttributes)
            {
                while(rowReader.MoveToNextAttribute())
                {
                    string columnName = MergeColumnName(rowReader.LocalName, parentPath);
                    rowData[columnName] = rowReader.Value;
                    rowColumns.Add(columnName);
                    newCols.Add(columnName);
                }
                
            }

            if (isEmptyElement)
            {
                return newCols;
            }

            // Read Cells
            string oldParentPath = parentPath;
            int itemNumber = 1;
            string previousElement = null;
            HashSet<string> previousNewCols = new HashSet<string>();

            while (rowReader.Read() && rowReader.LocalName != rowElementName && rowReader.NodeType != XmlNodeType.EndElement)
            {
                bool isArray = isArrayDict.Contains(oldParentPath);
                bool isArrayInternal = previousElement == rowReader.LocalName;
                bool isParentList = isArray || isArrayInternal;
                previousElement = rowReader.LocalName;

                if(isParentList && parentPath == oldParentPath)
                {
                    parentPath = AlignArrayItemsAndPath(oldParentPath, rowElementName, itemNumber, previousNewCols, rowReader, rowData, rowColumns);
                }

                // rename existing or already added rows. It may be that ón the first iteration an array was not seen as an array because it contained only one item
                if (isArrayInternal && !isArray)
                {
                    isArrayDict.Add(oldParentPath);
                    string from = MergeColumnName(rowReader.LocalName, oldParentPath, itemNumber, false);
                    string to = MergeColumnName(rowReader.LocalName, parentPath, itemNumber, true);


                    // if in a previous row an array was not seen as an array and now it is, the previous items need to be adapted
                    if(overallColumns.Contains(from))
                    {
                        databaseHelper.RenameAlias(from, to, tableName);
                    }
                    overallColumns.Remove(from);
                    overallColumns.Add(to);

                    rowColumns.Remove(from);
                    rowColumns.Add(to);

                    newCols.Remove(from);
                    newCols.Add(to);

                    if (rowData.TryGetValue(from, out string value))
                    {
                        rowData.Remove(from);
                        rowData.Add(to, value);
                    }

                }
                string columnName = MergeColumnName(rowReader.LocalName, parentPath, itemNumber, isParentList);
                if (rowReader.NodeType == XmlNodeType.Text)
                {
                    rowData[columnName] = rowReader.Value;
                    rowColumns.Add(columnName);
                    newCols.Add(columnName);
                }
                else
                {
                    previousNewCols = LoadRowData(rowReader, rowData, rowColumns, isArrayDict, overallColumns, databaseHelper, tableName, columnName);
                    newCols.UnionWith(previousNewCols);
                }
                itemNumber++;
            }
            
            if(itemNumber == 1)
            {
                // empty value
                rowData[parentPath] = string.Empty;
                rowColumns.Add(parentPath);
                newCols.Add(parentPath);
            }
            return newCols;
        }

        private static string AlignArrayItemsAndPath(string parentPath, string parentNodeName, int itemNumber, HashSet<string> previousNewCols, XmlReader rowReader, Dictionary<string, string> rowData, HashSet<string> rowColumns)
        {
            int endIndex = parentPath.Length - parentNodeName.Length - 1;
            string newParentPath = endIndex < 0 ? string.Empty : parentPath.Substring(0, endIndex);

            // update the first array element so that it contains "1" and it does not contain the array name
            if (itemNumber == 2)
            {
                foreach (string previousColumnName in previousNewCols)
                {
                    string col = previousColumnName.Substring(parentPath.Length + rowReader.LocalName.Length + 2);
                    string adjustedParentPath = newParentPath + (newParentPath == string.Empty ? string.Empty : "_");
                    string newColumnName = $"{adjustedParentPath}{rowReader.LocalName}_1_{col}";
                    string previousValue = rowData[previousColumnName];

                    rowColumns.Remove(previousColumnName);
                    rowData.Remove(previousColumnName);

                    rowData[newColumnName] = previousValue;
                    rowColumns.Add(newColumnName);
                }
            }
            return newParentPath;
        }

        private static string MergeColumnName(string columnName, string parentName, int itemNumber = 0, bool isParentList = false)
        {
            if (parentName == string.Empty)
            {
                if(isParentList)
                {
                    return $"{columnName}_{itemNumber}";
                }
                return columnName;
            }
            if (isParentList)
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
