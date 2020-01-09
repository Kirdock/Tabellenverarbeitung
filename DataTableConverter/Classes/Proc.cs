using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    [Serializable()]
    class Proc : IComparable<Proc>, IEquatable<Proc>
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public DataTable Replace { get; set; }
        public bool CheckTotal { get; set; }
        public bool Locked { get; set; }
        public bool CheckWord { get; set; }
        public bool LeaveEmpty { get; set; }

        public Proc(string name, DataTable replace, int id)
        {
            Name = name;
            Replace = replace;
            Id = id;
        }

        public Proc(DataTable replace, bool checkTotal, bool checkWord, bool leaveEmpty)
        {
            Replace = replace;
            CheckTotal = checkTotal;
            CheckWord = checkWord;
            LeaveEmpty = leaveEmpty;
        }


        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(Proc proc)
        {
            return Name.CompareTo(proc.Name);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Proc);
        }

        public bool Equals(Proc other)
        {
            return other != null &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            var hashCode = 1555994313;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<DataTable>.Default.GetHashCode(Replace);
            return hashCode;
        }
    }
}
