using DataTableConverter.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DataTableConverter.Assisstant.importers
{
    internal static class XmlImporter
    {

        internal static void Import(string path, DatabaseHelper databaseHelper, ProgressBar progressBar, Form mainForm, string tableName)
        {
            XDocument doc = XDocument.Load(path);
            HashSet<string> overallColumns = new HashSet<string>();
            databaseHelper.CreateTable(new List<string>(), tableName);
            progressBar?.StartLoadingBar(doc.Root.Elements().Count(), mainForm);
            SQLiteCommand insertCommand = null;
            Dictionary<string, string> rowData = new Dictionary<string, string>();
            

            foreach (var row in doc.Root.Elements())
            {
                // revert everything to empty string and keep columns, so that the SQL command does not need to be re-created and can be optimized
                foreach (var key in rowData.Keys.ToList()) // two list because on enumerable there can't be edits in a foreach
                {
                    rowData[key] = string.Empty;
                }

                HashSet<string> rowColumns = new HashSet<string>();
                LoadRowData(row, rowData, rowColumns);

                foreach (var columnName in rowColumns)
                {
                    bool added = overallColumns.Add(columnName);
                    if (added)
                    {
                        databaseHelper.AddColumn(tableName, columnName);
                    }
                }

                insertCommand = databaseHelper.InsertRow(rowData, tableName, insertCommand);
                progressBar?.UpdateLoadingBar(mainForm);
            }
        }

        internal static void LoadRowData(XElement row, Dictionary<string, string> rowData, HashSet<string> columns, string parentName = "")
        {
            foreach (var attribute in row.Attributes())
            {
                string columnName = MergeColumnName(attribute.Name.LocalName, parentName);
                rowData[columnName] = attribute.Value;
                columns.Add(columnName);
            }

            bool isParentList = row.Elements(row.Elements().First().Name.LocalName).Count() > 1;
            int itemNumber = 1;
            HashSet<string> itemNames = new HashSet<string>();
            foreach (var itemAttribute in row.Elements())
            {
                string columnName = MergeColumnName(itemAttribute.Name.LocalName, parentName, itemNumber, isParentList);
                if (itemAttribute.HasElements)
                {
                    LoadRowData(itemAttribute, rowData, columns, columnName);
                }
                else
                {
                    rowData[columnName] = itemAttribute.Value;
                    columns.Add(columnName);
                }
                itemNumber++;
            }
        }

        private static string MergeColumnName(string columnName, string parentName, int itemNumber = 0, bool isParentList = false)
        {
            if (parentName == string.Empty)
            {
                return columnName;
            }
            if (isParentList)
            {
                return $"{parentName} {itemNumber} {columnName}";
            }
            return $"{parentName} {columnName}";
        }
    }
}
