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
            return RemoveEmptyHeaders(DuplicateColumns);
        }

        public override void RenameHeaders(string oldName, string newName)
        {
            for (int x = 0; x < DuplicateColumns.Length; x++)
            {
                if (DuplicateColumns[x] == oldName)
                {
                    DuplicateColumns[x] = newName;
                }
            }
        }

        public override void RemoveHeader(string colName)
        {
            DuplicateColumns = DuplicateColumns.Where(x => x != colName).ToArray();
        }

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName = "main")
        {
            DuplicateColumns = GetHeaders();
            Hashtable hTable = new Hashtable();
            Hashtable totalTable = new Hashtable();

            int[] subStringBegin = duplicateCase.getBeginSubstring();
            int[] subStringEnd = duplicateCase.getEndSubstring();

            //we could calculate an identifier and save it into another table (make it with an index but not primary key)
            //first iteration => save identifier and rowId into another table
            //second iteration => select everything with count > 1
            string column = null;
            if (!string.IsNullOrWhiteSpace(NewColumn))
            {
                invokeForm.DatabaseHelper.AddColumnWithDialog(NewColumn, invokeForm, tableName, out column);
            }

            if (column != null)
            {
                for (int index = 0; index < table.Rows.Count; index++)
                {
                    string identifierTotal = GetColumnsAsObjectArray(table.Rows[index], DuplicateColumns, null, null, null);
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
                        string identifier = GetColumnsAsObjectArray(table.Rows[index], DuplicateColumns, subStringBegin, subStringEnd, tolerances);
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

        private string GetColumnsAsObjectArray(DataRow row, string[] columns, int[] subStringBegin, int[] subStringEnd, List<Tolerance> tolerances)
        {
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < columns.Length; i++)
            {
                #region Set Tolerances
                StringBuilder result = new StringBuilder(row[columns[i]].ToString().ToLower());
                if (tolerances != null)
                {
                    foreach (Tolerance tol in tolerances)
                    {
                        List<string> array = new List<string>(tol.getColumnsAsArrayToLower()) { tol.Name }.Distinct().ToList();
                        string replaceWith = array.Contains(string.Empty) ? string.Empty : tol.Name;
                        array.Remove(string.Empty);

                        foreach (string t in array)
                        {
                            result.Replace(t, replaceWith);
                        }
                    }
                }
                #endregion

                string resultString = result.ToString();

                #region Set Substring
                if (subStringBegin != null)
                {
                    int begin = subStringBegin[i];
                    int end = subStringEnd[i];
                    if (begin != 0 && end != 0 && end >= begin)
                    {
                        if (begin - 1 > resultString.Length)
                        {
                            resultString = string.Empty;
                        }
                        else
                        {
                            int count = end - begin + 1;
                            if (begin + count > resultString.Length)
                            {
                                count = resultString.Length - begin + 1;
                            }
                            resultString = resultString.Substring(begin - 1, count);
                        }
                    }
                }
                #endregion

                res.Append("|").Append(resultString);
            }
            return res.ToString();
        }

    }
}
