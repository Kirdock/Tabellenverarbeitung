using DataTableConverter.Assisstant;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    internal class ProcOrder : WorkProc
    {
        internal static readonly string ClassName = "Sortierung";

        public ProcOrder(int ordinal, int id, string name) : base(ordinal, id, name) { }

        public override string[] GetHeaders()
        {
            return WorkflowHelper.RemoveEmptyHeaders(DuplicateColumns);
        }

        public override void renameHeaders(string oldName, string newName)
        {
            foreach (DataRow row in Columns.Rows)
            {
                if (row.ItemArray[0].ToString() == oldName)
                {
                    row.SetField(0, newName);
                }
            }
        }

        public override void doWork(DataTable table, out string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure)
        {
            StringBuilder builder = new StringBuilder();

            foreach (DataRow row in Columns.Rows)
            {
                object col = row[0];
                bool orderDESC = string.IsNullOrWhiteSpace(row[1]?.ToString()) ? false : (bool)row[1];
                builder.Append("[").Append(col.ToString()).Append("] ").Append(orderDESC ? "DESC" : "ASC").Append(", ");
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
