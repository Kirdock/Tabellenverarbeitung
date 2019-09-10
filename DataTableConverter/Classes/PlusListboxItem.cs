using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    class PlusListboxItem
    {
        public enum RowMergeState { Sum, Count, Nothing}
        internal RowMergeState State = RowMergeState.Nothing;
        internal string Value;

        public PlusListboxItem(string value)
        {
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
