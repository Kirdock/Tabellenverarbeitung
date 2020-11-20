using DataTableConverter.Assisstant;
using DataTableConverter.Classes.WorkProcs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DataTableConverter.Classes
{
    [Serializable()]
    abstract class WorkProc : IComparable<WorkProc>, IEquatable<WorkProc>
    {
        public int ProcedureId { get; set; }
        internal DataTable Columns { get; set; }
        internal string[] DuplicateColumns { get; set; }
        internal int Ordinal { get; set; }
        virtual internal string NewColumn { get; set; }
        public string Name { get; set; }
        internal bool CopyOldColumn { get; set; }
        internal WorkProc() { }

        internal WorkProc(int ordinal, int id, string name)
        {
            SetColumns();
            Ordinal = ordinal;
            ProcedureId = id;
            DuplicateColumns = new string[0];
            Name = name;

        }

        protected void SetColumns()
        {
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
        }

        public int CompareTo(WorkProc other)
        {
            return Ordinal.CompareTo(other.Ordinal);
        }

        public bool Equals(WorkProc other)
        {
            return other.Ordinal == Ordinal;
        }

        public override int GetHashCode()
        {
            var hashCode = -1241368578;
            hashCode = hashCode * -1521134295 + ProcedureId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<DataTable>.Default.GetHashCode(Columns);
            hashCode = hashCode * -1521134295 + Ordinal.GetHashCode();
            return hashCode;
        }

        protected bool PrepareMultiple(string[] columns, Form1 invokeForm, string tableName, out string[] sourceColumns, out string[] destinationColumns)
        {
            sourceColumns = invokeForm.DatabaseHelper.GetColumnNames(columns, tableName);

            if (CopyOldColumn)
            {
                destinationColumns = invokeForm.DatabaseHelper.CopyColumns(columns, tableName);
            }
            else if (!string.IsNullOrWhiteSpace(NewColumn))
            {
                invokeForm.DatabaseHelper.AddColumnsWithDialog(NewColumn, columns, invokeForm, tableName, out destinationColumns);
            }
            else
            {
                destinationColumns = sourceColumns;
            }
            return destinationColumns != null;
        }

        protected bool PrepareSingle(ref string sourceColumn, Form1 invokeForm, string tableName, out string destinationColumn)
        {
            sourceColumn = invokeForm.DatabaseHelper.GetColumnName(sourceColumn, tableName);
            destinationColumn = sourceColumn;
            if (CopyOldColumn)
            {
                destinationColumn = invokeForm.DatabaseHelper.CopyColumn(sourceColumn, tableName);
            }
            else if (!string.IsNullOrWhiteSpace(NewColumn))
            {
                invokeForm.DatabaseHelper.AddColumnWithDialog(NewColumn, invokeForm, tableName, out destinationColumn);
            }
            return destinationColumn != null;
        }

        abstract public string[] GetHeaders();

        abstract public void RenameHeaders(string oldName, string newName);
        abstract public void RemoveHeader(string colName);
        abstract public void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName = "main");
    }
}
