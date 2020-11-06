using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using DataTableConverter.Classes.WorkProcs;
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
        internal static readonly string TempSort = "[TEMP_SORT]";
        private static readonly string fileName = "Dateiname";
        internal static string FileName
        {
            get
            {
                return Properties.Settings.Default.ImportHeaderUpperCase ? fileName.ToUpper() : fileName;
            }
        }

        internal static void RemoveEmptyRows(this DataTable table)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (table.Rows[i].ItemArray.All(item => string.IsNullOrWhiteSpace(item?.ToString())))
                {
                    table.Rows.RemoveAt(i--);
                }
            }
        }

        internal static void AdjustDBASEImport(this DataTable table)
        {
            //remove newLine
            foreach (DataRow row in table.AsEnumerable())
            {
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    row[i] = row[i].ToString().Replace("\n", string.Empty);
                }
            }
        }

        internal static void Trim(this DataTable table)
        {
            ProcTrim proc = new ProcTrim();
            string order = string.Empty;
            proc.DoWork(table, ref order, null, null, null, null, null, OrderType.Windows, null, out int[] _);
        }

        internal static string[] HeadersOfDataTableAsString(this DataTable table)
        {
            return table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
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

        internal static Dictionary<string, int> GroupCountOfColumn(this DataTable table, string column)
        {
            return table.GroupCountOfColumn(table.Columns.IndexOf(column));
        }

        internal static List<int> HeaderIndices(this DataTable table, string[] columns)
        {
            return columns.Select(column => table.Columns.IndexOf(column)).Where(index => index != -1).ToList();
        }

        internal static object[] HeadersOfDataTable(this DataTable table)
        {
            return table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray() ?? new object[0];
        }

        internal static object[] ColumnValues(this DataTable table, int column)
        {
            return table.AsEnumerable().Select(row => row[column]).ToArray();
        }

        internal static string[] ColumnValuesAsString(this DataTable table, int column)
        {
            return table.AsEnumerable().Select(row => row[column]?.ToString()).ToArray();
        }

        internal static IEnumerable<int> ColumnValuesAsInt(this DataTable table, int column)
        {
            return table.AsEnumerable().Select(row => (int)row[column]);
        }

        internal static DataTable SetColumnsTypeStringWithContainingData(this DataTable table)
        {
            DataTable dtCloned = table.Clone();
            foreach (DataColumn col in dtCloned.Columns)
            {
                col.DataType = typeof(string);
            }

            foreach (DataRow row in table.Rows)
            {
                dtCloned.ImportRow(row);
            }
            return dtCloned;
        }

        internal static void ConcatTable(this DataTable originalTable, DataTable table, string originalFilename, string secondFilename)
        {
            List<int> ColumnIndizes = new List<int>();
            table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList().ForEach(x =>
            {
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

        internal static EnumerableRowCollection<DataRow> SkipTakeRows(this DataTable table, decimal range, int page, out string tempColumn, OrderedEnumerableRowCollection<DataRow> enumerable = null)
        {
            string temp = tempColumn = table.TryAddColumn(TempSort);
            EnumerableRowCollection<DataRow> result;
            if (page == -1)
            {
                result = enumerable ?? table.AsEnumerable();
            }
            else
            {
                int count = 0;
                
                foreach (DataRow row in (enumerable ?? table.AsEnumerable()).ToList())//lazy loading. this does not always run before the next (enumerable.OrderBy...) statement
                {
                    row[temp] = count++;
                }
                result = (enumerable ?? table.AsEnumerable()).Where(row => {
                    bool status = table.Columns.Contains(temp);
                    if (status)
                    {
                        int.TryParse(row[temp]?.ToString(), out int res);
                        status = res >= (page - 1) * range && res < page * range;
                    }
                    return status;
                });
            }
            return result;
        }

        internal static DataView GetSortedView(this DataTable table, string order, OrderType orderType, int page, Action addHistory = null)
        {
            //Select ROW_NUMBER() over(order by name) as rnumber, name, hint from main ORDER by
                //case
                //	when rnumber > half  then(rnumber - (half-0.5))
                //    when rnumber <= half then rnumber
                //end asc


            DataView view;
            table.Dispose(); //in hope to remove all remaining lazy loading
            table.AcceptChanges();
            table.BeginLoadData();
            Dictionary<string, SortOrder> dict = ViewHelper.GenerateSortingList(order);
            if (dict.Count == 0)
            {
                view = table.SkipTakeRows(Properties.Settings.Default.MaxRows, page, out string tempColumn).AsDataView();
                table.Columns.Remove(tempColumn);
                //view = table.AsEnumerable().Where(row => int.TryParse(row[temp]?.ToString(), out int res) && res > (page-1)*range && res < (page-1)*(range+1) ).AsDataView(); //create new view. Default view throws exception "is not opened"
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

                OrderedEnumerableRowCollection<DataRow> enumerable = table.AsEnumerable().OrderBy(field => field?.Field<string>(firstElement.Key), new NaturalStringComparer(firstElement.Value));

                foreach (var column in dict.Skip(1))
                {
                    enumerable = enumerable.ThenBy(field => field.Field<string>(column.Key), new NaturalStringComparer(column.Value));
                }

                if (orderType != OrderType.Windows)
                {
                    int half = enumerable.Count() / 2;

                    int count = 0;
                    foreach (DataRow row in enumerable.ToList())//lazy loading. this does not always run before the next (enumerable.OrderBy...) statement
                    {
                        row[tempSortName] = count++;
                    }
                    enumerable = enumerable.OrderBy(row =>
                    {
                        if (row != null && table.Columns.IndexOf(tempSortName) != -1 && int.TryParse(row[tempSortName]?.ToString(), out int res)) //everything here is triggered if row[tempSortName] is set... or before it is set... idk... wtf. so there is a new column but there are not any indices written (or just one)
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
                view = table.SkipTakeRows(Properties.Settings.Default.MaxRows, page, out string tempColumn, enumerable).AsDataView();
                table.Columns.Remove(tempColumn);
                //view = enumerable.AsDataView();

                if (tempSortName != string.Empty)
                {
                    table.Columns.Remove(tempSortName);
                }

            }
            
            table.EndLoadData();
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

        /// <summary>
        /// Rows with the same value in the given column "identifier" are merged.
        /// <para></para>Columns are either summed, counted or a new one is created
        /// </summary>
        /// <param name="table"></param>
        /// <param name="identifier"></param>
        /// <param name="additionalColumns"></param>
        /// <param name="progressBar"></param>
        /// <returns>The temporary sort-column that is created for the history</returns>
        internal static string MergeRows(this DataTable table, string identifier, List<PlusListboxItem> additionalColumns, bool separator, ProgressBar progressBar, Form mainForm)
        {
            progressBar?.StartLoadingBar(table.Rows.Count, mainForm);
            string separatorText = separator ? "#,0.###" : string.Empty;
            int lastIndex = table.Columns.Count;
            //we have to add tempSort column in order to manage row deletion/insertion (history)
            string name = table.TryAddColumn(TempSort);

            for (int i = 0; i < table.Rows.Count; i++)
            {
                table.Rows[i][lastIndex] = i;
            }
            table.AcceptChanges();

            Dictionary<string, MergeRowsInfo> dict = new Dictionary<string, MergeRowsInfo>();
            for(int i = 0; i < table.Rows.Count; i++)
            {
                string key = table.Rows[i][identifier].ToString();
                if (dict.TryGetValue(key,out MergeRowsInfo firstRow))
                {
                    firstRow.RowsMerged++;
                    foreach (PlusListboxItem column in additionalColumns)
                    {
                        if (column.State == PlusListboxItem.RowMergeState.Count)
                        {
                            firstRow.CountDict[column.Value]++;
                        }
                        else if(column.State == PlusListboxItem.RowMergeState.Sum)
                        {
                            if (decimal.TryParse(table.Rows[i][column.Value].ToString(), out decimal result))
                            {
                                firstRow.CountDict[column.Value] += result;
                            }
                        }
                        else
                        {
                            string value = table.Rows[i][column.Value].ToString();
                            if (value != string.Empty)
                            {
                                string header = column.Value + firstRow.RowsMerged;
                                if (!table.Columns.Contains(header))
                                {
                                    header = table.TryAddColumn(column.Value, firstRow.RowsMerged);
                                }
                                firstRow.Row[header] = value;
                            }
                            //table.Columns[header].SetOrdinal(table.Columns.IndexOf(column.Value) + firstRow.RowsMerged);
                            //ordinal destroys my history atm
                        }
                    }
                    
                    table.Rows[i].Delete();
                }
                else
                {
                    MergeRowsInfo info = new MergeRowsInfo(table.Rows[i]);
                    foreach(PlusListboxItem column in additionalColumns)
                    {
                        if(column.State == PlusListboxItem.RowMergeState.Count)
                        {
                            info.CountDict.Add(column.Value.ToString(), 1);
                        }
                        if (column.State == PlusListboxItem.RowMergeState.Sum)
                        {
                            decimal.TryParse(info.Row[column.Value].ToString(), out decimal result);
                            info.CountDict.Add(column.Value.ToString(), result);
                        }
                    }
                    dict.Add(key, info);
                }
                progressBar?.UpdateLoadingBar(mainForm);
            }

            foreach(MergeRowsInfo info in dict.Values)
            {
                foreach(string column in info.CountDict.Keys)
                {
                    info.Row[column] = info.CountDict[column].ToString(separatorText);
                }
            }
            return name;
        }

        internal static bool AddColumnWithDialog(this DataTable table, string column, Form mainForm)
        {
            bool inserted = true;
            if (table.Columns.Contains(column))
            {
                inserted = mainForm.MessagesYesNo(MessageBoxIcon.Warning, $"Es gibt bereits eine Spalte mit der Bezeichnung \"{column}\".\nSpalte überschreiben?") == DialogResult.Yes;
                if (inserted)
                {
                    foreach(DataRow row in table.Rows)
                    {
                        row[column] = string.Empty;
                    }
                }
            }
            else
            {
                table.Columns.Add(column);
            }
            return inserted;
        }
    }
        
}
