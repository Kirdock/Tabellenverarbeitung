using DataTableConverter.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    class ProcMergeRows : WorkProc
    {
        internal static readonly string ClassName = "Zeilen zusammenfassen";
        public string Identifier;
        public bool Separator;

        public ProcMergeRows(int ordinal, int id, string name) : base(ordinal, id, name)
        {
            Columns = new DataTable { TableName = "MergeRows" };
            Columns.Columns.Add("Spalte", typeof(string));
            Columns.Columns.Add("Aktion", typeof(PlusListboxItem.RowMergeState));
        }

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName)
        {
            if (!string.IsNullOrWhiteSpace(Identifier))
            {
                string column = invokeForm.DatabaseHelper.GetColumnName(Identifier, tableName);
                List<PlusListboxItem> additionalColumns = Columns.AsEnumerable().Select(row => new PlusListboxItem((PlusListboxItem.RowMergeState)Enum.ToObject(typeof(PlusListboxItem.RowMergeState), row[1] as int? ?? 0), row[0].ToString())).ToList();
                additionalColumns.ForEach(item => item.Value = invokeForm.DatabaseHelper.GetColumnName(item.Value, tableName));
                invokeForm.DatabaseHelper.MergeRows(column, additionalColumns, Separator, invokeForm, null, tableName);
            }
        }

        public override string[] GetHeaders()
        {
            return RemoveEmptyHeaders(Columns.AsEnumerable().Select(row => row[0].ToString()).Concat(new string[] { Identifier }).Distinct());
        }

        public override void RenameHeaders(string oldName, string newName)
        {
            foreach (DataRow row in Columns.Rows)
            {
                if (row.ItemArray[0].ToString() == oldName)
                {
                    row.SetField(0, newName);
                }
            }
        }

        public override void RemoveHeader(string colName)
        {
            Columns = Columns.AsEnumerable().Where(row => row[0].ToString() != colName).ToTable(Columns);
            if (Identifier == colName)
            {
                Identifier = string.Empty;
            }
        }
    }
}
