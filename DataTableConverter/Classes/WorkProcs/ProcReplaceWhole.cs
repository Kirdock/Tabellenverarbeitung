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
    class ProcReplaceWhole : WorkProc
    {
        internal string ReplaceText;
        internal static readonly string ClassName = "Text ersetzen";

        public ProcReplaceWhole(string[] columns, string replaceText)
        {
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            foreach (string col in columns)
            {
                Columns.Rows.Add(col);
            }
            ReplaceText = replaceText;
        }


        public override string[] GetHeaders()
        {
            return WorkflowHelper.RemoveEmptyHeaders(Columns.Rows.Cast<DataRow>().Select(dr => dr.ItemArray.Length > 0 ? dr.ItemArray[0].ToString() : null));
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
            string[] columns = GetHeaders();
            sortingOrder = string.Empty;

            List<int> headerIndices = DataHelper.HeaderIndices(table, columns);
            foreach (DataRow row in table.Rows)
            {
                foreach (int i in headerIndices)
                {
                    row[i] = ReplaceText;
                }
            }
        }
    }
}
