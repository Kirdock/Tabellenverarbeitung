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
        internal static readonly string FileName = "Dateiname";
        internal static readonly string TempSort = "[TEMP_SORT]";
        internal static readonly string OldAffix = " Alt";
        internal static object[] HeadersOfDataTable(DataTable table)
        {
            return table != null ? table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray() : new object[0];
        }

        internal static object[] ColumnValues(DataTable table, int column)
        {
            object[] columnValues = new object[table.Rows.Count];

            for (int i = 0; i < table.Rows.Count; i++)
            {
                columnValues[i] = table.Rows[i][column];
            }
            return columnValues;
        }

        internal static void CopyColumns(string[] columns, DataTable table)
        {
            string[] oldColumns = new string[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                string oldName = AddColumn(columns[i] + OldAffix, table);
                oldColumns[i] = oldName;
            }
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    row[oldColumns[i]] = row[columns[i]];
                }
            }
        }

        internal static void RemoveNull(DataTable table)
        {
            foreach(DataRow row in table.Rows)
            {
                for(int i = 0; i < row.ItemArray.Length; i++)
                {
                    if(row[i] == DBNull.Value)
                    {
                        row[i] = string.Empty;
                    }
                }
            }
        }

        internal static string AddColumn(string headerName, DataTable data, int counter = 0)
        {
            string result;
            string name = counter == 0 ? headerName : headerName + counter;
            if (data.Columns.Contains(name))
            {
                counter++;
                result = AddColumn(headerName, data, counter);
            }
            else
            {
                result = name;
                data.Columns.Add(name);
            }
            return result;
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

        internal static void SetHeaders(DataTable table, DataTable oldTable)
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

        internal static List<string> HeadersToLower(DataTable table)
        {
            return table.Columns.Cast<DataColumn>().Select(dt => dt.ColumnName.ToLower()).ToList();
        }

        internal static void ConcatTables(DataTable originalTable, DataTable table, string originalFilename, string secondFilename)
        {
            List<int> ColumnIndizes = new List<int>();
            table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList().ForEach(x => {
                int index = originalTable.Columns.IndexOf(x);
                if (index == -1)
                {
                    index = originalTable.Columns.Count;
                    originalTable.Columns.Add(x);
                }
                ColumnIndizes.Add(index);
            });
            int filenameColumnIndex;
            if ((filenameColumnIndex = originalTable.Columns.IndexOf(FileName)) == -1)
            {
                AddColumn(FileName, originalTable);
                int colIndex = filenameColumnIndex = originalTable.Columns.Count - 1;
                foreach (DataRow row in originalTable.Rows)
                {
                    row[colIndex] = originalFilename;
                }
            }

            foreach (DataRow row in table.Rows)
            {
                object[] itemArray = new object[originalTable.Columns.Count];
                int count = 0;
                string secondName = secondFilename;
                try
                {
                    secondName = row[FileName]?.ToString() ?? secondFilename;
                }
                catch { }
                foreach (int index in ColumnIndizes)
                {
                    itemArray[index] = row.ItemArray[count];
                    count++;
                }
                itemArray[filenameColumnIndex] = secondName;
                originalTable.Rows.Add(itemArray);
            }
        }

        internal static List<CellMatrix> ChangesOfDataTable(DataTable tableNew)
        {
            List<CellMatrix> result = new List<CellMatrix>();

            //getModified
            int tempIndex;
            if ((tempIndex = tableNew.Columns.IndexOf(TempSort)) != -1)
            {
                tableNew.Columns[tempIndex].SetOrdinal(tableNew.Columns.Count - 1);
            }
            DataTable changes = tableNew.GetChanges(DataRowState.Modified);
            HashSet<int> newColumns = new HashSet<int>();
            for (int rowIndex = 0; changes != null && rowIndex < changes.Rows.Count; rowIndex++)
            {

                for(int colIndex = 0; colIndex < changes.Rows[rowIndex].ItemArray.Length; colIndex++)
                {
                    object value;
                    if( (value = changes.Rows[rowIndex][colIndex,DataRowVersion.Original]) != changes.Rows[rowIndex][colIndex, DataRowVersion.Current])
                    {
                        if (value == DBNull.Value)
                        {
                            newColumns.Add(colIndex);
                        }
                        else
                        {
                            result.Add(new CellMatrix(rowIndex, colIndex, value.ToString()));
                        }
                    }
                }
            }

            //deleted rows
            changes = tableNew.GetChanges(DataRowState.Deleted);
            if (changes != null && tempIndex != -1)
            {
                foreach (DataRow row in changes.Rows)
                {
                    object[][] newItem = new object[1][];
                    List<object> newList = new List<object>();
                    for (int i = 0; i < changes.Columns.Count - 1; i++) //we don't look at TempSort
                    {
                        object value;
                        if ((value = row[i, DataRowVersion.Original]) != DBNull.Value)
                        {
                            newList.Add(value);
                        }
                    }
                    newItem[0] = newList.ToArray();
                    result.Add(new CellMatrix(new History { State = State.DeleteRow, Row = newItem, RowIndex = int.Parse(row[TempSort,DataRowVersion.Original].ToString()) }));
                }
            }

            //added columns
            foreach (int index in newColumns)
            {
                result.Add(new CellMatrix(new History { State = State.InsertColumn, ColumnIndex = index }));
            }

            return result;
        }

        internal static List<int> HeaderIndices(DataTable table, string[] columns)
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

        internal static string AddStringAsFloat(string a, string b)
        {
            float.TryParse(a, out float fa);
            float.TryParse(b, out float fb);
            return (fa + fb).ToString();
        }
    }
}
