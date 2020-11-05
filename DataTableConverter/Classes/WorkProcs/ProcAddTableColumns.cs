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
        public string SettingPreset;
        public int PresetType;
        public int FileEncoding = 0;
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

        public override void RenameHeaders(string oldName, string newName)
        {
            if (IdentifySource == oldName)
            {
                IdentifySource = newName;
            }
        }

        public override void RemoveHeader(string colName)
        {
            IdentifySource = null;
        }

        public override void DoWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, out int[] newOrderIndices)
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
            List<string> files = new List<string>();
            string invalidColumnName = Properties.Settings.Default.InvalidColumnName;
            if (!CheckFile(filePath, ref path)) //find file
            {
                OpenFileDialog dialog = ImportHelper.GetOpenFileDialog(true);
                DialogResult res = DialogResult.Cancel;
                invokeForm.Invoke(new MethodInvoker(() =>
                {
                    res = dialog.ShowDialog(invokeForm);
                }));
                if (res == DialogResult.OK)
                {
                    files.AddRange(dialog.FileNames);
                }
                dialog.Dispose();
            }
            else
            {
                files.Add(path);
            }
            Dictionary<string, ImportSettings> dict = new Dictionary<string, ImportSettings>();
            ImportSettings setting = ImportHelper.GenerateSettingsThroughPreset(PresetType, SettingPreset);
            DataTable importTables = null;
            int fileEncoding = 0;

            foreach (string file in files)
            {
                if (setting != null)
                {
                    if (!dict.ContainsKey(file))
                    {
                        dict.Add(Path.GetExtension(file).ToLower(), setting);
                    }
                    fileEncoding = setting.CodePage;
                }

                //DataTable newTable = ImportHelper.ImportFile(file, true, dict, ctxRow, null, invokeForm, ref fileEncoding); //load file


                ////Try to outsorce the method StartMerge in Form1
                //if(ImportHelper.ValidateImport(newTable, invokeForm, ref IdentifyAppend, ref invalidColumnName))
                //{
                //    if (importTables == null)
                //    {
                //        importTables = newTable;
                //    }
                //    else
                //    {
                //        importTables.ConcatTable(newTable, Path.GetFileName(path), Path.GetFileName(file));
                //    }
                //}

            }
            //string[] importColumns = new string[0];
            //int sourceMergeIndex = -1;
            //int importMergeIndex = -1;
            //DialogResult result = DialogResult.No;

            //if ((sourceMergeIndex = table.Columns.IndexOf(IdentifyAppend)) > -1)
            //{
            //    if ((importMergeIndex = importTables.Columns.IndexOf(IdentifyAppend)) > -1)
            //    {
            //        importColumns = importTables.HeadersOfDataTableAsString();
            //        result = DialogResult.Yes;
            //        if (table.Rows.Count != importTables.Rows.Count)
            //        {
            //            result = invokeForm.MessagesYesNo(MessageBoxIcon.Warning, $"Die Zeilenanzahl der beiden Tabellen stimmt nicht überein ({table.Rows.Count} zu {importTables.Rows.Count})!\nTrotzdem fortfahren?");
            //        }
            //    }
            //    else
            //    {
            //        invokeForm.MessagesOK(MessageBoxIcon.Warning, $"Die zu importierende Tabellen haben keine Spalte mit der Bezeichnung {IdentifyAppend}");
            //        result = Form1.ShowMergeForm(ref importColumns, ref sourceMergeIndex, ref importMergeIndex, table, importTables, string.Empty, invokeForm);
            //    }
            //}
            //else
            //{
            //    invokeForm.MessagesOK(MessageBoxIcon.Warning, $"Die Haupttabelle hat keine Spalte mit der Bezeichnung {IdentifyAppend}");
            //    result = Form1.ShowMergeForm(ref importColumns, ref sourceMergeIndex, ref importMergeIndex, table, importTables, string.Empty, invokeForm);
            //}

            //if (result != DialogResult.No)
            //{
            //    table.AddColumnsOfDataTable(importTables, importColumns, table.Columns.IndexOf(IdentifySource), importTables.Columns.IndexOf(IdentifyAppend), out int[] newIndices, null);
            //    newOrderIndices = newIndices;
            //    if (Properties.Settings.Default.SplitPVM)
            //    {
            //        int count = table.SplitDataTable(filePath, invokeForm, fileEncoding == 0 ? FileEncoding : fileEncoding, invalidColumnName);
            //        if (count != 0 && !invokeForm.IsDisposed)
            //        {
            //            invokeForm.Invoke(new MethodInvoker(() =>
            //            {
            //                invokeForm.ValidRows = count;
            //            }));
            //        }
            //    }
            //    foreach (DataRow row in table.AsEnumerable().Where(row => row.RowState != DataRowState.Deleted && row[invalidColumnName].ToString() == Properties.Settings.Default.FailAddressValue))
            //    {
            //        row.Delete();
            //    }
            //}
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
