using System;
using System.Collections.Generic;

namespace DataTableConverter.Classes
{
    [Serializable()]
    class Work : IComparable<Work>, IEquatable<Work>
    {
        public string Name { get; set; }
        public List<WorkProc> Procedures { get; set; }
        public int Id { get; set; }

        public Work(string name, List<WorkProc> procedures, int id)
        {
            Name = name;
            Procedures = procedures;
            Id = id;
        }


        public int CompareTo(Work other)
        {
            return Name.CompareTo(other.Name);
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Work);
        }

        public bool Equals(Work other)
        {
            return other != null &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            var hashCode = 1547675429;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<WorkProc>>.Default.GetHashCode(Procedures);
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            return hashCode;
        }
    }
}
