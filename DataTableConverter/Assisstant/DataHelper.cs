using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataTableConverter.Assisstant
{
    class DataHelper
    {

        internal static DataTable DictionaryToDataTable(Dictionary<string, int> dict, string columnName, bool showFromTo)
        {
            DataTable result = new DataTable();
            if (dict.Count == 0)
            {
                return result;
            }

            var columnNames = dict.Keys;
            if (showFromTo)
            {
                result.Columns.AddRange(new string[] { columnName, "Anzahl", "Von", "Bis" }.Select(c => new DataColumn(c,typeof(string))).ToArray());
                int count = 1;
                foreach (KeyValuePair<string, int> item in dict)
                {
                    int newCount = count + item.Value;
                    result.Rows.Add(new object[] { item.Key, item.Value.ToString(), count.ToString(), (newCount - 1).ToString() });
                    count = newCount;
                }
            }
            else
            {
                result.Columns.AddRange(new string[] { columnName, "Anzahl" }.Select(c => new DataColumn(c, typeof(string))).ToArray());
                foreach (KeyValuePair<string, int> item in dict)
                {
                    result.Rows.Add(new object[] { item.Key, item.Value.ToString()});
                }
            }

            return result;
        }
        
    }
}
