using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace DataTableConverter.Extensions
{
    internal static class DataTableExtensions
    {
        private static readonly string TempSort = "[TEMP_SORT]";
        internal static readonly string FileName = "Dateiname";

        internal static DataTable RemoveEmptyRows(this DataTable table)
        {
            return table.AsEnumerable().Where(row => row.ItemArray.Any(item => !string.IsNullOrWhiteSpace(item?.ToString()))).ToTable(table);
        }

        internal static List<string> HeadersToLower(this DataTable table)
        {
            return table.Columns.Cast<DataColumn>().Select(dt => dt.ColumnName.ToLower()).ToList();
        }

        internal static string[] HeadersOfDataTableAsString(this DataTable table)
        {
            return table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
        }

        internal static void OverrideHeaders(this DataTable sourceTable, DataTable table)
        {
            OverrideHeaders(sourceTable, table.HeadersOfDataTableAsString());
        }

        internal static void OverrideHeaders(this DataTable table, string[] headers)
        {
            for (int i = 0; i < headers.Length && i < table.Columns.Count; i++)
            {
                int index;

                if ((index = table.Columns.IndexOf(headers[i])) != -1)
                {
                    int counter = 1;
                    for (; table.Columns.IndexOf(headers[i] + counter) != -1; counter++) { }
                    table.Columns[index].ColumnName = table.Columns[index].ColumnName + counter;
                }
                table.Columns[i].ColumnName = headers[i];
            }
        }

        internal static DataTable ToTable(this EnumerableRowCollection<DataRow> rows, DataTable table)
        {
            return rows.Any() ? rows.CopyToDataTable() : table.Clone();
        }

        internal static string TryAddColumn(this DataTable data, string headerName, int counter = 0)
        {
            string result;
            string name = counter == 0 ? headerName : headerName + counter;
            if (data.Columns.Contains(name))
            {
                counter++;
                result = TryAddColumn(data, headerName, counter);
            }
            else
            {
                result = name;
                data.Columns.Add(name, typeof(string));
            }
            return result;
        }

        internal static void CopyColumns(this DataTable table, string[] columns)
        {
            string[] oldColumns = new string[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                string oldName = table.TryAddColumn(columns[i] + Properties.Settings.Default.OldAffix);
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

        internal static Dictionary<string, int> GroupCountOfColumn(this DataTable table, int columnIndex)
        {
            Dictionary<string, int> pair = new Dictionary<string, int>();
            foreach (DataRow row in table.Rows)
            {
                string item = row[columnIndex].ToString();
                if (pair.ContainsKey(item))
                {
                    pair[item] = pair[item] + 1;
                }
                else
                {
                    pair.Add(item, 1);
                }
            }
            return pair;
        }

        internal static List<int> HeaderIndices(this DataTable table, string[] columns)
        {
            return columns.Select(column => table.Columns.IndexOf(column)).Where(index => index != -1).ToList();
        }

        internal static List<CellMatrix> ChangesOfDataTable(this DataTable tableNew)
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

                for (int colIndex = 0; colIndex < changes.Rows[rowIndex].ItemArray.Length; colIndex++)
                {
                    object value;
                    if ((value = changes.Rows[rowIndex][colIndex, DataRowVersion.Original]) != changes.Rows[rowIndex][colIndex, DataRowVersion.Current])
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
                    result.Add(new CellMatrix(new History { State = State.DeleteRow, Row = newItem, RowIndex = int.Parse(row[TempSort, DataRowVersion.Original].ToString()) }));
                }
            }

            //added columns
            foreach (int index in newColumns)
            {
                result.Add(new CellMatrix(new History { State = State.InsertColumn, ColumnIndex = index }));
            }

            return result;
        }

        internal static void SplitDataTable(this DataTable table, string path, string columnName = null)
        {
            DataTable table1 = new DataTable() { TableName = path.AppendFileName(Properties.Settings.Default.FailAddressText) };
            string invalidColumnName = columnName ?? Properties.Settings.Default.InvalidColumnName;
            if (!table.Columns.Contains(invalidColumnName))
            {
                return;
            }
            DataTable table2;
            foreach (DataColumn column in table.Columns)
            {
                table1.Columns.Add(column.ColumnName, typeof(string));
            }
            table2 = table1.Copy();
            table2.TableName = path.AppendFileName(Properties.Settings.Default.RightAddressText);


            foreach (DataRow row in table.Rows)
            {
                string value = row[invalidColumnName].ToString();
                if (value == Properties.Settings.Default.FailAddressValue)
                {
                    table1.ImportRow(row);
                }
                else
                {
                    table2.ImportRow(row);
                }
            }
            path = Path.GetDirectoryName(path);
            foreach (DataTable Table in new DataTable[] { table1, table2 })
            {
                string FileName = Table.TableName;
                switch (Properties.Settings.Default.PVMSaveFormat)
                {
                    //CSV
                    case 0:
                        {
                            ExportHelper.ExportCsv(Table, path, FileName);
                        }
                        break;

                    //Dbase
                    case 1:
                        {
                            ExportHelper.ExportDbase(FileName, Table, path);
                        }
                        break;

                    //Excel
                    case 2:
                        {
                            ExportHelper.ExportExcel(Table, path, FileName);
                        }
                        break;
                }
            }
        }
        internal static void RemoveNull(this DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    if (row[i] == DBNull.Value)
                    {
                        row[i] = string.Empty;
                    }
                }
            }
        }

        internal static object[] HeadersOfDataTable(this DataTable table)
        {
            return table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray() ?? new object[0];
        }

        internal static object[] ColumnValues(this DataTable table, int column)
        {
            return table.AsEnumerable().Select(row => row[column]).ToArray();
        }

        internal static void SetColumnsTypeString(this DataTable table)
        {
            foreach(DataColumn col in table.Columns)
            {
                col.DataType = typeof(string);
            }
        }

        internal static void ConcatTable(this DataTable originalTable, DataTable table, string originalFilename, string secondFilename)
        {
            List<int> ColumnIndizes = new List<int>();
            table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList().ForEach(x => {
                int index = originalTable.Columns.IndexOf(x);
                if (index == -1)
                {
                    index = originalTable.Columns.Count;
                    originalTable.TryAddColumn(x);
                }
                ColumnIndizes.Add(index);
            });
            int filenameColumnIndex;
            if ((filenameColumnIndex = originalTable.Columns.IndexOf(FileName)) == -1)
            {
                originalTable.TryAddColumn(FileName);
                int colIndex = filenameColumnIndex = originalTable.Columns.Count - 1;
                foreach (DataRow row in originalTable.Rows)
                {
                    row[colIndex] = originalFilename;
                }
            }
            bool containsName = table.Columns.Contains(FileName);

            foreach (DataRow row in table.Rows)
            {
                object[] itemArray = new object[originalTable.Columns.Count];
                int count = 0;

                foreach (int index in ColumnIndizes)
                {
                    itemArray[index] = row.ItemArray[count];
                    count++;
                }
                itemArray[filenameColumnIndex] = containsName ? row[FileName]?.ToString() : secondFilename;
                originalTable.Rows.Add(itemArray);
            }
        }

        internal static void AddColumnsOfDataTable(this DataTable sourceTable, DataTable importTable, string[] ImportColumns, int SourceMergeIndex, int ImportMergeIndex, bool SortColumn, string orderColumnName, ProgressBar pgbLoading = null)
        {
            int oldCount = sourceTable.Columns.Count;
            int newColumnIndex = oldCount + ImportColumns.Length - 1; //-1: without identifier

            for (int i = 0; i < ImportColumns.Length; i++)
            {
                if (i == ImportMergeIndex) continue;

                sourceTable.TryAddColumn(ImportColumns[i]);
            }
            if (SortColumn)
            {
                string SortColumnName = "[Sortierung]";
                sourceTable.TryAddColumn(orderColumnName);
                int LastIndex = importTable.Columns.Count;
                importTable.TryAddColumn(SortColumnName);
                for (int i = 0; i < importTable.Rows.Count; i++)
                {
                    importTable.Rows[i][LastIndex] = i.ToString();
                }
                ImportColumns = new List<string>(ImportColumns)
                            {
                                SortColumnName
                            }.ToArray();
            }

            pgbLoading?.Invoke(new MethodInvoker(() => { pgbLoading.Value = 0; pgbLoading.Maximum = importTable.Rows.Count; }));


            HashSet<int> hs = new HashSet<int>();


            for (int y = 0; y < sourceTable.Rows.Count; y++)
            {
                DataRow source = sourceTable.Rows[y];

                for (int rowIndex = 0; rowIndex < importTable.Rows.Count; rowIndex++)
                {

                    DataRow row = importTable.Rows[rowIndex];
                    if (source.ItemArray[SourceMergeIndex].ToString() == row.ItemArray[ImportMergeIndex].ToString())
                    {
                        int Offset = 0;
                        for (int i = 0; i < ImportColumns.Length; i++)
                        {
                            if (i == ImportMergeIndex)
                            {
                                Offset++;
                            }
                            else
                            {
                                source.SetField(oldCount + i - Offset, row.ItemArray[i]);
                            }
                        }
                        importTable.Rows.RemoveAt(rowIndex);
                        break;

                    }
                }

                pgbLoading?.BeginInvoke(new MethodInvoker(() => { pgbLoading.Value++; }));
            }
        }

        internal static IEnumerable<DataRow> GetSortedTable(this DataTable table, string order, OrderType orderType, Action addHistory = null)
        {
            IEnumerable<DataRow> result;
            Dictionary<string, SortOrder> dict = ViewHelper.GenerateSortingList(order);
            if (dict.Count == 0)
            {
                result = table.AsEnumerable().OrderBy(field => true);
            }
            else
            {
                var firstElement = dict.First();

                if (orderType != OrderType.Windows && table.Rows.Count % 2 == 1)
                {
                    DataRow row = table.NewRow();
                    row[firstElement.Key] = "0";
                    table.Rows.Add(row);
                    addHistory?.Invoke();
                }

                var enum2 = table.AsEnumerable().OrderBy(field => field.Field<string>(firstElement.Key), new NaturalStringComparer(firstElement.Value));

                foreach (var column in dict.Skip(1))
                {
                    enum2 = enum2.ThenBy(field => field.Field<string>(column.Key), new NaturalStringComparer(column.Value));
                }

                if (orderType != OrderType.Windows)
                {
                    bool flag = firstElement.Value == SortOrder.Ascending;
                    int half = table.Rows.Count / 2;

                    IEnumerable<DataRow> one = enum2.Take(half);
                    IEnumerable<DataRow> two = enum2.Skip(half);
                    if (flag)
                    {
                        result = one.InterleaveEnumerationsOfEqualLength(two);
                    }
                    else
                    {
                        result = two.InterleaveEnumerationsOfEqualLength(one);
                    }
                }
                else
                {
                    result = enum2;
                }
            }
            return result;
        }

        internal static DataView GetSortedView(this DataTable table, string order, OrderType orderType, Action addHistory = null)
        {
            DataView view;
            table.AcceptChanges();
            Dictionary<string, SortOrder> dict = ViewHelper.GenerateSortingList(order);
            if (dict.Count == 0)
            {
                view = table.AsEnumerable().OrderBy(field => true).AsDataView();
            }
            else
            {
                var firstElement = dict.First();
                string tempSortName = string.Empty;


                if (orderType != OrderType.Windows)
                {
                    if (table.Rows.Count % 2 == 1)
                    {
                        DataRow row = table.NewRow();
                        row[firstElement.Key] = "0";
                        table.Rows.Add(row);
                        addHistory?.Invoke();
                    }

                    tempSortName = table.TryAddColumn(TempSort);
                }

                var enumerable = table.AsEnumerable().OrderBy(field => field.Field<string>(firstElement.Key), new NaturalStringComparer(firstElement.Value));

                foreach (var column in dict.Skip(1))
                {
                    enumerable = enumerable.ThenBy(field => field.Field<string>(column.Key), new NaturalStringComparer(column.Value));
                }

                if (orderType != OrderType.Windows)
                {
                    int half = enumerable.Count() / 2;

                    int count = 0;
                    foreach (DataRow row in enumerable)//lazy loading. this does not always run before the next (enumerable.OrderBy...) statement
                    {
                        row[tempSortName] = count++;
                    }

                   
                    enumerable = enumerable.OrderBy(row =>
                    {
                        if (table.Columns.IndexOf(tempSortName) != -1 && int.TryParse(row[tempSortName]?.ToString(), out int res)) //everything here is triggered if row[tempSortName] is set... or before it is set... idk... wtf. so there is a new column but there are not any indices written (or just one)
                        {
                            int position = res + 1;

                            bool upperHalf = position > half;
                            if (upperHalf)
                            {
                                position -= half;
                            }

                            return new CustomSortItem(position, upperHalf);
                        }
                        else
                        {
                            return new CustomSortItem(0, false);
                        }
                    }, new CustomSort(firstElement.Value == SortOrder.Ascending));
                }
                view = enumerable.AsDataView();

                if (tempSortName != string.Empty)
                {
                    view.Table.Columns.Remove(tempSortName);
                }
            }
            return view;
        }

        internal static IEnumerable<DataRow> InterleaveEnumerationsOfEqualLength<DataRow>(this IEnumerable<DataRow> first, IEnumerable<DataRow> second)
        {
            using (IEnumerator<DataRow> enumerator1 = first.GetEnumerator(), enumerator2 = second.GetEnumerator())
            {
                while (enumerator1.MoveNext() && enumerator2.MoveNext())
                {
                    yield return enumerator1.Current;
                    yield return enumerator2.Current;
                }
            }
        }

        internal static string AddTempNumeration(this DataTable table)
        {
            int lastIndex = table.Columns.Count;
            string name = table.TryAddColumn(TempSort);
            
            for (int i = 0; i < table.Rows.Count; i++)
            {
                table.Rows[i][lastIndex] = i;
            }
            return name;
        }


        internal static string MergeRows(this DataTable table, string identifier, int identifierIndex, List<PlusListboxItem> additionalColumns, OrderType orderType, Action<string> SetStatusLabel)
        {
            SetStatusLabel("Daten werden geladen");

            string tempSortName = table.AddTempNumeration();
            int rowCount = table.Rows.Count;
            table = table.GetSortedView($"[{identifier}] asc", orderType).ToTable();
            while (rowCount < table.Rows.Count)
            {
                table.Rows.RemoveAt(table.Rows.Count - 1);
            }
            table.AcceptChanges();

            SetStatusLabel("Daten werden überprüft");
            Dictionary<string, DataRowArray> dict = new Dictionary<string, DataRowArray>();
            //RowIdentifier, Values
            string oldIdenfifier = table.Rows[0][identifierIndex].ToString();
            int counter = 0;
            IEnumerable<PlusListboxItem> countColumns = additionalColumns.Where(item => item.State == PlusListboxItem.RowMergeState.Count);
            int countColumnsCount = countColumns.Count();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow oldRow = table.Rows[i];
                string newIdenfifier = oldRow[identifier].ToString();

                counter = newIdenfifier == oldIdenfifier ? counter + 1 : 1;

                if (dict.ContainsKey(newIdenfifier) && dict.TryGetValue(newIdenfifier, out DataRowArray dataRowArray))
                {
                    List<string> values = new List<string>();
                    foreach (PlusListboxItem additionalColumn in additionalColumns)
                    {
                        values.Add(oldRow[additionalColumn.ToString()].ToString());
                    }
                    dataRowArray.Add(values);
                }
                else
                {
                    if (countColumnsCount > 0)
                    {
                        foreach (PlusListboxItem item in countColumns)
                        {
                            oldRow[item.ToString()] = 1;
                        }
                    }
                    dict.Add(newIdenfifier, new DataRowArray(oldRow, new List<string>()));
                }


                if (counter > 1)
                {
                    table.Rows[i].Delete();
                }

                oldIdenfifier = newIdenfifier;
            }

            int newColumns = dict.Values.Max(dataRowArray => dataRowArray.Values.Count) - 1;

            SetStatusLabel("Neue Spalten werden hinzugefügt");
            for (int i = 1; i <= newColumns; i++)
            {
                foreach (PlusListboxItem additionalColumn in additionalColumns.Where(item => item.State == PlusListboxItem.RowMergeState.Nothing))
                {
                    table.TryAddColumn(additionalColumn.ToString() + i);
                }
            }

            int itemCount = dict.Values.Count;
            counter = 0;
            foreach (DataRowArray dataRowArray in dict.Values)
            {
                if (dataRowArray.Values.Count > 1)
                {
                    SetStatusLabel($"Daten werden geschrieben {counter}/{itemCount}");
                    for (int i = 1; i < dataRowArray.Values.Count; i++) //except first one
                    {
                        for (int y = 0; y < dataRowArray.Values[i].Count; y++)
                        {
                            if (additionalColumns[y].State == PlusListboxItem.RowMergeState.Sum)
                            {
                                dataRowArray.DataRow[additionalColumns[y].ToString()] = DataHelper.AddStringAsFloat(
                                    dataRowArray.DataRow[additionalColumns[y].ToString()].ToString(),
                                    dataRowArray.Values[i][y]);
                            }
                            else if (additionalColumns[y].State == PlusListboxItem.RowMergeState.Count)
                            {
                                if (int.TryParse(dataRowArray.DataRow[additionalColumns[y].ToString()].ToString(), out int result))
                                {
                                    dataRowArray.DataRow[additionalColumns[y].ToString()] = result + 1;
                                }
                            }
                            //additional Column
                            else
                            {
                                dataRowArray.DataRow[additionalColumns[y].ToString() + i] = dataRowArray.Values[i][y];
                            }
                        }
                    }
                }
                counter++;
            }
            return tempSortName;
        }
    }
}
