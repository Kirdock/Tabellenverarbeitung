using DataTableConverter.Assisstant;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    class ProcPadding : WorkProc
    {
        internal static readonly string ClassName = "Zeichen auffüllen";
        internal char? Character = null;
        internal int Counter = 1;
        internal DataTable Conditions;
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
            Conditions.Columns.Add("Spalte");
            Conditions.Columns.Add("Wert");
        }

        public override string[] GetHeaders()
        {
            HashSet<string> headers = new HashSet<string>(GetAffectedHeaders());
            Conditions.Rows.Cast<DataRow>().Select(row => row[(int)ConditionColumn.Spalte].ToString()).ToList().ForEach(header => headers.Add(header));
            return WorkflowHelper.RemoveEmptyHeaders(headers.ToArray());
        }

        private string[] GetAffectedHeaders()
        {
            return Columns.Rows.Cast<DataRow>().Select(dr => dr.ItemArray.Length > 0 ? dr.ItemArray[0].ToString() : null).ToArray();
        }

        public override void doWork(DataTable table, out string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure)
        {
            int lastCol = table.Columns.Count;
            string[] columns = GetAffectedHeaders();
            sortingOrder = string.Empty;
            bool intoNewCol = false;

            if (!Character.HasValue)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(NewColumn))
            {
                DataHelper.addColumn(NewColumn, table);
                intoNewCol = true;
            }
            List<int> headerIndices = DataHelper.getHeaderIndices(table, columns);
            foreach (DataRow row in table.Rows)
            {
                foreach (int i in headerIndices)
                {
                    bool valid = Conditions.Rows.Count == 0;
                    int index = intoNewCol ? lastCol : i;
                    bool result = !valid;
                    foreach (DataRow rep in Conditions.Rows)
                    {
                        string column = rep[(int)ConditionColumn.Spalte].ToString();
                        string value = rep[(int)ConditionColumn.Wert].ToString();
                        if (!string.IsNullOrWhiteSpace(column))
                        {
                            result = result && row[column].ToString() == value;
                        }
                    }
                    if (valid || result)
                    {
                        switch (OperationSide)
                        {
                            case Side.Left:
                                row[index] = row[i].ToString().PadLeft(Counter, Character.Value);
                                break;

                            case Side.Right:
                            default:
                                row[index] = row[i].ToString().PadRight(Counter, Character.Value);
                                break;
                        }
                    }
                    else if(intoNewCol)
                    {
                        row[index] = row[i];
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
    }
}
