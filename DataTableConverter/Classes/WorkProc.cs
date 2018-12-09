using System;
using System.Collections.Generic;
using System.Data;

namespace DataTableConverter.Classes
{
    [Serializable()]
    class WorkProc : IComparable<WorkProc>, IEquatable<WorkProc>
    {
        public int ProcedureId { get; set; }
        public DataTable Columns { get; set; }
        public string[] DuplicateColumns { get; set; }
        public int Ordinal { get; set; }
        public ProcedureState Type { get; set; }
        public string NewColumn { get; set; }
        public string Formula { get; set; }
        public string[] Headers { get; set; }
        public string Name { get; set; }

        public WorkProc(int ordinal, int id, int type, string name)
        {
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            Ordinal = ordinal;
            ProcedureId = id;
            DuplicateColumns = new string[0];
            Name = name;


            switch (type)
            {
                //System-Proc
                case 1:
                    switch (id)
                    {
                        case 1:
                            Type = ProcedureState.Trim;
                            break;

                        case 2:
                            Type = ProcedureState.Merge;
                            break;

                        default:
                            Type = ProcedureState.Order;
                            break;
                    }
                    break;

                //Duplicate
                case 2:
                    Type = ProcedureState.Duplicate;
                    break;

                //User-Proc
                default:
                    Type = ProcedureState.User;
                    break;
            }

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
    }
}
