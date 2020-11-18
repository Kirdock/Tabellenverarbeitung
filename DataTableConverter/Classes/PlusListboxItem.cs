using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    class PlusListboxItem
    {
        public enum RowMergeState { Sum, Count, Nothing }
        internal static List<KeyValuePair<string, int>> RowMergeStateList = new List<KeyValuePair<string, int>>()
        {
            new KeyValuePair<string, int>("Anhängen", 2),
            new KeyValuePair<string, int>("Anzahl", 1),
            new KeyValuePair<string, int>("Summe", 0),
        };
        internal RowMergeState State = RowMergeState.Nothing;
        internal string Value;
        internal string DisplayValue;

        public PlusListboxItem(string value, string displayValue)
        {
            Value = value;
            DisplayValue = displayValue;
        }

        public PlusListboxItem(RowMergeState state, string value)
        {
            State = state;
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }

        internal void Next()
        {
            if(State == RowMergeState.Nothing)
            {
                State = RowMergeState.Sum;
            }
            else if(State == RowMergeState.Sum)
            {
                State = RowMergeState.Count;
            }
            else
            {
                State = RowMergeState.Nothing;
            }
        }


    }
}
