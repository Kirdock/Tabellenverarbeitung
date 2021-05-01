using DataTableConverter.Assisstant;
using System;
using System.Collections.Generic;
using System.IO;
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
            return RemoveEmptyHeaders(new string[] { IdentifySource });
        }

        public ProcAddTableColumns(int ordinal, int id, string name) : base(ordinal, id, name)
        {

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

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName)
        {
            if (string.IsNullOrWhiteSpace(IdentifyAppend) || string.IsNullOrWhiteSpace(IdentifySource))
            {
                return;
            }
            string path = null;

            List<string> files = new List<string>();
            string invalidColumnAlias = Properties.Settings.Default.InvalidColumnName;
            if (!CheckFile(filePath, ref path)) //find file
            {
                OpenFileDialog dialog = invokeForm.ImportHelper.GetOpenFileDialog(true);
                DialogResult res = DialogResult.Cancel;
                invokeForm.Invoke(new MethodInvoker(() =>
                {
                    res = dialog.ShowDialog(invokeForm);
                }));
                if (res == DialogResult.OK)
                {
                    files.AddRange(dialog.FileNames);
                }
                else
                {
                    return;
                }
                dialog.Dispose();
            }
            else
            {
                files.Add(path);
            }
            Dictionary<string, ImportSettings> dict = new Dictionary<string, ImportSettings>();
            ImportSettings setting = invokeForm.ImportHelper.GenerateSettingsThroughPreset(PresetType, SettingPreset);
            string importTable = null;
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

                string newTable = invokeForm.ImportHelper.ImportFile(file, true, dict, ctxRow, null, invokeForm, ref fileEncoding); //load file

                if (importTable == null)
                {
                    importTable = newTable;
                }
                else
                {
                    invokeForm.DatabaseHelper.ConcatTable(importTable, newTable, Path.GetFileName(path), Path.GetFileName(file));
                }
            }
            if (importTable != null)
            {
                DataHelper.StartMerge(importTable, fileEncoding == 0 ? FileEncoding : fileEncoding, filePath, IdentifySource, IdentifyAppend, invalidColumnAlias, sortingOrder, orderType, invokeForm, tableName);
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
