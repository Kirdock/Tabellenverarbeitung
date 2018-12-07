using DataTableConverter.Assisstant;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    class History
    {
        public State State { get; set; }
        public List<CellMatrix> Table { get; set; }
        public object[][] Row { get; set; }
        public DataColumn[] Column { get; set; }
        public string NewText { get; set; }
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public object[][] ColumnValues { get; set; }
        public string Order { get; set; }
        

        public History() { }

    }
}
