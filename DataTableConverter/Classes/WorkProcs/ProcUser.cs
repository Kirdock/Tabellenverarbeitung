using DataTableConverter.Assisstant;
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
        public override string[] GetHeaders()
        {
            return WorkflowHelper.RemoveEmptyHeaders(Columns.Rows.Cast<DataRow>().Select(dr => dr.ItemArray.Length > 0 ? dr.ItemArray[0].ToString() : null));
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

        public override void renameHeaders(string oldName, string newName)
        {
            foreach (DataRow row in Columns.Rows)
            {
                if (row.ItemArray[0].ToString() == oldName)
                {
                    row.SetField(0, newName);
                }
            }
        }

        public override void removeHeader(string colName)
        {
            Columns =  Columns.AsEnumerable().Where(row => row[0].ToString() != colName).ToTable(Columns);
        }

        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, out int[] newOrderIndices)
        {
            newOrderIndices = new int[0];
            string[] columns = GetHeaders();
            string newColumn = null;
            bool newCol = false;


            IEnumerable<DataRow> replaces = procedure.Replace.Rows.Cast<DataRow>().Where(row => !string.IsNullOrEmpty(row[0]?.ToString()) || !string.IsNullOrEmpty(row[1]?.ToString()));
            IEnumerable<DataRow> replaceWithoutEmpty = replaces.Where(replace => replace[0].ToString() != string.Empty);
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

                        if (procedure.CheckTotal)
                        {
                            DataRow foundRows = replaces.FirstOrDefault(replace => replace[0].ToString() == value);
                            if (foundRows != null)
                            {
                                row[index] = foundRows[1];
                            }
                            else
                            {
                                row[index] = value;
                            }
                        }
                        else if (procedure.CheckWord)
                        {
                            foreach (DataRow rep in replaceWithoutEmpty)
                            {
                                string pattern = @"\b" + Regex.Escape(rep[0].ToString()) + @"\b";
                                row[index] = Regex.Replace(value, pattern, rep[1].ToString());
                            }
                        }
                        else
                        {
                            foreach (DataRow rep in replaceWithoutEmpty)
                            {
                                value = value.Replace(rep[0].ToString(), rep[1].ToString());
                            }
                            row[index] = value;
                        }
                    }
                }
            }
        }
    }
}
