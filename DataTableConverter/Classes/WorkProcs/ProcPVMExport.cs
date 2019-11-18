using DataTableConverter.Assisstant;
using DataTableConverter.Extensions;
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
    class ProcPVMExport : WorkProc
    {
        internal static readonly string ClassName = "PVM Export";
        private Action UpdateLoadingBar;
        public string SecondFileName;
        public ProcPVMExport(int ordinal, int id, string name) : base(ordinal, id, name) { }
        public ProcPVMExport(string[] headers, Action updateLoadingBar = null)
        {
            UpdateLoadingBar = updateLoadingBar;
            SetColumns();
            AddColumns(headers);
        }

        private void AddColumns(string[] headers)
        {
            foreach(string col in headers)
            {
                Columns.Rows.Add(col);
            }
        }

        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, out int[] newOrderIndices)
        {
            newOrderIndices = new int[0];
            DataTable saveTable = table.Copy().GetSortedView(sortingOrder, orderType).ToTable();
            IEnumerable<string> sourceColumns = saveTable.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray();
            List<string> destHeaders = new List<string>(GetHeaders());
            foreach (string col in sourceColumns)
            {
                if (!destHeaders.Contains(col))
                {
                    saveTable.Columns.Remove(col);
                }
            }
            string path = Properties.Settings.Default.AutoSavePVM ? Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + Properties.Settings.Default.PVMAddressText + ".csv" : null;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = $"CSV Dateien ({ImportHelper.CsvExt})|{ImportHelper.CsvExt}|Alle Dateien (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            SaveFileDialog saveFileDialog2 = new SaveFileDialog
            {
                Filter = $"CSV Dateien ({ImportHelper.CsvExt})|{ImportHelper.CsvExt}|Alle Dateien (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };
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
                path = path ?? saveFileDialog1.FileName;
                try
                {
                    ExportHelper.ExportCsv(saveTable, Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path), invokeForm.FileEncoding, invokeForm, Properties.Settings.Default.PVMSaveTwice ? UpdateLoadingBar : null);
                    
                    if(Properties.Settings.Default.PVMSaveTwice)
                    {
                        if((string.IsNullOrWhiteSpace(SecondFileName) || !Directory.Exists(SecondFileName)))
                        {
                            DialogResult result2 = DialogResult.Cancel;
                            if (path == null)
                            {
                                invokeForm.Invoke(new MethodInvoker(() =>
                                {
                                    result2 = saveFileDialog2.ShowDialog(invokeForm);
                                }));
                            }
                            if (result2 == DialogResult.OK)
                            {
                                path = saveFileDialog2.FileName;
                                ExportHelper.ExportCsv(saveTable, Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path), invokeForm.FileEncoding, invokeForm, UpdateLoadingBar);
                            }
                        }
                        else
                        {
                            path = Path.Combine(SecondFileName, Path.GetFileNameWithoutExtension(path));
                            ExportHelper.ExportCsv(saveTable, Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path), invokeForm.FileEncoding, invokeForm, UpdateLoadingBar);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorHelper.LogMessage(ex, invokeForm);
                }
            }
            saveFileDialog1.Dispose();
            saveFileDialog2.Dispose();
        }

        public override string[] GetHeaders()
        {
            return WorkflowHelper.RemoveEmptyHeaders(Columns.ColumnValuesAsString(0));
        }

        public override void renameHeaders(string oldName, string newName)
        {
            foreach (DataRow row in Columns.Rows)
            {
                if (row[0].ToString() == oldName)
                {
                    row[0] = newName;
                }
            }
        }

        public override void removeHeader(string colName)
        {
            Columns = Columns.AsEnumerable().Where(row => row[0].ToString() != colName).ToTable(Columns);
        }
    }
}
