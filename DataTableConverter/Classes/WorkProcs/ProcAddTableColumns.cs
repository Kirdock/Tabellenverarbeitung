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
            return RemoveEmptyHeaders(new string[] { IdentifySource });
        }

        public ProcAddTableColumns(int ordinal, int id, string name) :base(ordinal, id, name)
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

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName = "main")
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
                dialog.Dispose();
            }
            else
            {
                files.Add(path);
            }
            Dictionary<string, ImportSettings> dict = new Dictionary<string, ImportSettings>();
            ImportSettings setting = invokeForm.ImportHelper.GenerateSettingsThroughPreset(PresetType, SettingPreset);
            string importTables = null;
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

                if (importTables == null)
                {
                    importTables = newTable;
                }
                else
                {
                    invokeForm.DatabaseHelper.ConcatTable(newTable, Path.GetFileName(path), Path.GetFileName(file), importTables);
                }
            }
            DataHelper.StartMerge(importTables, fileEncoding == 0 ? FileEncoding : fileEncoding, filePath, IdentifySource, IdentifyAppend, invalidColumnAlias, invokeForm, tableName);
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
