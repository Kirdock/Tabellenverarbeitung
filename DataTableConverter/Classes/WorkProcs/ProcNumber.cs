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
    class ProcNumber : WorkProc
    {
        internal int Start;
        internal int End;
        internal bool Repeat;
        internal static readonly string ClassName = "Nummerierung";

        public ProcNumber(int ordinal, int id, string name) : base(ordinal, id, name){}

        public ProcNumber(string newColumn, int start, int end, bool repeat)
        {
            NewColumn = newColumn;
            End = end;
            Start = start;
            Repeat = repeat;
        }

        public override void doWork(DataTable table, out string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure)
        {
            sortingOrder = string.Empty;
            if (!string.IsNullOrWhiteSpace(NewColumn))
            {
                int index = table.Columns.Count;
                DataHelper.AddColumn(NewColumn, table);

                int count = Start;
                bool noEnd = End != 0;
                foreach (DataRow row in table.Rows)
                {
                    row[index] = count;
                    count++;
                    if (noEnd && count > End)
                    {
                        if (Repeat)
                        {
                            count = Start;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        public override string[] GetHeaders()
        {
            return new string[0];
        }

        public override void renameHeaders(string oldName, string newName)
        {
            return;
        }
    }
}
