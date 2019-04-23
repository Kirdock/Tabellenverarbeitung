using DataTableConverter.Assisstant;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename)
        {
            Hashtable hTable = new Hashtable();
            Hashtable totalTable = new Hashtable();
            ArrayList duplicateList = new ArrayList();

            int[] subStringBegin = duplicateCase.getBeginSubstring();
            int[] subStringEnd = duplicateCase.getEndSubstring();
            
            int lastIndex = table.Columns.IndexOf("Duplikat");
            bool columnAdded;
            if (columnAdded = lastIndex == -1)
            {
                lastIndex = table.Columns.Count;
                DataHelper.AddColumn("Duplikat", table);

            }



            for (int index = 0; index < table.Rows.Count; index++)
            {
                string identifierTotal = WorkflowHelper.GetColumnsAsObjectArray(table.Rows[index], DuplicateColumns, null, null, null);
                bool isTotal;
                if (isTotal = totalTable.Contains(identifierTotal))
                {
                    table.Rows[(int)totalTable[identifierTotal]].SetField(lastIndex, duplicateCase.ShortcutTotal);
                    table.Rows[index].SetField(lastIndex, duplicateCase.ShortcutTotal + duplicateCase.ShortcutTotal);
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
                        table.Rows[(int)hTable[identifier]].SetField(lastIndex, duplicateCase.Shortcut);
                        table.Rows[index].SetField(lastIndex, duplicateCase.Shortcut + duplicateCase.Shortcut);
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
