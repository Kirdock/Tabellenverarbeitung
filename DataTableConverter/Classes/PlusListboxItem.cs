﻿using System.Collections.Generic;

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
            if (State == RowMergeState.Nothing)
            {
                State = RowMergeState.Sum;
            }
            else if (State == RowMergeState.Sum)
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
