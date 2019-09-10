﻿using System;
using System.Collections.Generic;
using System.Data;
using DataTableConverter.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    [Serializable()]
    class ExportSeparate
    {
        public string Column;
        public string Name;
        public int Format;
        public DataTable Table;
        internal IEnumerable<string> Values => Table.ColumnValuesAsString(0);
        internal bool CheckedAllValues;

        internal ExportSeparate(string name, string column)
        {
            Format = 0;
            Name = name;
            Column = column;
            Table = new DataTable { TableName = "ExportSeparate" };
            Table.Columns.Add("Werte", typeof(string));
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}