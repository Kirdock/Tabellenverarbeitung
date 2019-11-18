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
    class ProcPadding : WorkProc
    {
        internal static readonly string ClassName = "Zeichen auffüllen";
        public char? Character = null;
        public int Counter = 1;
        public DataTable Conditions;
        internal enum Side { Left, Right};
        internal Side OperationSide = Side.Right;
        private enum ConditionColumn : int { Spalte = 0, Wert = 1};

        public ProcPadding(int ordinal, int id, string name) :base(ordinal, id, name)
        {
            InitConditions();
        }

        internal ProcPadding() : base(0, 0, null)
        {
            InitConditions();
        }

        private void InitConditions()
        {
            Conditions = new DataTable();
            Conditions.Columns.Add("Spalte", typeof(string));
            Conditions.Columns.Add("Wert", typeof(string));
        }

        public override string[] GetHeaders()
        {
            HashSet<string> headers = new HashSet<string>(GetAffectedHeaders());
            Conditions.Rows.Cast<DataRow>().Select(row => row[(int)ConditionColumn.Spalte].ToString()).ToList().ForEach(header => headers.Add(header));
            return WorkflowHelper.RemoveEmptyHeaders(headers);
        }

        internal string[] GetAffectedHeaders()
        {
            return WorkflowHelper.RemoveEmptyHeaders(Columns.Rows.Cast<DataRow>().Select(dr => dr.ItemArray.Length > 0 ? dr.ItemArray[0].ToString() : null));
        }

        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, out int[] newOrderIndices)
        {
            newOrderIndices = new int[0];
            string[] columns = GetAffectedHeaders();
            
            if (!Character.HasValue)
            {
                return;
            }

            if (CopyOldColumn)
            {
                //it would be easier/faster to rename oldColumn and create a new one with the old name; but with that method it is much for table.GetChanges() (History ValueChange)
                table.CopyColumns(columns);
            }

            bool newCol = !string.IsNullOrWhiteSpace(NewColumn);
            string c = newCol && table.AddColumnWithDialog(NewColumn, invokeForm) ? NewColumn : null;
            bool intoNewCol = c != null;
            if (!newCol || c != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    foreach (string col in columns)
                    {
                        bool valid = Conditions.Rows.Count == 0;
                        string index = c ?? col;
                        bool result = false;
                        foreach (DataRow rep in Conditions.Rows)
                        {
                            string column = rep[(int)ConditionColumn.Spalte].ToString();
                            string value = rep[(int)ConditionColumn.Wert].ToString();
                            if (!string.IsNullOrWhiteSpace(column))
                            {
                                result |= row[column].ToString() == value;
                            }
                        }
                        if (valid || result)
                        {
                            switch (OperationSide)
                            {
                                case Side.Left:
                                    row[index] = row[col].ToString().PadLeft(Counter, Character.Value);
                                    break;

                                case Side.Right:
                                default:
                                    row[index] = row[col].ToString().PadRight(Counter, Character.Value);
                                    break;
                            }
                        }
                        else if (intoNewCol)
                        {
                            row[index] = row[col];
                        }
                    }
                }
            }
        }

        public override void renameHeaders(string oldName, string newName)
        {
            foreach (DataRow row in Columns.Rows)
            {
                if (row[0].ToString() == oldName)
                {
                    row[0] = newName;
                }
            }

            foreach (DataRow row in Conditions.Rows)
            {
                if (row[(int)ConditionColumn.Spalte].ToString() == oldName)
                {
                    row[(int)ConditionColumn.Spalte] = newName;
                }
            }
        }

        public override void removeHeader(string colName)
        {
            Columns = Columns.AsEnumerable().Where(row => row[0].ToString() != colName).ToTable(Columns);
            Conditions =Conditions.AsEnumerable().Where(row => row[(int)ConditionColumn.Spalte].ToString() != colName).ToTable(Conditions);
        }
    }
}
