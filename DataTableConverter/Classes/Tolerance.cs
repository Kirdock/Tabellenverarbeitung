using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataTableConverter.Classes
{
    [Serializable()]
    class Tolerance : IComparable<Tolerance>, IEquatable<Tolerance>
    {
        public string Name { get; set; }
        public DataTable Columns { get; set; }

        public int Id { get; set; }
        internal bool Locked { get; set; }

        public Tolerance(string name, DataTable columns, int id = 0)
        {
            Name = name;
            Columns = columns;
            Id = id;
        }

        public string[] getColumnsAsArrayToLower()
        {
            return Columns.Rows.Cast<DataRow>().Select(dt => dt.ItemArray[0].ToString().ToLower()).ToArray();
        }

        public int CompareTo(Tolerance other)
        {
            return Name.CompareTo(other.Name);
        }

        public bool Equals(Tolerance other)
        {
            return Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            var hashCode = -1128376926;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<DataTable>.Default.GetHashCode(Columns);
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
