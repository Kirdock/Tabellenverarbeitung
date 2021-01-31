using System;
using System.Collections.Generic;

namespace DataTableConverter.Classes
{
    internal class CountListboxItem : IEquatable<CountListboxItem>
    {
        internal int Count;
        internal object Value;

        public CountListboxItem(int count, object value)
        {
            Count = count;
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CountListboxItem);
        }

        public bool Equals(CountListboxItem other)
        {
            return other != null &&
                   EqualityComparer<object>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            var hashCode = -1663980258;
            hashCode = hashCode * -1521134295 + Count.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(Value);
            return hashCode;
        }

        public override string ToString()
        {
            return Value.ToString();
        }


    }
}
