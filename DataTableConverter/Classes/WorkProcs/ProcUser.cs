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
    internal class ProcUser : WorkProc
    {

        public override string[] GetHeaders()
        {
            return WorkflowHelper.RemoveEmptyHeaders(Columns.Rows.Cast<DataRow>().Select(dr => dr.ItemArray.Length > 0 ? dr.ItemArray[0].ToString() : null).ToArray());
        }

        public ProcUser(int ordinal, int id,string name) : base(ordinal, id, name) { }

        public ProcUser(string[] columns, string header)
        {
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            foreach(string col in columns)
            {
                Columns.Rows.Add(col);
            }
            NewColumn = header;
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
            DataTable replaces = procedure.Replace;
            if (!string.IsNullOrWhiteSpace(NewColumn))
            {
                table.Columns.Add(NewColumn);
                intoNewCol = true;
            }
            List<int> headerIndices = DataHelper.getHeaderIndices(table, columns);
            foreach (DataRow row in table.Rows)
            {
                foreach (int i in headerIndices)
                {
                    foreach (DataRow rep in replaces.Rows)
                    {
                        int index = intoNewCol ? lastCol : i;
                        if (row[i].ToString() == rep[0].ToString())
                        {
                            row[index] = rep[1].ToString();
                        }
                        else if (intoNewCol)
                        {
                            row[index] = row[i];
                        }
                    }
                }
            }
        }
    }
}
