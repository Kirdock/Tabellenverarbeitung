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
    class ProcSubstring : WorkProc
    {
        internal static readonly string ClassName = "Substring";
        internal int Start;
        internal int End;

        public ProcSubstring(int ordinal, int id, string name) : base(ordinal, id, name) {
            Start = 1;
        }

        public ProcSubstring(string[] columns, string header, bool copyOldColumn, int start, int end)
        {
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            foreach (string col in columns)
            {
                Columns.Rows.Add(col);
            }
            NewColumn = header;
            CopyOldColumn = copyOldColumn;
            Start = start;
            End = end;
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
            int lastCol = table.Columns.Count;
            string[] columns = GetHeaders();
            sortingOrder = string.Empty;
            bool intoNewCol = false;

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
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    if (headerIndices.Contains(i))
                    {
                        int index = intoNewCol ? lastCol : i;
                        string value = row[i].ToString();
                        if (End == 0)
                        {
                            row[index] = Start > value.Length ? string.Empty : value.Substring(Start - 1);
                        }
                        else
                        {
                            int length = (End - Start);
                            row[index] = Start > value.Length ? string.Empty : length + Start > value.Length ? value.Substring(Start - 1) : value.Substring(Start - 1, length + 1);
                        }

                    }
                }
            }
        }

    }
}
