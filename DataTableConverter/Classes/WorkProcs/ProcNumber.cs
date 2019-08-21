using DataTableConverter.Assisstant;
using DataTableConverter.Extensions;
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
    class ProcNumber : WorkProc
    {
        public int Start;
        public int End;
        public bool Repeat;
        internal static readonly string ClassName = "Nummerierung";

        public ProcNumber(int ordinal, int id, string name) : base(ordinal, id, name){}

        public ProcNumber(string newColumn, int start, int end, bool repeat)
        {
            NewColumn = newColumn;
            End = end;
            Start = start;
            Repeat = repeat;
        }

        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form invokeForm, out int[] newOrderIndices)
        {
            newOrderIndices = new int[0];
            if (!string.IsNullOrWhiteSpace(NewColumn))
            {
                int index = table.Columns.Count;
                table.TryAddColumn(NewColumn);

                int count = Start;
                bool noEnd = End != 0;
                foreach (DataRow row in table.GetSortedTable(sortingOrder,orderType))
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

        public override void removeHeader(string colName)
        {
            return;
        }
    }
}
