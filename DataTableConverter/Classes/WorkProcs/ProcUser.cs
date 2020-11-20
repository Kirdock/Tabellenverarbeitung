﻿using DataTableConverter.Assisstant;
using DataTableConverter.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    internal class ProcUser : WorkProc
    {
        internal static readonly string ClassName = "Suchen & Ersetzen";
        internal static readonly string ContainsDataFormat = "[befüllt]";
        public bool IsSystem;
        public Proc Procedure;
        public override string[] GetHeaders()
        {
            return WorkflowHelper.RemoveEmptyHeaders(Columns.AsEnumerable().Select(dr => dr.ItemArray.Length > 0 ? dr.ItemArray[0].ToString() : null));
        }

        public ProcUser(int ordinal, int id,string name) : base(ordinal, id, name) { }

        public ProcUser(string[] columns, string header, bool copyOldColumn)
        {
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            foreach(string col in columns)
            {
                Columns.Rows.Add(col);
            }
            NewColumn = header;
            CopyOldColumn = copyOldColumn;
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
            Columns =  Columns.AsEnumerable().Where(row => row[0].ToString() != colName).ToTable(Columns);
        }

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName = "main")
        {
            string[] columns = GetHeaders();
            string newColumn = null;
            bool newCol = false;
            procedure = Procedure ?? procedure;

            IEnumerable<DataRow> replaces = procedure.Replace.AsEnumerable().Where(row => !string.IsNullOrEmpty(row[0]?.ToString()) || !string.IsNullOrEmpty(row[1]?.ToString()));
            IEnumerable<DataRow> replaceWithoutEmpty = replaces.Where(replace => replace[0].ToString() != string.Empty && replace[0].ToString() != ContainsDataFormat);
            IEnumerable<DataRow> replaceWithEmpty = replaces.Where(replace => replace[0].ToString() == string.Empty && replace[1].ToString().Length > 0);
            DataRow replaceWhole = replaces.FirstOrDefault(replace => replace[0].ToString() == ContainsDataFormat);
            string replaceWholeText = replaceWhole?[1].ToString() ?? string.Empty;
            bool containsReplaceWhole = replaceWhole != null;
            bool containsEmpty = replaceWithEmpty.Count() > 0;
            string replaceEmptyString = containsEmpty ? replaceWithEmpty.First()[1].ToString() : string.Empty;
            if (CopyOldColumn)
            {
                //it would be easier/faster to rename oldColumn and create a new one with the old name; but with that method it is much for table.GetChanges() (History ValueChange)
                table.CopyColumns(columns);
            }
            else if ((newCol = !string.IsNullOrWhiteSpace(NewColumn)))
            {
                newColumn = table.AddColumnWithDialog(NewColumn, invokeForm) ? NewColumn : null;
            }

            if (!newCol || newColumn != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    foreach (string column in columns)
                    {
                        string index = newColumn ?? column;
                        string value = row[column].ToString();
                        string result = value;
                        bool changed = false;
                        
                        if (procedure.CheckTotal)
                        {
                            DataRow foundRows = replaceWithoutEmpty.FirstOrDefault(replace => replace[0].ToString() == value);
                            if (foundRows != null)
                            {
                                result = foundRows[1].ToString();
                                changed = true;
                            }
                        }
                        else if (procedure.CheckWord)
                        {
                            foreach (DataRow rep in replaceWithoutEmpty)
                            {
                                string pattern = @"(?<=^|[\s>])" + Regex.Escape(rep[0].ToString()) + @"(?!\w)";
                                if (Regex.IsMatch(result, pattern))
                                {
                                    result = Regex.Replace(result, pattern, rep[1].ToString());
                                    changed = true;
                                }
                            }
                        }
                        else
                        {
                            foreach (DataRow rep in replaceWithoutEmpty)
                            {
                                string pattern = rep[0].ToString();
                                if (result.Contains(pattern))
                                {
                                    result = result.Replace(pattern, rep[1].ToString());
                                    changed = true;
                                }
                            }
                        }
                        
                        if(containsEmpty && !changed && result == string.Empty)
                        {
                            result = replaceEmptyString;
                            changed = true;
                        }
                        if (containsReplaceWhole && !changed && result != string.Empty)
                        {
                            result = replaceWholeText;
                            changed = true;
                        }
                        row[index] = !changed && procedure.LeaveEmpty ? string.Empty : ProcTrim.Trim(result);
                    }
                }
            }
        }
    }
}
