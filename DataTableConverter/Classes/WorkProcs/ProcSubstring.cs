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
    class ProcSubstring : WorkProc
    {
        internal static readonly string ClassName = "Substring";
        public int Start;
        public int End;
        public string ReplaceText;
        public bool ReplaceChecked; //I need it because ReplaceText can also be empty
        public bool ReverseCheck;

        public ProcSubstring(int ordinal, int id, string name) : base(ordinal, id, name)
        {
            Start = 1;
        }

        public ProcSubstring(string[] columns, string header, bool copyOldColumn, int start, int end, string replaceText, bool replaceChecked, bool reverseCheck)
        {
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            foreach (string col in columns)
            {
                Columns.Rows.Add(col);
            }
            NewColumn = header;
            CopyOldColumn = copyOldColumn;
            Start = start;
            End = end;
            ReplaceText = replaceText;
            ReplaceChecked = replaceChecked;
            ReverseCheck = reverseCheck;
        }

        public override string[] GetHeaders()
        {
            return WorkflowHelper.RemoveEmptyHeaders(Columns.Rows.Cast<DataRow>().Select(dr => dr.ItemArray.Length > 0 ? dr.ItemArray[0].ToString() : null));
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
            Columns = Columns.AsEnumerable().Where(row => row[0].ToString() != colName).ToTable(Columns);
        }

        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, out int[] newOrderIndices)
        {
            newOrderIndices = new int[0];
            string[] columns = GetHeaders();

            if (CopyOldColumn)
            {
                //it would be easier/faster to rename oldColumn and create a new one with the old name; but with that method it is much for table.GetChanges() (History ValueChange)
                table.CopyColumns(columns);
            }
            bool newCol = !string.IsNullOrWhiteSpace(NewColumn);
            string newColumn = newCol && table.AddColumnWithDialog(NewColumn, invokeForm) ? NewColumn : null;
            Func<string, int, int, string> substring = ReverseCheck ? (Func<string, int, int, string>)SubstringReverse : Substring;
            if (!newCol || newColumn != null)
            {
                if (!ReplaceChecked)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        foreach (string header in columns)
                        {
                            string value = row[header].ToString();
                            string col = newColumn ?? header;
                            if (End == 0)
                            {
                                row[col] = Start > value.Length ? string.Empty : substring(value, Start-1, 0);
                            }
                            else
                            {
                                int length = (End - Start);
                                row[col] = Start > value.Length ? string.Empty : length + Start > value.Length ? substring(value, Start-1,0) : substring(value,Start - 1, length + 1);
                            }
                        }
                    }
                }
                else
                {
                    foreach (DataRow row in table.Rows)
                    {
                        foreach (string header in columns)
                        {
                            string value = row[header].ToString();
                            string result = Start > value.Length ? string.Empty : substring(value, 0, Start - 1) + ReplaceText;

                            if (End < value.Length && End != 0 && Start <= value.Length)
                            {
                                result += substring(value,End,0);
                            }

                            row[newColumn ?? header] = result;
                        }
                    }
                }
            }
        }

        private string Substring(string value, int start, int end = 0)
        {
            return end == 0 ? value.Substring(start) : value.Substring(start, end);
        }

        private string SubstringReverse(string value, int start, int end = 0)
        {
            return end == 0 ? value.Substring(0, value.Length - start) : value.Substring(value.Length - end, end - start);
        }

    }
}
