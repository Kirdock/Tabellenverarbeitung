using DataTableConverter.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    class ProcReplaceWhole : WorkProc
    {
        internal static readonly string ClassName = "Text ersetzen";
        internal enum ColumnIndex : int { Column = 0, Value = 1 };

        public ProcReplaceWhole(int ordinal, int id, string name) : base(ordinal, id, name)
        {
            InitColumns();
        }

        public ProcReplaceWhole(DataTable table)
        {
            InitColumns();
            foreach (DataRow row in table.Rows)
            {
                Columns.Rows.Add(new object[] { row[(int)ColumnIndex.Column].ToString(), row[(int)ColumnIndex.Value].ToString() });
            }
        }

        private void InitColumns()
        {
            Columns = new DataTable { TableName = "Columnnames" };
            SetColumns(Columns);
        }

        internal static void SetColumns(DataTable table)
        {
            table.Columns.Add("Spalten", typeof(string));
            table.Columns.Add("Text", typeof(string));
        }


        public override string[] GetHeaders()
        {
            return RemoveEmptyHeaders(Columns.AsEnumerable().Select(row => row[(int)ColumnIndex.Column].ToString()));
        }

        private IEnumerable<DataRow> GetFoundRows(List<string> columns)
        {
            return Columns.AsEnumerable().Where(dr => dr.ItemArray.Length > 0 && dr[0].ToString().Length != 0 && columns.Contains(dr[(int)ColumnIndex.Column].ToString(), StringComparer.OrdinalIgnoreCase));
        }

        public override void RenameHeaders(string oldName, string newName)
        {
            foreach (DataRow row in Columns.Rows)
            {
                if (row.ItemArray[(int)ColumnIndex.Column].ToString() == oldName)
                {
                    row.SetField((int)ColumnIndex.Column, newName);
                }
            }
        }

        public override void RemoveHeader(string colName)
        {
            Columns = Columns.AsEnumerable().Where(row => row[(int)ColumnIndex.Column].ToString() != colName).ToTable(Columns);
        }

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName)
        {
            DataRow[] distinctDataTale = GetFoundRows(invokeForm.DatabaseHelper.GetSortedColumnsAsAlias(tableName)).ToArray();
            if (distinctDataTale.Length != 0)
            {
                invokeForm.DatabaseHelper.ReplaceColumnValues(distinctDataTale, tableName);
            }
        }
    }
}
