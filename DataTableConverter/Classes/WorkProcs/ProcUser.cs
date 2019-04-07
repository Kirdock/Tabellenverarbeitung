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
            return WorkflowHelper.RemoveEmptyHeaders(Columns.Rows.Cast<DataRow>().Select(dr => dr.ItemArray.Length > 0 ? dr.ItemArray[0].ToString() : null));
        }

        public ProcUser(int ordinal, int id,string name) : base(ordinal, id, name) { }

        public ProcUser(string[] columns, string header, bool copyOldColumn)
        {
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            foreach(string col in columns)
            {
                Columns.Rows.Add(col);
            }
            NewColumn = header;
            CopyOldColumn = copyOldColumn;
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
            if (CopyOldColumn)
            {
                //it would be easier/faster to rename oldColumn and create a new one with the old name; but with that method it is much for table.GetChanges() (History ValueChange)
                DataHelper.CopyColumns(columns, table);
            }
            if (!string.IsNullOrWhiteSpace(NewColumn))
            {
                DataHelper.AddColumn(NewColumn, table);
                intoNewCol = true;
            }
            List<int> headerIndices = DataHelper.HeaderIndices(table, columns);
            foreach (DataRow row in table.Rows)
            {
                foreach (int i in headerIndices)
                {
                    int index = intoNewCol ? lastCol : i;
                    foreach (DataRow rep in replaces.Rows)
                    {
                        string value = row[i].ToString();
                        string replace = rep[0].ToString();

                        if (procedure.CheckTotal && value == replace)
                        {
                            row[index] = rep[1].ToString();
                        }
                        else if (!procedure.CheckTotal && value.Contains(replace))
                        {
                            row[index] = value.Replace(replace, rep[1].ToString());
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
