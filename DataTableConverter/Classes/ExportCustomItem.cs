﻿using System.Collections.Generic;
using System.Linq;

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

        internal ExportCustomItem() { }

        internal ExportCustomItem(string name, string column, IEnumerable<string> values)
        {
            Name = name;
            Column = column;
            Values = new Dictionary<string, bool>();
            foreach (string value in values)
            {
                Values.Add(value, true);
            }
        }

        internal ExportCustomItem(string name, string column, IEnumerable<string> values, int format, bool checkedAllValues)
        {
            Name = name;
            Column = column;
            Format = format;
            Values = new Dictionary<string, bool>();
            CheckedAllValues = checkedAllValues;
            foreach (string value in values)
            {
                Values.Add(value, true);
            }
        }

        internal ExportCustomItem(string name, string column, int format = 1)
        {
            Name = name;
            Column = column;
            Values = new Dictionary<string, bool>();
            Format = format;
        }

        internal void SetValues(IEnumerable<string> allValues, bool status, IEnumerable<ExportCustomItem> dict)
        {
            Values.Clear();
            if (status && !Properties.Settings.Default.SeparateSelectable)
            {
                IEnumerable<string> values = dict.SelectMany(item => item.SelectedValues);
                foreach (string value in allValues)
                {
                    Values.Add(value, !values.Contains(value));
                }
            }
            else
            {
                string[] a = allValues.ToArray();
                foreach (string value in allValues)
                {
                    Values.Add(value, status);
                }
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
