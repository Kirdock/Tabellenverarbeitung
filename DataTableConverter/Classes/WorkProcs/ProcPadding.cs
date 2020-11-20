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
        internal enum Side { Left = 0, Right = 1};
        internal Side OperationSide = Side.Right;
        internal enum ConditionColumn : int { Spalte = 0, Wert = 1};

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
            Conditions.AsEnumerable().Select(row => row[(int)ConditionColumn.Spalte].ToString()).ToList().ForEach(header => headers.Add(header));
            return WorkflowHelper.RemoveEmptyHeaders(headers);
        }

        internal string[] GetAffectedHeaders()
        {
            return WorkflowHelper.RemoveEmptyHeaders(Columns.AsEnumerable().Select(dr => dr.ItemArray.FirstOrDefault()?.ToString()));
        }

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName = "main")
        {
            string[] columns = GetAffectedHeaders();
            
            if (!Character.HasValue)
            {
                return;
            }

            if (PrepareMultiple(columns, invokeForm, tableName, out string[] sourceColumns, out string[] destinationColumns))
            {
                invokeForm.DatabaseHelper.Padding(sourceColumns, destinationColumns, Conditions, OperationSide, Counter, Character.Value, tableName);
            }
        }

        public override void RenameHeaders(string oldName, string newName)
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

        public override void RemoveHeader(string colName)
        {
            Columns = Columns.AsEnumerable().Where(row => row[0].ToString() != colName).ToTable(Columns);
            Conditions =Conditions.AsEnumerable().Where(row => row[(int)ConditionColumn.Spalte].ToString() != colName).ToTable(Conditions);
        }
    }
}
