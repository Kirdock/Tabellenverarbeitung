using DataTableConverter.Assisstant;
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
        internal string IdentifySource, IdentifyAppend;
        internal bool SaveWhenFinished = true;
        internal bool ImportAll = true;
        internal bool RememberSort;
        internal int SaveFormat;

        public override string[] GetHeaders()
        {
            return new string[] { IdentifySource };
        }

        public override void renameHeaders(string oldName, string newName)
        {
            if (IdentifySource == oldName)
            {
                IdentifySource = newName;
            }
        }

        //public override void GetImportTableInformation()
        //{
        //    ref WorkflowHelper.RemoveEmptyHeaders(DataHelper.HeadersOfDataTable(Columns).Cast<string>());
        //    ref IdentifyAppend;
        //    ref InvalidColumnName;


        //}

        public override void doWork(DataTable table, out string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath)
        {
            //I should first load the File (before workflow.start; right after header-check)
            //additional method in WorkProc and only this class overrides it
            //new SelectDuplicateColumns dialog only for second table

            sortingOrder = string.Empty;
            string path = null;
            string InvalidColumnName = Properties.Settings.Default.InvalidColumnName;
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
                DataTable newTable = ImportHelper.ImportFile(path, null, false, null); //load file
                if (newTable != null) {
                    object[] ImportHeaders = DataHelper.HeadersOfDataTable(newTable);
                    List<string> selectedImportHeaders = DataHelper.HeadersOfDataTable(Columns).Cast<string>().ToList();
                    List<string> notFoundHeaders = new List<string>();
                    string[] importColumns = new string[0];
                    if (!ImportAll)
                    {
                        List<string> list = new List<string>(selectedImportHeaders) { IdentifyAppend, InvalidColumnName };

                        foreach (string header in WorkflowHelper.RemoveEmptyHeaders(list))
                        {
                            if (newTable.Columns.IndexOf(header) == -1)
                            {
                                notFoundHeaders.Add(header);
                            }
                        }
                    }
                    else
                    {
                        if (newTable.Columns.IndexOf(IdentifyAppend) == -1)
                        {
                            notFoundHeaders.Add(IdentifyAppend);
                        }
                        else
                        {
                            importColumns = ImportHeaders.Cast<string>().ToArray();
                        }
                    }

                    if (notFoundHeaders.Count > 0)
                    {
                        SelectDuplicateColumns form = new SelectDuplicateColumns(notFoundHeaders.ToArray(), ImportHeaders);
                        if(form.ShowDialog() == DialogResult.OK)
                        {
                            string[] from = form.Table.Rows.Cast<DataRow>().Select(row => row.ItemArray[0].ToString()).ToArray();
                            string[] to = form.Table.Rows.Cast<DataRow>().Select(row => row.ItemArray[1].ToString()).ToArray();

                            for (int i = 0; i < from.Length; i++)
                            {
                                if(from[i] == InvalidColumnName)
                                {
                                    InvalidColumnName = to[i];
                                }
                                if (from[i] == IdentifyAppend)
                                {
                                    IdentifyAppend = to[i];
                                }
                                int index;
                                if((index = selectedImportHeaders.IndexOf(from[i])) > -1)
                                {
                                    selectedImportHeaders[index] = to[i];
                                }
                            }
                            notFoundHeaders.Clear();
                            if (ImportAll)
                            {
                                importColumns = new List<string>(ImportHeaders.Cast<string>()) { IdentifyAppend }.ToArray();
                            }
                            else
                            {
                                importColumns = selectedImportHeaders.ToArray();
                            }
                        }
                    }
                    if(notFoundHeaders.Count == 0)
                    {
                        DataHelper.AddColumnsOfDataTable(table, newTable, importColumns, table.Columns.IndexOf(IdentifySource), newTable.Columns.IndexOf(IdentifyAppend), RememberSort, NewColumn, null);
                        if (Properties.Settings.Default.SplitPVM)
                        {
                            DataHelper.SplitDataTable(table, path, SaveFormat);
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
            return File.Exists(path);
        }
    }
}
