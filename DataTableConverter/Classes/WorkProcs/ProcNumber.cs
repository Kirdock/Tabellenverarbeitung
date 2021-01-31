using System;
using System.Collections.Generic;
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

        public ProcNumber(int ordinal, int id, string name) : base(ordinal, id, name) { }

        public ProcNumber(string newColumn, int start, int end, bool repeat)
        {
            NewColumn = newColumn;
            End = end;
            Start = start;
            Repeat = repeat;
        }

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName = "main")
        {
            if (!string.IsNullOrWhiteSpace(NewColumn))
            {
                invokeForm.DatabaseHelper.AddColumnWithDialog(NewColumn, invokeForm, tableName, out string column);
                if (column != null)
                {
                    invokeForm.DatabaseHelper.Enumerate(column, Start, End, Repeat, sortingOrder, orderType, tableName);
                }
            }
        }

        public override string[] GetHeaders()
        {
            return new string[0];
        }

        public override void RenameHeaders(string oldName, string newName)
        {
            return;
        }

        public override void RemoveHeader(string colName)
        {
            return;
        }
    }
}
