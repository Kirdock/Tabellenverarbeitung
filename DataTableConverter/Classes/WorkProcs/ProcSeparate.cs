﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    class ProcSeparate : WorkProc
    {
        internal static readonly string ClassName = "Trennen";
        public bool ContinuedColumn;
        public BindingList<ExportSeparate> Files;

        public ProcSeparate(int ordinal, int id, string name) : base(ordinal, id, name)
        {
            Files = new BindingList<ExportSeparate>();
        }


        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName)
        {
            NewColumn = string.IsNullOrEmpty(NewColumn) ? "FTNR" : NewColumn;
            List<string> columnsAliases = invokeForm.DatabaseHelper.GetSortedColumnsAsAlias(tableName);

            foreach (ExportSeparate item in Files)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                string columnName = invokeForm.DatabaseHelper.GetColumnName(item.Column, tableName);

                if (item.CheckedAllValues)
                {
                    foreach (string value in invokeForm.DatabaseHelper.GroupCountOfColumn(columnName, tableName).Keys)
                    {
                        string newTable = $"{item.Name}_{value}";
                        invokeForm.DatabaseHelper.CreateTable(columnsAliases, newTable);
                        dict.Add(value, newTable);
                    }
                }
                else if (item.SaveRemaining)
                {
                    string newTable = item.Name;
                    invokeForm.DatabaseHelper.CreateTable(columnsAliases, newTable);
                    foreach (string value in invokeForm.DatabaseHelper.GroupCountOfColumn(columnName, tableName).Keys.Where(key => !Files.Any(file => file.Column == columnName && file.Values.Contains(key))))
                    {
                        if (!dict.ContainsKey(value))
                        {
                            dict.Add(value, newTable);
                        }
                    }
                }
                else
                {
                    string newTable = item.Name;
                    invokeForm.DatabaseHelper.CreateTable(columnsAliases, newTable);
                    foreach (string value in item.Values)
                    {
                        if (!dict.ContainsKey(value))
                        {
                            dict.Add(value, newTable);
                        }
                    }
                }


                SQLiteCommand command = invokeForm.DatabaseHelper.GetDataCommand(tableName, sortingOrder, orderType);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        int columnIndex;
                        for (columnIndex = 0; columnIndex < reader.FieldCount && reader.GetName(columnIndex) != item.Column; columnIndex++) { }
                        SQLiteCommand tempCommand = null;
                        while (reader.Read())
                        {
                            if (dict.TryGetValue(reader.GetString(columnIndex), out string tempTable))
                            {
                                tempCommand = invokeForm.DatabaseHelper.InsertRow(columnsAliases, reader, tempTable, tempCommand);
                            }
                        }
                    }
                }

                foreach (string tempTable in dict.Values.Distinct())
                {
                    if (ContinuedColumn)
                    {
                        string destinationColumn = invokeForm.DatabaseHelper.AddColumnAt(0, NewColumn, tempTable);
                        invokeForm.DatabaseHelper.Enumerate(destinationColumn, -1, -1, false, sortingOrder, orderType, tempTable);
                    }

                    string path = Path.GetDirectoryName(filePath);
                    invokeForm.ExportHelper.Save(path, tempTable, Path.GetExtension(filePath), invokeForm.FileEncoding, item.Format, sortingOrder, orderType, invokeForm, tableName);
                }
            }
        }

        public override string[] GetHeaders()
        {
            return RemoveEmptyHeaders(Files.Select(file => file.Column));
        }

        public override void RenameHeaders(string oldName, string newName)
        {
            foreach (ExportSeparate file in Files)
            {
                if (file.Column == oldName)
                {
                    file.Column = newName;
                }
            }
        }

        public override void RemoveHeader(string colName)
        {
            Files = new BindingList<ExportSeparate>(Files.Where(file => file.Column != colName).ToList());
        }
    }
}
