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
        internal string Formula { get; set; }
        public string Name { get; set; }
        internal bool CopyOldColumn { get; set; }
        internal WorkProc() { }

        internal WorkProc(int ordinal, int id, string name)
        {
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            Ordinal = ordinal;
            ProcedureId = id;
            DuplicateColumns = new string[0];
            Name = name;

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

        abstract public string[] GetHeaders();

        abstract public void renameHeaders(string oldName, string newName);

        abstract public void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow);
    }
}
