using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    class PlusListboxItem
    {
        public enum RowMergeState { Summe, Anzahl, Nichts}
        internal RowMergeState State = RowMergeState.Nichts;
        internal string Value;

        public PlusListboxItem(string value)
        {
            Value = value;
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
            if(State == RowMergeState.Nichts)
            {
                State = RowMergeState.Summe;
            }
            else if(State == RowMergeState.Summe)
            {
                State = RowMergeState.Anzahl;
            }
            else
            {
                State = RowMergeState.Nichts;
            }
        }


    }
}
