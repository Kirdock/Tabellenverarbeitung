using DataTableConverter.View;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DataTableConverter.Assisstant
{
    class DataHelper
    {

        internal static int StartMerge(string importTable, int encoding, string filePath, string sourceIdentifierColumnName, string importIdentifierColumnName, string invalidColumnAlias, Form1 invokeForm, string tableName)
        {
            string[] importColumnNames = new string[0];
            string filename = System.IO.Path.GetFileNameWithoutExtension(filePath);
            int count = 0;
            DialogResult result = DialogResult.No;
            int importRowCount = invokeForm.DatabaseHelper.GetRowCount(importTable);
            int originalRowCount = invokeForm.DatabaseHelper.GetRowCount(tableName);
            Dictionary<string, string> importTableColumnAliasMapping = invokeForm.DatabaseHelper.GetAliasColumnMapping(importTable);
            Dictionary<string, string> originalTableColumnAliasMapping = invokeForm.DatabaseHelper.GetAliasColumnMapping(tableName);

            if (string.IsNullOrWhiteSpace(sourceIdentifierColumnName) || string.IsNullOrWhiteSpace(importIdentifierColumnName))
            {
                result = ShowMergeForm(ref importColumnNames, ref sourceIdentifierColumnName, ref importIdentifierColumnName, originalTableColumnAliasMapping, originalRowCount, importTableColumnAliasMapping, importRowCount, filename, invokeForm);
            }
            else
            {
                string res = originalTableColumnAliasMapping.FirstOrDefault(pair => pair.Key.Equals(sourceIdentifierColumnName, System.StringComparison.OrdinalIgnoreCase)).Value;
                if (res!= null)
                {
                    sourceIdentifierColumnName = res;
                    res = importTableColumnAliasMapping.FirstOrDefault(pair => pair.Key.Equals(importIdentifierColumnName, System.StringComparison.OrdinalIgnoreCase)).Value;
                    if (res != null)
                    {
                        importIdentifierColumnName = res;

                        importColumnNames = importTableColumnAliasMapping.Values.Cast<string>().ToArray();
                        result = DialogResult.Yes;
                        if (originalRowCount != importRowCount)
                        {
                            result = MessageHandler.MessagesYesNo(invokeForm, MessageBoxIcon.Warning, $"Die Zeilenanzahl der beiden Tabellen stimmt nicht überein ({originalRowCount} zu {importRowCount })!\nTrotzdem fortfahren?");
                        }
                    }
                    else
                    {
                        MessageHandler.MessagesOK(invokeForm, MessageBoxIcon.Warning, $"Die zu importierende Tabelle hat keine Spalte mit der Bezeichnung {importIdentifierColumnName}");
                        result = ShowMergeForm(ref importColumnNames, ref sourceIdentifierColumnName, ref importIdentifierColumnName, originalTableColumnAliasMapping, originalRowCount, importTableColumnAliasMapping, importRowCount, filename, invokeForm);
                    }
                }
                else
                {
                    MessageHandler.MessagesOK(invokeForm, MessageBoxIcon.Warning, $"Die Haupttabelle hat keine Spalte mit der Bezeichnung {sourceIdentifierColumnName}");
                    result = ShowMergeForm(ref importColumnNames, ref sourceIdentifierColumnName, ref importIdentifierColumnName, originalTableColumnAliasMapping, originalRowCount, importTableColumnAliasMapping, importRowCount, filename, invokeForm);
                }
            }

            if (result == DialogResult.Yes)
            {
                string invalidColumnName = importTableColumnAliasMapping.FirstOrDefault(pair => pair.Key.Equals(invalidColumnAlias, System.StringComparison.OrdinalIgnoreCase)).Value;
                if (invalidColumnName == null || !importColumnNames.Contains(invalidColumnName))
                {
                    SelectDuplicateColumns f = new SelectDuplicateColumns(new string[] { invalidColumnAlias }, importTableColumnAliasMapping, true);
                    DialogResult res = DialogResult.Cancel;
                    invokeForm.Invoke(new MethodInvoker(() =>
                    {
                        res = f.ShowDialog(invokeForm);
                    }));
                    if (res == DialogResult.OK)
                    {
                        invalidColumnName = f.Table.AsEnumerable().First()[1].ToString();
                    }
                }

                bool abort = invokeForm.DatabaseHelper.PVMImport(importTable, importColumnNames, sourceIdentifierColumnName, importIdentifierColumnName, tableName, invokeForm, out string orderColumn);

                if (abort) return 0;

                invokeForm.DatabaseHelper.ApplyOrder(orderColumn, tableName);
                invokeForm.DatabaseHelper.DeleteColumn(orderColumn, tableName);

                if (Properties.Settings.Default.SplitPVM)
                {
                    count = invokeForm.DatabaseHelper.PVMSplit(filePath, invokeForm, encoding, invalidColumnName, string.Empty, Classes.OrderType.Windows, tableName);
                }
                invokeForm.DatabaseHelper.DeleteInvalidRows(tableName, invalidColumnName);
            }
            return count;
        }

        private static DialogResult ShowMergeForm(ref string[] importColumns, ref string sourceColumnName, ref string importColumnName, Dictionary<string, string> originalTableHeaders, int originalRowCount, Dictionary<string, string> importTableHeaders, int importRowCount, string filename, Form invokeForm)
        {
            bool result;
            using (MergeTable form = new MergeTable(originalTableHeaders, importTableHeaders, filename, originalRowCount, importRowCount))
            {
                DialogResult res = DialogResult.Cancel;
                invokeForm.Invoke(new MethodInvoker(() =>
                {
                    res = form.ShowDialog(invokeForm);
                }));
                if (result = (res == DialogResult.OK))
                {
                    importColumns = form.SelectedColumns.ToArray();
                    sourceColumnName = form.OriginalIdentifierColumnName;
                    importColumnName = form.ImportIdentifierColumnName;
                }
            }
            return result ? DialogResult.Yes : DialogResult.No;
        }
    }
}
