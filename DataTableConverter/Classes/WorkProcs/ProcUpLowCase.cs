using DataTableConverter.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    internal class ProcUpLowCase : WorkProc
    {
        public static readonly string ClassName = "Groß-/Kleinschreibung";
        public int Option; //0: UpperCase; 1: LowerCase, 2: first letter UpperCase, 3: first letters Uppercase
        public bool AllColumns { get; set; }
        public override string[] GetHeaders()
        {
            return AllColumns ? new string[0] : RemoveEmptyHeaders(Columns.AsEnumerable().Select(dr => dr.ItemArray.Length > 0 ? dr.ItemArray[0].ToString() : null));
        }

        public ProcUpLowCase(int ordinal, int id, string name) : base(ordinal, id, name)
        {
            Option = 0;
        }

        public ProcUpLowCase(string[] columns, bool allColumns, int option)
        {
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            foreach (string col in columns)
            {
                Columns.Rows.Add(col);
            }
            AllColumns = allColumns;
            Option = option;
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

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName)
        {
            invokeForm.DatabaseHelper.SetCustomUppercase(GetHeaders(), Option, tableName);
        }


    }
}
