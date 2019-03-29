using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataTableConverter.Classes
{
    [Serializable()]
    class Case : IComparable<Case>, IEquatable<Case>
    {
        public string Name { get; set; }
        public string Shortcut { get; set; }
        public DataTable Columns { get; set; }
        internal string ShortcutTotal { get; set; }

        public int Id { get; set; }

        public Case(string name, string shortcut, DataTable columns, int id)
        {
            Name = name;
            Shortcut = shortcut;
            Columns = columns;
            Id = id;
        }

        public string[] getColumnsAsArray()
        {
            return Columns.Rows.Cast<DataRow>().Select(dt => dt.ItemArray[0].ToString()).ToArray();
        }

        public int[] getBeginSubstring()
        {
            return Columns.Rows.Cast<DataRow>().Select(dt => dt.ItemArray[1].ToString() == string.Empty ? 0 : (int)dt.ItemArray[1]).ToArray();
        }

        public int[] getEndSubstring()
        {
            return Columns.Rows.Cast<DataRow>().Select(dt => dt.ItemArray[2].ToString() == string.Empty ? 0 : (int)dt.ItemArray[2]).ToArray();
        }

        public int CompareTo(Case other)
        {
            return Name.CompareTo(other.Name);
        }

        public bool Equals(Case other)
        {
            return Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            var hashCode = -1457353003;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Shortcut);
            hashCode = hashCode * -1521134295 + EqualityComparer<DataTable>.Default.GetHashCode(Columns);
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            return hashCode;
        }
    }
}
