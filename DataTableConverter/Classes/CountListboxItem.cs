using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    class CountListboxItem
    {
        internal int Count;
        internal object Value;

        public CountListboxItem(int count, object value)
        {
            Count = count;
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
