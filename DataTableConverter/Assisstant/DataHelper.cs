using DataTableConverter.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Assisstant
{
    class DataHelper
    {
        internal static object[] getHeadersOfDataTable(DataTable table)
        {
            return table != null ? table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray() : new object[0];
        }

        internal static object[] getColumnValues(DataTable table, int column)
        {
            object[] columnValues = new object[table.Rows.Count];

            for (int i = 0; i < table.Rows.Count; i++)
            {
                columnValues[i] = table.Rows[i][column];
            }
            return columnValues;
        }

        internal static void addColumn(string headerName, DataTable data, int counter = 0)
        {
            string name = counter == 0 ? headerName : headerName + counter;
            if (data.Columns.Contains(name))
            {
                counter++;
                addColumn(headerName, data, counter);
            }
            else
            {
                data.Columns.Add(name);
            }
        }

        internal static DataTable DictionaryToDataTable(Dictionary<string, int> dict, string columnName)
        {
            DataTable result = new DataTable();
            if (dict.Count == 0)
            {
                return result;
            }

            var columnNames = dict.Keys;
            result.Columns.AddRange(new string[] { columnName, "Anzahl", "Von", "Bis" }.Select(c => new DataColumn(c)).ToArray());
            int count = 1;
            foreach (KeyValuePair<string, int> item in dict)
            {
                int newCount = count + item.Value;
                result.Rows.Add(new object[] { item.Key, item.Value.ToString(), count.ToString(), (newCount - 1).ToString() });
                count = newCount;
            }

            return result;
        }

        internal static void setHeaders(DataTable table, DataTable oldTable)
        {
            string[] newHeaders = table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            for (int i = 0; i < newHeaders.Length && i < oldTable.Columns.Count; i++)
            {
                int index;
                if ((index = oldTable.Columns.IndexOf(newHeaders[i])) != -1)
                {
                    oldTable.Columns[index].ColumnName = oldTable.Columns[index].ColumnName + "1";
                }
                oldTable.Columns[i].ColumnName = newHeaders[i];
            }
        }

        internal static List<string> getHeadersToLower(DataTable table)
        {
            return table.Columns.Cast<DataColumn>().Select(dt => dt.ColumnName.ToLower()).ToList();
        }

        internal static List<CellMatrix> getChangesOfDataTable(DataTable tableOld, DataTable tableNew, int columnIndex)
        {
            //Löschen und Hinzufügen von Zeilen und Löschen von Spalten nicht berücksichtigt

            List<CellMatrix> result = new List<CellMatrix>();
            int beginIndex = 0;
            int endIndex = tableOld.Columns.Count - 1;
            if (columnIndex != -1)
            {
                beginIndex = columnIndex;
                endIndex = columnIndex + 1;
            }

            object[] oldHeaders = getHeadersOfDataTable(tableOld);
            object[] newHeaders = getHeadersOfDataTable(tableNew);

            for (int colIndexOld = beginIndex; colIndexOld < endIndex; colIndexOld++)
            {
                string oldColName = tableOld.Columns[colIndexOld].ColumnName;
                int colIndexNew = tableNew.Columns.IndexOf(oldColName);

                if (colIndexNew != -1)
                {
                    for (int rowIndex = 0; rowIndex < tableOld.Rows.Count; rowIndex++)
                    {
                        if (tableOld.Rows[rowIndex].ItemArray[colIndexOld]?.ToString() != tableNew.Rows[rowIndex].ItemArray[colIndexNew]?.ToString())
                        {
                            result.Add(new CellMatrix(rowIndex, colIndexOld, tableOld.Rows[rowIndex].ItemArray[colIndexOld]?.ToString()));
                        }
                    }
                }
            }

            if (oldHeaders.Length < newHeaders.Length)
            {
                for (int i = newHeaders.Length - 1; i >= 0; i--)
                {
                    //Spalte wurde hinzugefügt
                    if (!oldHeaders.Contains(newHeaders[i]))
                    {

                        result.Add(new CellMatrix(new History { State = State.InsertColumn, ColumnIndex = i }));
                    }
                }
            }
            return result;
        }

        internal static List<int> getHeaderIndices(DataTable table, string[] columns)
        {
            List<int> indices = new List<int>();
            foreach (string col in columns)
            {
                int index = table.Columns.IndexOf(col);
                if (index != -1)
                {
                    indices.Add(index);
                }
            }

            return indices;
        }
    }
}
