using DataTableConverter.Assisstant;
using DataTableConverter.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    internal class ProcDuplicate : WorkProc
    {
        internal override string NewColumn => "Duplikat";

        internal ProcDuplicate(int ordinal, int id, string name) : base(ordinal, id,name)
        {
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            Ordinal = ordinal;
            ProcedureId = id;
            DuplicateColumns = new string[0];
            Name = name;
        }

        public override string[] GetHeaders()
        {
            return WorkflowHelper.RemoveEmptyHeaders(DuplicateColumns);
        }

        public override void renameHeaders(string oldName, string newName)
        {
            for (int x = 0; x < DuplicateColumns.Length; x++)
            {
                if (DuplicateColumns[x] == oldName)
                {
                    DuplicateColumns[x] = newName;
                }
            }
        }

        public override void removeHeader(string colName)
        {
            DuplicateColumns = DuplicateColumns.Where(x => x != colName).ToArray();
        }

        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, out int[] newOrderIndices)
        {
            DuplicateColumns = GetHeaders();
            newOrderIndices = new int[0];
            Hashtable hTable = new Hashtable();
            Hashtable totalTable = new Hashtable();

            int[] subStringBegin = duplicateCase.getBeginSubstring();
            int[] subStringEnd = duplicateCase.getEndSubstring();


            string column = !string.IsNullOrWhiteSpace(NewColumn) && table.AddColumnWithDialog(NewColumn, invokeForm) ? NewColumn : null;

            if (column != null)
            {
                for (int index = 0; index < table.Rows.Count; index++)
                {
                    string identifierTotal = WorkflowHelper.GetColumnsAsObjectArray(table.Rows[index], DuplicateColumns, null, null, null);
                    bool isTotal;
                    if (isTotal = totalTable.Contains(identifierTotal))
                    {
                        table.Rows[(int)totalTable[identifierTotal]].SetField(column, duplicateCase.ShortcutTotal);
                        table.Rows[index].SetField(column, duplicateCase.ShortcutTotal + duplicateCase.ShortcutTotal);
                    }
                    else
                    {
                        totalTable.Add(identifierTotal, index);
                    }
                    if (!isTotal)
                    {
                        string identifier = WorkflowHelper.GetColumnsAsObjectArray(table.Rows[index], DuplicateColumns, subStringBegin, subStringEnd, tolerances);
                        if (hTable.Contains(identifier))
                        {
                            table.Rows[(int)hTable[identifier]].SetField(column, duplicateCase.Shortcut);
                            table.Rows[index].SetField(column, duplicateCase.Shortcut + duplicateCase.Shortcut);
                        }
                        else
                        {
                            hTable.Add(identifier, index);
                        }
                    }
                }
            }
        }

    }
}
