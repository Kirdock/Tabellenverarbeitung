using System;
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
            IEnumerable<ExportCustomItem> items = Files.Select(item =>
            {
                string columnName = invokeForm.DatabaseHelper.GetColumnName(item.Column, tableName);
                IEnumerable<string> values;
                if (item.CheckedAllValues)
                {
                    values = invokeForm.DatabaseHelper.GroupCountOfColumn(columnName, tableName).Keys;
                }
                else if (item.SaveRemaining)
                {
                    values = invokeForm.DatabaseHelper.GroupCountOfColumn(columnName, tableName).Keys.Where(key => !Files.Any(file => file.Column == columnName && file.Values.Contains(key)));
                }
                else
                {
                    values = item.Values;
                }
                return new ExportCustomItem(item.Name, columnName, values, item.Format, item.CheckedAllValues || item.SaveRemaining);
            });

            invokeForm.ExportHelper.ExportTableWithColumnCondition(items, filePath, invokeForm.FileEncoding, sortingOrder, orderType, invokeForm, ContinuedColumn ? NewColumn : null, tableName, invokeForm.SetWorkflowText);
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
