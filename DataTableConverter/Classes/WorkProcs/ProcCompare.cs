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
    class ProcCompare : WorkProc
    {
        internal static readonly string ClassName = "Spalten vergleichen";
        public string SourceColumn;
        public string CompareColumn;

        public ProcCompare(int ordinal, int id, string name) : base(ordinal, id, name) { }

        internal ProcCompare(string newColumn, bool oldColumn, string sourceColumn, string compareColumn)
        {
            NewColumn = newColumn;
            CopyOldColumn = oldColumn;
            SourceColumn = sourceColumn;
            CompareColumn = compareColumn;
        }

        public override void DoWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, out int[] newOrderIndices)
        {
            newOrderIndices = new int[0];
            bool isNewColumn = !string.IsNullOrWhiteSpace(NewColumn);
            if (string.IsNullOrWhiteSpace(SourceColumn) || string.IsNullOrWhiteSpace(CompareColumn))
            {
                return;
            }

            string column = SourceColumn;
            if (CopyOldColumn)
            {
                table.CopyColumns(new string[] { SourceColumn });
            }
            else if (isNewColumn)
            {
                column = table.AddColumnWithDialog(NewColumn, invokeForm) ? NewColumn : null;
            }

            if (column != null)
            {
                if (isNewColumn)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        string source = row[SourceColumn].ToString();
                        row[column] = source == row[CompareColumn].ToString() ? string.Empty : source;
                    }
                }
                else
                {
                    foreach (DataRow row in table.Rows)
                    {
                        if (row[SourceColumn].ToString() == row[CompareColumn].ToString())
                        {
                            row[column] = string.Empty;
                        }
                    }
                }
            }
        }

        public override string[] GetHeaders()
        {
            return new string[] { SourceColumn, CompareColumn };
        }

        public override void RenameHeaders(string oldName, string newName)
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

        public override void RemoveHeader(string colName)
        {
            if(CompareColumn == colName)
            {
                CompareColumn = null;
            }
            if(SourceColumn == colName)
            {
                SourceColumn = null;
            }
        }

    }
}
