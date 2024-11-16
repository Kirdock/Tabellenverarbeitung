using DataTableConverter.Assisstant;
using DataTableConverter.Extensions;
using DataTableConverter.View.WorkProcViews;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    class ProcPVMExport : WorkProc
    {
        internal static readonly string ClassName = "PVM Export";
        public string SecondFileName;
        public int FileEncoding = 0;
        public SaveFormat Format = SaveFormat.CSV;
        public ProcPVMExport(int ordinal, int id, string name) : base(ordinal, id, name) { }
        public ProcPVMExport(string[] headers, SaveFormat format)
        {
            SetColumns();
            AddColumns(headers);
            Format = format;
        }

        private void AddColumns(string[] headers)
        {
            foreach (string col in headers)
            {
                Columns.Rows.Add(col);
            }
        }

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName)
        {
            PrepareMultiple(GetHeaders(), invokeForm, tableName, out string[] sourceColumns, out _);
            string newTable = null;

            System.Data.SQLite.SQLiteCommand command = invokeForm.DatabaseHelper.SplitTableOnColumnsCommand(sourceColumns, sortingOrder, orderType, tableName);
            if (Properties.Settings.Default.PVMLeadingZero)
            {
                tableName = newTable = invokeForm.DatabaseHelper.CreateTableWithCommand(command);
                command = null;
                Dictionary<string, string> aliasColumnMapping = invokeForm.DatabaseHelper.GetAliasColumnMapping(tableName);
                string leadingZeroColumn = aliasColumnMapping.FirstOrDefault(pair => pair.Key.Equals(Properties.Settings.Default.PVMLeadingZeroAlias, StringComparison.OrdinalIgnoreCase)).Value;
                string leadingZeroText = Properties.Settings.Default.PVMLeadingZeroText;
                if (leadingZeroColumn == null || leadingZeroText.Length == 0)
                {
                    using (PVMImportLeadingZeroForm form = new PVMImportLeadingZeroForm(aliasColumnMapping))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            leadingZeroColumn = form.LeadingZeroColumn;
                            leadingZeroText = form.LeadingZeroText;
                        }
                    }
                }
                invokeForm.DatabaseHelper.ReplaceLeadingZero(leadingZeroColumn, leadingZeroText, tableName);
            }

            string path = Properties.Settings.Default.AutoSavePVM ? Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + Properties.Settings.Default.PVMAddressText + ".csv" : null;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = $"CSV Dateien ({ImportHelper.CsvExt})|{ImportHelper.CsvExt}|Alle Dateien (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };
            Action updateLoadingBar = invokeForm.UpdateLoadingBar;

            DialogResult result = DialogResult.Cancel;
            if (path == null)
            {
                invokeForm.Invoke(new MethodInvoker(() =>
                {
                    result = saveFileDialog1.ShowDialog(invokeForm);
                }));
            }
            if (path != null || result == DialogResult.OK)
            {
                invokeForm.StartLoadingBarCount((Properties.Settings.Default.PVMSaveTwice ? 2 : 1) * invokeForm.DatabaseHelper.GetRowCount(tableName));
                path = path ?? saveFileDialog1.FileName;
                try
                {
                    int fileEncoding = invokeForm.FileEncoding == 0 ? FileEncoding : invokeForm.FileEncoding;
                    //saveTable
                    invokeForm.ExportHelper.Save(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path), Path.GetExtension(filePath), fileEncoding, Format, sortingOrder, orderType, invokeForm, tableName, command, Properties.Settings.Default.PVMSaveTwice ? updateLoadingBar : null);

                    if (Properties.Settings.Default.PVMSaveTwice)
                    {

                        if ((string.IsNullOrWhiteSpace(SecondFileName) || !Directory.Exists(SecondFileName)))
                        {
                            DialogResult result2 = DialogResult.Cancel;
                            if (path == null)
                            {
                                invokeForm.Invoke(new MethodInvoker(() =>
                                {
                                    result2 = saveFileDialog1.ShowDialog(invokeForm);
                                }));
                            }
                            if (result2 == DialogResult.OK)
                            {
                                path = saveFileDialog1.FileName;
                                invokeForm.ExportHelper.Save(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path), Path.GetExtension(filePath), fileEncoding, Format, sortingOrder, orderType, invokeForm, tableName, command, updateLoadingBar);
                            }
                        }
                        else
                        {
                            path = Path.Combine(SecondFileName, Path.GetFileNameWithoutExtension(path));
                            invokeForm.ExportHelper.Save(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path), Path.GetExtension(filePath), fileEncoding, Format, sortingOrder, orderType, invokeForm, tableName, command, updateLoadingBar);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorHelper.LogMessage(ex, invokeForm);
                }
            }
            saveFileDialog1.Dispose();
            if(newTable != null)
            {
                invokeForm.DatabaseHelper.Delete(newTable);
            }
        }

        public override string[] GetHeaders()
        {
            return RemoveEmptyHeaders(Columns.ColumnValuesAsString(0));
        }

        public override void RenameHeaders(string oldName, string newName)
        {
            foreach (DataRow row in Columns.Rows)
            {
                if (row[0].ToString() == oldName)
                {
                    row[0] = newName;
                }
            }
        }

        public override void RemoveHeader(string colName)
        {
            Columns = Columns.AsEnumerable().Where(row => row[0].ToString() != colName).ToTable(Columns);
        }
    }
}
