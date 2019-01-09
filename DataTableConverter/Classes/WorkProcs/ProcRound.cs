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
    class ProcRound : WorkProc
    {
        internal static readonly string ClassName = "Runden";
        internal int Decimals;

        public override string[] GetHeaders()
        {
            return WorkflowHelper.RemoveEmptyHeaders(Columns.Rows.Cast<DataRow>().Select(dr => dr.ItemArray.Length > 0 ? dr.ItemArray[0].ToString() : null).ToArray());
        }

        public ProcRound(int ordinal, int id, string name) : base(ordinal, id, name) { }

        public ProcRound(string[] columns, int decimals, string newColumn)
        {
            Decimals = decimals;
            NewColumn = newColumn;
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            foreach (string col in columns)
            {
                Columns.Rows.Add(col);
            }
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
            int lastCol = table.Columns.Count;
            string[] columns = GetHeaders();
            sortingOrder = string.Empty;
            bool intoNewCol = false;
            
            if (!string.IsNullOrWhiteSpace(NewColumn))
            {
                table.Columns.Add(NewColumn);
                intoNewCol = true;
            }
            List<int> headerIndices = DataHelper.getHeaderIndices(table, columns);
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < row.ItemArray.Length; i++)
                {

                    if (columns == null || headerIndices.Contains(i))
                    {
                        int index = intoNewCol ? lastCol : i;
                        if (float.TryParse(row.ItemArray[i].ToString(), out float result))
                        {
                            row.SetField(index, Math.Round( result, Decimals,MidpointRounding.AwayFromZero));
                        }
                    }
                }
            }
        }
    }
}
