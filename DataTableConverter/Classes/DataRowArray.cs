using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    class DataRowArray
    {
        internal List<List<string>> Values;
        internal DataRow DataRow;

        internal DataRowArray(DataRow dataRow, List<string> values)
        {
            Values = new List<List<string>>
            {
                values
            };
            DataRow = dataRow;
        }

        internal void Add(List<string> values)
        {
            Values.Add(values);
        }

    }
}
