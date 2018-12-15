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
    internal class ProcDuplicate : WorkProc
    {
        internal override string NewColumn => "Duplikat";

        internal ProcDuplicate(int ordinal, int id, int type, string name) : base(ordinal, id, type, name)
        {
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            Ordinal = ordinal;
            ProcedureId = id;
            DuplicateColumns = new string[0];
            Name = name;
        }

        public override string[] getHeaders()
        {
            return WorkflowHelper.removeEmptyHeaders(DuplicateColumns);
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

        public override void doWork(DataTable table, out string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure)
        {
            sortingOrder = string.Empty;
            Hashtable hTable = new Hashtable();
            ArrayList duplicateList = new ArrayList();

            int[] subStringBegin = duplicateCase.getBeginSubstring();
            int[] subStringEnd = duplicateCase.getEndSubstring();
            DataTable oldTable = table.Copy();
            int lastIndex = table.Columns.IndexOf("Duplikat");
            bool columnAdded;
            if (columnAdded = lastIndex == -1)
            {
                lastIndex = table.Columns.Count;
                DataHelper.addColumn("Duplikat", table);

            }



            for (int index = 0; index < table.Rows.Count; index++)
            {
                string identifier = WorkflowHelper.getColumnsAsObjectArray(table.Rows[index], DuplicateColumns, subStringBegin, subStringEnd, tolerances);

                if (hTable.Contains(identifier))
                {
                    table.Rows[(int)hTable[identifier]].SetField(lastIndex, duplicateCase.Shortcut);
                    table.Rows[index].SetField(lastIndex, duplicateCase.Shortcut);
                }
                else
                {
                    hTable.Add(identifier, index);
                }
            }
        }

    }
}
