using DataTableConverter.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Assisstant
{
    class CellMatrix
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public string Value { get; set; }
        public History bigChange { get; set; }

        public CellMatrix(int rowIndex, int columnIndex, string value)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            Value = value;
        }

        public CellMatrix(History history)
        {
            bigChange = history;
        }
    }
}
