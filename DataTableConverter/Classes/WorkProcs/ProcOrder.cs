using DataTableConverter.Assisstant;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes.WorkProcs
{
    internal class ProcOrder : WorkProc
    {
        internal static readonly string ClassName = "Sortierung";

        public ProcOrder(int ordinal, int id, int type, string name) : base(ordinal, id, type, name) { }

        public override string[] getHeaders()
        {
            return WorkflowHelper.removeEmptyHeaders(DuplicateColumns);
        }

        public override void renameHeaders(string oldName, string newName)
        {
            return;
        }

        public override void doWork(DataTable table, out string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure)
        {
            StringBuilder builder = new StringBuilder();

            foreach (DataRow row in table.Rows)
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
