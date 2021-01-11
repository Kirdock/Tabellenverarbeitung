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
    class ProcSplit : WorkProc
    {
        internal static readonly string ClassName = "Aufteilen";
        public string Column;
        public string SplitText;

        public ProcSplit(int ordinal, int id, string name) : base(ordinal, id, name){}

        public ProcSplit(string column, string splitText, string newColumn)
        {
            Column = column;
            SplitText = splitText;
            NewColumn = newColumn;
        }

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName = "main")
        {
            if (!string.IsNullOrWhiteSpace(NewColumn) && !string.IsNullOrWhiteSpace(Column) && SplitText?.Length > 0)
            {
                invokeForm.DatabaseHelper.SplitColumnByString(Column, NewColumn, SplitText, tableName);
            }
        }

        public override string[] GetHeaders()
        {
            return new string[] { Column };
        }

        public override void RemoveHeader(string colName)
        {
            if(Column == colName)
            {
                Column = null;
            }
        }

        public override void RenameHeaders(string oldName, string newName)
        {
            if(Column == oldName)
            {
                Column = newName;
            }
        }
    }
}
