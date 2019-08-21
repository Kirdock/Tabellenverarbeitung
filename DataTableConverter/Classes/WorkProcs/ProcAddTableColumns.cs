using DataTableConverter.Assisstant;
using DataTableConverter.Extensions;
using DataTableConverter.View;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    class ProcAddTableColumns : WorkProc
    {
        public string IdentifySource, IdentifyAppend;
        internal static string ClassName = "PVM Import";
        //internal bool ImportAll = true;

        public override string[] GetHeaders()
        {
            return WorkflowHelper.RemoveEmptyHeaders(new string[] { IdentifySource });
        }

        public ProcAddTableColumns(int ordinal, int id, string name) :base(ordinal, id, name)
        {
            ReplacesTable = true;
        }

        public override void renameHeaders(string oldName, string newName)
        {
            if (IdentifySource == oldName)
            {
                IdentifySource = newName;
            }
        }

        public override void removeHeader(string colName)
        {
            IdentifySource = null;
        }

        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form invokeForm, out int[] newOrderIndices)
        {
            //I should first load the File (before workflow.start; right after header-check)
            //additional method in WorkProc and only this class overrides it
            //new SelectDuplicateColumns dialog only for second table
            newOrderIndices = new int[0];

            if (string.IsNullOrWhiteSpace(IdentifyAppend) || string.IsNullOrWhiteSpace(IdentifySource))
            {
                return;
            }
            string path = null;
            string invalidColumnName = Properties.Settings.Default.InvalidColumnName;
            if (!CheckFile(filePath, ref path)) //find file
            {
                OpenFileDialog dialog = ImportHelper.GetOpenFileDialog(false);
                if(dialog.ShowDialog() == DialogResult.OK)
                {
                    path = dialog.FileName;
                }
            }
            if (!string.IsNullOrWhiteSpace(path))
            {
                DataTable newTable = ImportHelper.ImportFile(path, null, false, null, ctxRow); //load file
                if (newTable != null) {
                    object[] ImportHeaders = newTable.HeadersOfDataTable();
                    List<string> notFoundHeaders = new List<string>();
                    string[] importColumns = new string[0];
                    if (!newTable.Columns.Contains(IdentifyAppend))
                    {
                        notFoundHeaders.Add(IdentifyAppend);
                    }
                    else if (!newTable.Columns.Contains(invalidColumnName))
                    {
                        notFoundHeaders.Add(invalidColumnName);
                    }
                    else
                    {
                        importColumns = ImportHeaders.Cast<string>().ToArray();
                    }

                    if (notFoundHeaders.Count > 0)
                    {
                        SelectDuplicateColumns form = new SelectDuplicateColumns(notFoundHeaders.ToArray(), ImportHeaders, true)
                        {
                            Text = "Folgende Spalten der zu importierenden Tabelle wurden nicht gefunden"
                        };
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            string[] from = form.Table.Rows.Cast<DataRow>().Select(row => row.ItemArray[0].ToString()).ToArray();
                            string[] to = form.Table.Rows.Cast<DataRow>().Select(row => row.ItemArray[1].ToString()).ToArray();

                            for (int i = 0; i < from.Length; i++)
                            {
                                if(from[i] == invalidColumnName)
                                {
                                    invalidColumnName = to[i];
                                }
                                if (from[i] == IdentifyAppend)
                                {
                                    IdentifyAppend = to[i];
                                }
                            }
                            notFoundHeaders.Clear();
                            importColumns = ImportHeaders.Cast<string>().ToArray();
                        }
                    }
                    if(notFoundHeaders.Count == 0)
                    {
                        table.AddColumnsOfDataTable(newTable, importColumns, table.Columns.IndexOf(IdentifySource), newTable.Columns.IndexOf(IdentifyAppend), out int[] newIndices, null);
                        newOrderIndices = newIndices;
                        if (Properties.Settings.Default.SplitPVM)
                        {
                            table.SplitDataTable(path, invalidColumnName);
                        }
                    }
                }
            }
        }

        internal static bool CheckFile(string filePath, ref string path)
        {
            path = Path.Combine(
                Path.GetDirectoryName(filePath),
                Path.GetFileNameWithoutExtension(filePath) + Properties.Settings.Default.PVMAddressText + Path.GetExtension(filePath));
            return Properties.Settings.Default.PVMAddressText != string.Empty && File.Exists(path);
        }
    }
}
