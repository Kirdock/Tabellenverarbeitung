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
    class ProcCompare : WorkProc
    {
        internal static readonly string ClassName = "Spalten vergleichen";
        internal string SourceColumn;
        internal string CompareColumn;

        public ProcCompare(int ordinal, int id, string name) : base(ordinal, id, name) { }

        internal ProcCompare(string newColumn, bool oldColumn, string sourceColumn, string compareColumn)
        {
            NewColumn = newColumn;
            CopyOldColumn = oldColumn;
            SourceColumn = sourceColumn;
            CompareColumn = compareColumn;
        }

        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow)
        {
            int lastCol = table.Columns.Count;
            bool intoNewCol = false;

            if (CopyOldColumn)
            {
                DataHelper.CopyColumns(new string[] { SourceColumn }, table);
            }
            else if (!string.IsNullOrWhiteSpace(NewColumn))
            {
                DataHelper.AddColumn(NewColumn, table);
                intoNewCol = true;
            }

            int index = intoNewCol ? lastCol : table.Columns.IndexOf(SourceColumn);

            foreach (DataRow row in table.Rows)
            {
                if(row[SourceColumn].ToString() == row[CompareColumn].ToString())
                {
                    row[index] = string.Empty;
                }
            }
        }

        public override string[] GetHeaders()
        {
            return new string[] { SourceColumn, CompareColumn };
        }

        public override void renameHeaders(string oldName, string newName)
        {
            if(SourceColumn == oldName)
            {
                SourceColumn = newName;
            }
            if(CompareColumn == oldName)
            {
                CompareColumn = newName;
            }
        }

    }
}
