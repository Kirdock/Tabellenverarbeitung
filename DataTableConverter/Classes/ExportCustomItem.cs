﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    class ExportCustomItem
    {
        internal string Column;
        internal string Name;
        internal int Format;
        internal Dictionary<string, bool> Values;
        internal IEnumerable<string> SelectedValues => GetSelectedValues();
        internal IEnumerable<string> AllValues => GetAllValues();
        internal bool CheckedAllValues;

        internal ExportCustomItem(string name, string column) {
            Name = name;
            Column = column;
            Values = new Dictionary<string, bool>();
        }

        internal void SetValues(IEnumerable<string> allValues, bool status)
        {
            Values.Clear();
            foreach (string value in allValues)
            {
                Values.Add(value, status);
            }
        }

        private IEnumerable<string> GetSelectedValues()
        {
            return Values.Where(item => item.Value).Select(x => x.Key);
        }

        private IEnumerable<string> GetAllValues()
        {
            return Values.Keys;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
