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
    class ProcReplaceWhole : WorkProc
    {
        internal static readonly string ClassName = "Text ersetzen";
        internal enum ColumnIndex : int { Column = 0, Value = 1};

        public ProcReplaceWhole(int ordinal, int id, string name) : base(ordinal, id, name) {
            InitColumns();
        }

        public ProcReplaceWhole(DataTable table)
        {
            InitColumns();
            foreach (DataRow row in table.Rows)
            {
                Columns.Rows.Add(new object[] { row[(int)ColumnIndex.Column].ToString(), row[(int)ColumnIndex.Value].ToString() });
            }
        }

        private void InitColumns()
        {
            Columns = new DataTable { TableName = "Columnnames" };
            SetColumns(Columns);
        }

        internal static void SetColumns(DataTable table)
        {
            table.Columns.Add("Spalten", typeof(string));
            table.Columns.Add("Text", typeof(string));
        }


        public override string[] GetHeaders()
        {
            return WorkflowHelper.RemoveEmptyHeaders(Columns.Rows.Cast<DataRow>().Select(dr => dr.ItemArray.Length > 0 ? dr.ItemArray[0].ToString() : null));
        }

        public override void renameHeaders(string oldName, string newName)
        {
            foreach (DataRow row in Columns.Rows)
            {
                if (row.ItemArray[(int)ColumnIndex.Column].ToString() == oldName)
                {
                    row.SetField((int) ColumnIndex.Column, newName);
                }
            }
        }

        public override void doWork(DataTable table, out string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure)
        {
            sortingOrder = string.Empty;
            foreach (DataRow row in table.Rows)
            {
                foreach(DataRow replaceRow in DataHelper.DataTableWithoutEmpty(Columns, (int)ColumnIndex.Column))
                {
                    row[replaceRow[(int)ColumnIndex.Column].ToString()] = replaceRow[(int)ColumnIndex.Value].ToString();
                }
            }
        }
    }
}
