using DataTableConverter.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    internal class ProcOrder : WorkProc
    {
        internal static readonly string ClassName = "Sortierung";

        public ProcOrder(int ordinal, int id, string name) : base(ordinal, id, name) { }

        public override string[] GetHeaders()
        {
            return RemoveEmptyHeaders(Columns.ColumnValuesAsString(0));
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
        }

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName)
        {
            StringBuilder builder = new StringBuilder();

            foreach (DataRow row in Columns.Rows)
            {
                string alias = row[0].ToString();
                bool orderDESC = string.IsNullOrWhiteSpace(row[1]?.ToString()) ? false : (bool)row[1];
                builder.Append("[").Append(invokeForm.DatabaseHelper.GetColumnName(alias, tableName)).Append("] COLLATE NATURALSORT ").Append(orderDESC ? "DESC" : "ASC").Append(", ");
            }
            string result = builder.ToString();
            if (result.Length > 2)
            {
                result = result.Substring(0, builder.Length - 2);
            }
            sortingOrder = result;
        }

    }
}
