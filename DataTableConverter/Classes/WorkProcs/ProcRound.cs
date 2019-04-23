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
    class ProcRound : WorkProc
    {
        internal static readonly string ClassName = "Runden";
        internal int Decimals;
        internal int Type;

        public override string[] GetHeaders()
        {
            return WorkflowHelper.RemoveEmptyHeaders(Columns.Rows.Cast<DataRow>().Select(dr => dr.ItemArray.Length > 0 ? dr.ItemArray[0].ToString() : null));
        }

        public ProcRound(int ordinal, int id, string name) : base(ordinal, id, name) { }

        public ProcRound(string[] columns, int decimals, string newColumn, int type, bool copyOldColumn)
        {
            Decimals = decimals;
            NewColumn = newColumn;
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            Type = type;
            foreach (string col in columns)
            {
                Columns.Rows.Add(col);
            }
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

        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename)
        {
            int lastCol = table.Columns.Count;
            string[] columns = GetHeaders();
            bool intoNewCol = false;

            if (CopyOldColumn)
            {
                //it would be easier/faster to rename oldColumn and create a new one with the old name; but with that method it is much for table.GetChanges() (History ValueChange)
                DataHelper.CopyColumns(columns, table);
            }
            if (!string.IsNullOrWhiteSpace(NewColumn))
            {
                DataHelper.AddColumn(NewColumn, table);
                intoNewCol = true;
            }
            List<int> headerIndices = DataHelper.HeaderIndices(table, columns);
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < row.ItemArray.Length; i++)
                {

                    if (columns == null || headerIndices.Contains(i))
                    {
                        int index = intoNewCol ? lastCol : i;
                        if (float.TryParse(row[i].ToString(), out float result))
                        {
                            row.SetField(index, Round(result));
                        }
                    }
                }
            }
        }

        private string Round(float number)
        {
            string result;
            switch (Type)
            {
                //normal round
                case 0:
                    {
                        result = Math.Round(number, Decimals, MidpointRounding.AwayFromZero).ToString();
                    }
                    break;

                //ceiling
                case 1:
                    {
                        result = RoundUp(number).ToString();
                    }
                    break;
                //floor
                default:
                    {
                        result = RoundDown(number).ToString();
                    }
                    break;
            }
            return result;
        }

        private double RoundUp(float input)
        {
            double multiplier = Math.Pow(10, Convert.ToDouble(Decimals));
            return Math.Ceiling(input * multiplier) / multiplier;
        }

        private double RoundDown(float input)
        {
            double multiplier = Math.Pow(10, Convert.ToDouble(Decimals));
            return Math.Floor(input * multiplier) / multiplier;
        }
    }
}
