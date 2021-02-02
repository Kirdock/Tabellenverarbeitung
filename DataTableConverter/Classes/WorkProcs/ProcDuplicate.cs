using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    internal class ProcDuplicate : WorkProc
    {
        internal override string NewColumn => "Duplikat";

        internal ProcDuplicate(int ordinal, int id, string name) : base(ordinal, id, name)
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

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName)
        {
            DuplicateColumns = GetHeaders();
            Hashtable hTable = new Hashtable();
            Hashtable totalTable = new Hashtable();

            int[] subStringBegin = duplicateCase.getBeginSubstring();
            int[] subStringEnd = duplicateCase.getEndSubstring();

            //we could calculate an identifier and save it into another table (make it with an index but not primary key)
            //first iteration => save identifier and rowId into another table
            //second iteration => select everything with count > 1
            //delete temp table after finish
            string column = null;
            if (!string.IsNullOrWhiteSpace(NewColumn))
            {
                invokeForm.DatabaseHelper.AddColumnWithDialog(NewColumn, invokeForm, tableName, out column);
            }

            if (column != null)
            {
                SQLiteCommand command = invokeForm.DatabaseHelper.GetDataCommand(tableName, DuplicateColumns);
                string sourceRowIdName = "sourceid";
                string identifierColumn = "identifier";
                string duplicateTableTotal = Guid.NewGuid().ToString();
                string duplicateTableShort = Guid.NewGuid().ToString();
                string[] duplicateColumns = new string[] { sourceRowIdName, identifierColumn };

                foreach (string table in new string[] { duplicateTableShort, duplicateTableTotal })
                {
                    invokeForm.DatabaseHelper.CreateTable(duplicateColumns, table);
                    invokeForm.DatabaseHelper.CreateIndexOn(table, identifierColumn, null, true);
                }
                Dictionary<int, string> updates = new Dictionary<int, string>();

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    SQLiteCommand updateCommandTotal = null;
                    SQLiteCommand updateCommandShort = null;
                    while (reader.Read())
                    {
                        string identifierTotal = GetColumnsAsObjectArray(reader, null, null, null);

                        if (invokeForm.DatabaseHelper.ExistsValueInColumn(identifierColumn, identifierTotal, duplicateTableTotal, sourceRowIdName, out int sourceId))
                        {
                            if (!updates.ContainsKey(sourceId))
                            {
                                updates.Add(sourceId, duplicateCase.ShortcutTotal);
                            }

                            updates.Add(reader.GetInt16(0), duplicateCase.ShortcutTotal + duplicateCase.ShortcutTotal);
                        }
                        else
                        {
                            updateCommandTotal = invokeForm.DatabaseHelper.InsertRow(duplicateColumns, new string[] { reader.GetInt32(0).ToString(), identifierTotal }, duplicateTableTotal, updateCommandTotal);

                            string identifierShort = GetColumnsAsObjectArray(reader, subStringBegin, subStringEnd, tolerances);
                            if (invokeForm.DatabaseHelper.ExistsValueInColumn(identifierColumn, identifierShort, duplicateTableShort, sourceRowIdName, out int sourceId2))
                            {
                                if (!updates.ContainsKey(sourceId2))
                                {
                                    updates.Add(sourceId2, duplicateCase.ShortcutTotal);
                                }
                                updates.Add(reader.GetInt16(0), duplicateCase.Shortcut + duplicateCase.Shortcut);
                            }
                            else
                            {
                                updateCommandShort = invokeForm.DatabaseHelper.InsertRow(duplicateColumns, new string[] { reader.GetInt32(0).ToString(), identifierShort }, duplicateTableShort, updateCommandShort);
                            }
                        }
                    }
                }
                invokeForm.DatabaseHelper.UpdateCells(updates.ToArray(), column, tableName);
                invokeForm.DatabaseHelper.Delete(duplicateTableTotal);
                invokeForm.DatabaseHelper.Delete(duplicateTableShort);
            }
        }

        private string GetColumnsAsObjectArray(SQLiteDataReader reader, int[] subStringBegin, int[] subStringEnd, List<Tolerance> tolerances)
        {
            StringBuilder res = new StringBuilder();
            for (int i = 1; i < reader.FieldCount; i++)
            {
                #region Set Tolerances
                StringBuilder result = new StringBuilder(reader.GetString(i).ToLower());
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
                    int begin = subStringBegin[i-1];
                    int end = subStringEnd[i-1];
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
