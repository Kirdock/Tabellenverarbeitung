using DataTableConverter.Assisstant;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes.WorkProcs
{
    internal class ProcUser : WorkProc
    {

        public override string[] getHeaders()
        {
            return WorkflowHelper.removeEmptyHeaders(Columns.Rows.Cast<DataRow>().Select(dr => dr.ItemArray.Length > 0 ? dr.ItemArray[0].ToString() : null).ToArray());
        }

        public ProcUser(int ordinal, int id, int type, string name) : base(ordinal, id, type, name) { }

        public ProcUser(string[] columns)
        {
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            foreach(string col in columns)
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
            string[] columns = getHeaders();
            sortingOrder = string.Empty;
            bool intoNewCol = false;
            DataTable replaces = procedure.Replace;
            if (!string.IsNullOrWhiteSpace(NewColumn))
            {
                table.Columns.Add(NewColumn);
                intoNewCol = true;
            }
            List<int> headerIndices = DataHelper.getHeaderIndices(table, columns);
            foreach (DataRow rep in replaces.Rows)
            {
                foreach (DataRow row in table.Rows)
                {
                    for (int i = 0; i < row.ItemArray.Length; i++)
                    {

                        if ((columns == null || headerIndices.Contains(i)) && rep.ItemArray[0].ToString().Length > 0)
                        {
                            int index = intoNewCol ? lastCol : i;
                            row.SetField(index, row.ItemArray[i].ToString().Replace(rep.ItemArray[0].ToString(), rep.ItemArray[1].ToString()));
                        }
                    }
                }
            }
        }
    }
}
