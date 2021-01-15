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
    class ProcSubstring : WorkProc
    {
        internal static readonly string ClassName = "Substring";
        public int Start;
        public int End;
        public string ReplaceText;
        public bool ReplaceChecked; //I need it because ReplaceText can also be empty
        public bool ReverseCheck;

        public ProcSubstring(int ordinal, int id, string name) : base(ordinal, id, name)
        {
            Start = 1;
        }

        public ProcSubstring(string[] columns, string header, bool copyOldColumn, int start, int end, string replaceText, bool replaceChecked, bool reverseCheck)
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
            ReplaceText = replaceText;
            ReplaceChecked = replaceChecked;
            ReverseCheck = reverseCheck;
        }

        public override string[] GetHeaders()
        {
            return RemoveEmptyHeaders(Columns.AsEnumerable().Select(dr => dr.ItemArray.Length > 0 ? dr.ItemArray[0].ToString() : null));
        }

        public override void RenameHeaders(string oldName, string newName)
        {
            foreach (DataRow row in Columns.Rows)
            {
                if (row.ItemArray[0].ToString() == oldName)
                {
                    row.SetField(0, newName);
                }
            }
        }

        public override void RemoveHeader(string colName)
        {
            Columns = Columns.AsEnumerable().Where(row => row[0].ToString() != colName).ToTable(Columns);
        }

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName = "main")
        {
            if (PrepareMultiple(GetHeaders(), invokeForm, tableName, out string[] sourceColumns, out string[] destinationColumns))
            {
                invokeForm.DatabaseHelper.Substring(sourceColumns, destinationColumns, ReplaceText, Start, End, ReplaceChecked, ReverseCheck, tableName);
            }
        }

    }
}
