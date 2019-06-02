using DataTableConverter.Assisstant;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        public override void removeHeader(string colName)
        {
            Columns = DataHelper.QueryTable(Columns, Columns.AsEnumerable().Where(row => row[0].ToString() != colName));
        }

        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow)
        {
            int lastCol = table.Columns.Count;
            string[] columns = GetHeaders();
            bool intoNewCol = false;
            IEnumerable<DataRow> replaces = procedure.Replace.Rows.Cast<DataRow>().Where(row => !string.IsNullOrWhiteSpace(row[0]?.ToString()));
            if (CopyOldColumn)
            {
                //it would be easier/faster to rename oldColumn and create a new one with the old name; but with that method it is much for table.GetChanges() (History ValueChange)
                DataHelper.CopyColumns(columns, table);
            }
            else if (!string.IsNullOrWhiteSpace(NewColumn))
            {
                DataHelper.AddColumn(NewColumn, table);
                intoNewCol = true;
            }
            foreach (DataRow row in table.Rows)
            {
                foreach (string column in columns)
                {
                    int index = intoNewCol ? lastCol : table.Columns.IndexOf(column);
                    string value = row[column].ToString();

                    if (procedure.CheckTotal)
                    {
                        DataRow foundRows = replaces.FirstOrDefault(replace => replace[0].ToString() == value);
                        if(foundRows != null)
                        {
                            row[index] = foundRows[1];
                        }
                        else
                        {
                            row[index] = value;
                        }
                    }
                    else
                    {
                        foreach (DataRow rep in replaces)
                        {
                            value = value.Replace(rep[0].ToString(), rep[1].ToString());
                        }
                        row[index] = value;
                    }
                }
            }
        }
    }
}
