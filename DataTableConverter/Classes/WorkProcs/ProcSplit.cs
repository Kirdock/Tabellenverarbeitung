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
        public string Column;
        public string SplitText;

        public ProcSplit(int ordinal, int id, string name) : base(ordinal, id, name){}

        public ProcSplit(string column, string splitText, string newColumn)
        {
            Column = column;
            SplitText = splitText;
            NewColumn = newColumn;
        }

        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, out int[] newOrderIndices)
        {
            newOrderIndices = new int[0];
            if (!string.IsNullOrWhiteSpace(NewColumn) && !string.IsNullOrWhiteSpace(Column))
            {
                int counter = 1;
                List<string> newColumns = new List<string>() { table.TryAddColumn(NewColumn, counter) };
                foreach (DataRow row in table.Rows)
                {
                    string[] result = row[Column].ToString().Split(new string[] { SplitText }, StringSplitOptions.RemoveEmptyEntries);
                    if (result.Length > 1)
                    {
                        while (counter < result.Length)
                        {
                            newColumns.Add(table.TryAddColumn(NewColumn, counter));
                            counter++;
                        }

                        for (int i = 0; i < result.Length; i++)
                        {
                            row[newColumns[i]] = result[i];
                        }
                    }
                }
            }
        }

        public override string[] GetHeaders()
        {
            return new string[] { Column };
        }

        public override void removeHeader(string colName)
        {
            if(Column == colName)
            {
                Column = null;
            }
        }

        public override void renameHeaders(string oldName, string newName)
        {
            if(Column == oldName)
            {
                Column = newName;
            }
        }
    }
}
