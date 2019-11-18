﻿using DataTableConverter.Assisstant;
using DataTableConverter.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    class ProcSeparate : WorkProc
    {
        internal static readonly string ClassName = "Trennen";
        public BindingList<ExportSeparate> Files;
        
        public ProcSeparate(int ordinal, int id, string name) : base(ordinal, id, name) {
            Files = new BindingList<ExportSeparate>();
        }


        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form invokeForm, out int[] newOrderIndices)
        {
            newOrderIndices = new int[0];
            foreach (ExportSeparate item in Files)
            {
                Dictionary<string, DataTable> Dict = new Dictionary<string, DataTable>();
                DataTable tableSkeleton = table.Clone();
                if (item.CheckedAllValues)
                {
                    foreach (string value in table.GroupCountOfColumn(table.Columns.IndexOf(item.Column)).Keys)
                    {
                        DataTable dictTable = tableSkeleton.Copy();
                        dictTable.TableName = $"{item.Name}_{value}";
                        Dict.Add(value, dictTable);
                    }
                }
                else
                {
                    DataTable temp = tableSkeleton.Copy();
                    temp.TableName = item.Name;
                    foreach (string value in item.Values)
                    {
                        if (!Dict.TryGetValue(value, out DataTable tables))
                        {
                            Dict.Add(value, temp);
                        }
                    }
                }
                foreach (DataRow row in table.Rows)
                {
                    if (Dict.TryGetValue(row[item.Column].ToString(), out DataTable dictTable))
                    {
                        dictTable.ImportRow(row);
                    }
                }

                foreach (DataTable dictTable in Dict.Values.Distinct())
                {
                    string FileName = dictTable.TableName;
                    string path = Path.GetDirectoryName(filePath);
                    switch (item.Format)
                    {
                        //CSV
                        case 0:
                            {
                                ExportHelper.ExportCsv(dictTable, path, FileName);
                            }
                            break;

                        //Dbase
                        case 1:
                            {
                                ExportHelper.ExportDbase(FileName, dictTable, path, invokeForm);
                            }
                            break;

                        //Excel
                        case 2:
                            {
                                ExportHelper.ExportExcel(dictTable, path, FileName, invokeForm);
                            }
                            break;
                    }
                }
            }
        }

        public override string[] GetHeaders()
        {
            return WorkflowHelper.RemoveEmptyHeaders(Files.Select(file => file.Column));
        }

        public override void renameHeaders(string oldName, string newName)
        {
            foreach (ExportSeparate file in Files)
            {
                if (file.Column == oldName)
                {
                    file.Column = newName;
                }
            }
        }

        public override void removeHeader(string colName)
        {
            Files = new BindingList<ExportSeparate>(Files.Where(file => file.Column != colName).ToList());
        }
    }
}
