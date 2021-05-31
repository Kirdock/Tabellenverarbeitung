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
    internal class PreparedTolerance
    {
        internal string[] Patterns;
        internal string ReplaceWith;
    }

    [Serializable()]
    internal class ProcDuplicate : WorkProc
    {
        public override string NewColumn => "Duplikat";

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

            int[] subStringBegin = null;
            int[] subStringEnd = null;
            if (!duplicateCase.ApplyAll)
            {
                List<int> begin = duplicateCase.getBeginSubstring().ToList();
                List<int> end = duplicateCase.getEndSubstring().ToList();

                for (int i = 0; i < begin.Count; i++)
                {
                    if (begin[i] == 0 || end[i] == 0 || end[i] < begin[i])
                    {
                        begin.RemoveAt(i);
                        end.RemoveAt(i);
                        i--;
                    }
                }

                subStringBegin = begin.ToArray();
                subStringEnd = end.ToArray();
            }
            bool containsShort = subStringBegin?.Length > 0;

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
                invokeForm.SetWorkflowText("Duplikate Abgleich");
                invokeForm.StartLoadingBarCount(invokeForm.DatabaseHelper.GetRowCount(tableName));
                string[] aliases = duplicateCase.ApplyAll ? invokeForm.DatabaseHelper.GetSortedColumnsAsAlias(tableName).ToArray() : DuplicateColumns;
                SQLiteCommand command = invokeForm.DatabaseHelper.GetDataCommand(tableName, aliases);
                string sourceRowIdName = "sourceid";
                string identifierColumn = "identifier";
                string duplicateTableTotal = Guid.NewGuid().ToString();
                string duplicateTableShort = Guid.NewGuid().ToString();
                string[] duplicateColumns = new string[] { sourceRowIdName, identifierColumn };

                foreach (string table in new string[] { duplicateTableShort, duplicateTableTotal })
                {
                    invokeForm.DatabaseHelper.CreateTable(duplicateColumns, table, false);
                    invokeForm.DatabaseHelper.CreateIndexOn(table, identifierColumn, null, true);
                }
                Dictionary<int, string> updates = new Dictionary<int, string>();

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    SQLiteCommand selectCommandTotal = invokeForm.DatabaseHelper.ExistsValueInColumnCommand(identifierColumn, duplicateTableTotal, sourceRowIdName);
                    SQLiteCommand selectCommandShort = invokeForm.DatabaseHelper.ExistsValueInColumnCommand(identifierColumn, duplicateTableShort, sourceRowIdName);
                    SQLiteCommand updateCommandTotal = invokeForm.DatabaseHelper.InsertDuplicateCommand(duplicateColumns, duplicateTableTotal);
                    SQLiteCommand updateCommandShort = invokeForm.DatabaseHelper.InsertDuplicateCommand(duplicateColumns, duplicateTableShort); ;
                    PreparedTolerance[] preparedTolerances = PrepareTolerances(tolerances);

                    while (reader.Read())
                    {
                        string identifierTotal = GetColumnsAsObjectArray(reader, null, null, null);

                        if (invokeForm.DatabaseHelper.ExistsValueInColumn(identifierTotal, out int sourceId, selectCommandTotal))
                        {
                            if (!updates.ContainsKey(sourceId))
                            {
                                updates.Add(sourceId, duplicateCase.ShortcutTotal);
                            }
                            int id = reader.GetInt16(0);
                            if (!updates.ContainsKey(id))
                            {
                                updates.Add(id, duplicateCase.ShortcutTotal + duplicateCase.ShortcutTotal);
                            }
                        }
                        else
                        {
                            int id = reader.GetInt16(0);
                            invokeForm.DatabaseHelper.InsertRowDuplicate(id.ToString(), identifierTotal, updateCommandTotal);
                            if (containsShort)
                            {
                                string identifierShort = GetColumnsAsObjectArray(reader, subStringBegin, subStringEnd, preparedTolerances);
                                if (invokeForm.DatabaseHelper.ExistsValueInColumn(identifierShort, out int sourceId2, selectCommandShort))
                                {
                                    if (!updates.ContainsKey(sourceId2))
                                    {
                                        updates.Add(sourceId2, duplicateCase.Shortcut);
                                    }

                                    if (!updates.ContainsKey(id))
                                    {
                                        updates.Add(id, duplicateCase.Shortcut + duplicateCase.Shortcut);
                                    }
                                }
                                else
                                {
                                    invokeForm.DatabaseHelper.InsertRowDuplicate(id.ToString(), identifierShort, updateCommandShort);
                                }
                            }
                        }
                        invokeForm.UpdateLoadingBar();
                    }
                }
                invokeForm.DatabaseHelper.UpdateCells(updates.ToArray(), column, tableName);
                invokeForm.DatabaseHelper.Delete(duplicateTableTotal);
                invokeForm.DatabaseHelper.Delete(duplicateTableShort);
                invokeForm.StartLoadingBar();
            }
        }

        private PreparedTolerance[] PrepareTolerances(List<Tolerance> tolerances)
        {
            PreparedTolerance[] preparedTolerances = new PreparedTolerance[tolerances.Count];
            int i = 0;
            foreach (Tolerance tol in tolerances)
            {
                List<string> array = new List<string>(tol.getColumnsAsArrayToLower()).Distinct().ToList();
                string replaceWith = array.Contains(string.Empty) ? string.Empty : array.First();
                array.Remove(string.Empty);
                string[] patterns = array.Select(t => @"(?<=^|[\s>])" + System.Text.RegularExpressions.Regex.Escape(t) + @"(?!\w)").ToArray();
                preparedTolerances[i] = new PreparedTolerance() { ReplaceWith = replaceWith, Patterns = patterns };
                i++;
            }
            return preparedTolerances;
        }

        private string GetColumnsAsObjectArray(SQLiteDataReader reader, int[] subStringBegin, int[] subStringEnd, PreparedTolerance[] preparedTolerances)
        {
            StringBuilder res = new StringBuilder();
            
            for (int i = 1; i < reader.FieldCount; i++)
            {
                string result = reader.GetValue(i).ToString();

                #region Set Tolerances
                if (preparedTolerances != null)
                {
                    foreach (PreparedTolerance tol in preparedTolerances)
                    {
                        foreach (string pattern in tol.Patterns)
                        {
                            result = System.Text.RegularExpressions.Regex.Replace(result, pattern, tol.ReplaceWith);
                        }
                    }
                }
                #endregion

                #region Set Substring
                if (subStringBegin != null)
                {
                    int begin = subStringBegin[i-1];
                    int end = subStringEnd[i-1];
                    if (begin - 1 > result.Length)
                    {
                        result = string.Empty;
                    }
                    else
                    {
                        int count = end - begin + 1;
                        if (begin + count > result.Length)
                        {
                            count = result.Length - begin + 1;
                        }
                        result = result.Substring(begin - 1, count);
                    }
                }
                #endregion

                res.Append("|").Append(result);
            }
            return res.ToString();
        }

    }
}
