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
            //remove Null-Value and newLine
            foreach (DataRow row in table.AsEnumerable())
            {
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    if (row[i] == DBNull.Value)
                    {
                        row[i] = string.Empty;
                    }
                    row[i] = row[i].ToString().Replace("\n", string.Empty);
                }
            }
        }

        internal static List<string> HeadersToLower(this DataTable table)
        {
            return table.Columns.Cast<DataColumn>().Select(dt => dt.ColumnName.ToLower()).ToList();
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

        internal static void OverrideHeaders(this DataTable sourceTable, DataTable table)
        {
            OverrideHeaders(sourceTable, table.HeadersOfDataTableAsString());
        }

        internal static void OverrideHeaders(this DataTable table, string[] headers, bool isHistory = false)
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
            if(headers.Length > table.Columns.Count)
            {
                for(int i = table.Columns.Count; i < headers.Length; i++)
                {
                    table.TryAddColumn(headers[i]);
                }
            }
            else if (isHistory)
            {
                while(table.Columns.Count > headers.Length)
                {
                    table.Columns.RemoveAt(table.Columns.Count - 1);
                }
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

        internal static Dictionary<string, int> GroupCountOfColumn(this DataTable table, string column)
        {
            return table.GroupCountOfColumn(table.Columns.IndexOf(column));
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
                tempIndex = tableNew.Columns.Count - 1;
            }
            HashSet<int> newColumns = new HashSet<int>();
            //Get changed values. Don't use GetChanges(DataRowState.Modified) because then we don't know the original rowIndex (changes.RowCount != table.RowCount)
            for (int rowIndex = 0; rowIndex < tableNew.Rows.Count; rowIndex++)
            {
                if (tableNew.Rows[rowIndex].RowState == DataRowState.Modified)
                {
                    for (int colIndex = 0; colIndex < tableNew.Rows[rowIndex].ItemArray.Length; ++colIndex)
                    {
                        if (colIndex == tempIndex) continue;

                        object value = tableNew.Rows[rowIndex][colIndex, DataRowVersion.Original];
                        if (value != tableNew.Rows[rowIndex][colIndex, DataRowVersion.Current])
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
            }

            //deleted rows
            DataTable changes = tableNew.GetChanges(DataRowState.Deleted);
            if (changes != null && tempIndex != -1) //without tempSort I don't know where the DataRow was
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="path"></param>
        /// <param name="mainForm"></param>
        /// <param name="fileEncoding"></param>
        /// <param name="columnName"></param>
        /// <returns>Count of valid Rows</returns>
        internal static int SplitDataTable(this DataTable table, string path, Form1 mainForm, int fileEncoding, string columnName = null)
        {
            DataTable table1 = new DataTable() { TableName = path.AppendFileName(Properties.Settings.Default.FailAddressText) };
            string invalidColumnName = columnName ?? Properties.Settings.Default.InvalidColumnName;
            if (!table.Columns.Contains(invalidColumnName))
            {
                return 0;
            }
            DataTable table2;
            foreach (DataColumn column in table.Columns)
            {
                table1.Columns.Add(column.ColumnName, typeof(string));
            }
            table2 = table1.Copy();
            table2.TableName = path.AppendFileName(Properties.Settings.Default.RightAddressText);


            foreach (DataRow row in table.AsEnumerable().Where(row => row.RowState != DataRowState.Deleted))
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
            if (table1.Columns.Contains(TempSort))
            {
                table1.Columns.Remove(TempSort);
                table2.Columns.Remove(TempSort);
            }
            foreach (DataTable Table in new DataTable[] { table1, table2 })
            {
                string FileName = Table.TableName;
                switch (Properties.Settings.Default.PVMSaveFormat)
                {
                    //CSV
                    case 0:
                        {
                            ExportHelper.ExportCsv(Table, path, FileName, fileEncoding, mainForm);
                        }
                        break;

                    //Dbase
                    case 1:
                        {
                            ExportHelper.ExportDbase(FileName, Table, path, mainForm);
                        }
                        break;

                    //Excel
                    case 2:
                        {
                            ExportHelper.ExportExcel(Table, path, FileName, mainForm);
                        }
                        break;
                }
            }
            return table2.Rows.Count;
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

        internal static void AddColumnsOfDataTable(this DataTable sourceTable, DataTable importTable, string[] importColumns, int SourceMergeIndex, int ImportMergeIndex, out int[] newIndices, Form mainForm, ProgressBar pgbLoading = null)
        {
            sourceTable.Columns.Add(TempSort, typeof(string));
            int counter = 0;
            foreach (DataRow row in sourceTable.Rows)
            {
                row[TempSort] = counter++;
            }
            sourceTable.AcceptChanges();

            int oldCount = sourceTable.Columns.Count;
            int newColumnIndex = oldCount + importColumns.Length - 1; //-1: without identifier
            int[] importIndices = new int[sourceTable.Rows.Count];

            for (int i = 0; i < importColumns.Length; i++)
            {
                if (i != ImportMergeIndex)
                {
                    sourceTable.TryAddColumn(importColumns[i]);
                }
            }
            

            

            try
            {
                pgbLoading?.Invoke(new MethodInvoker(() => { pgbLoading.Value = 0; pgbLoading.Maximum = sourceTable.Rows.Count; }));
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage(ex, mainForm, false);
            }


            Dictionary<string, int> sourceValues = new Dictionary<string, int>();
            for (int i = 0; i < sourceTable.Rows.Count; i++)
            {
                string value = sourceTable.Rows[i][SourceMergeIndex].ToString();
                if (!sourceValues.ContainsKey(value))
                {
                    sourceValues.Add(value, i);
                }
            }


            for (int i = 0; i < importTable.Rows.Count; i++)
            {
                string key = importTable.Rows[i][ImportMergeIndex].ToString();
                if (sourceValues.TryGetValue(key, out int value))
                {
                    importIndices[value] = i;
                    int offset = 0;
                    for (int y = 0; y < importColumns.Length; y++)
                    {
                        if (y == ImportMergeIndex)
                        {
                            offset++;
                        }
                        else
                        {
                            sourceTable.Rows[value][oldCount + y - offset] = importTable.Rows[i][y];
                        }
                    }
                    sourceValues.Remove(key);
                }
                try
                {
                    pgbLoading?.BeginInvoke(new MethodInvoker(() => { pgbLoading.Value++; }));
                }
                catch (Exception ex)
                {
                    ErrorHelper.LogMessage(ex, mainForm, false);
                }
            }

            List<int> indices = importIndices.ToList();
            int markDelete = sourceTable.Rows.Count + 1;
            foreach (int value in sourceValues.Values.OrderByDescending(value => value))
            {
                importIndices[value] = markDelete;
            }
            
            //set order of imported table
            IEnumerable<DataRow> sortedTable = sourceTable.Copy().AsEnumerable().Select((row, i) => new { row, i }).OrderBy(r => importIndices[r.i]).Select(r => r.row);
            sourceTable.Rows.Clear();
            foreach (DataRow row in sortedTable)
            {
                sourceTable.ImportRow(row);
            }
            for(int i = importTable.Rows.Count; i < sourceTable.Rows.Count; i++)
            {
                sourceTable.Rows[i].Delete();
            }

            newIndices = importIndices.Where(value => value != markDelete).ToArray();
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
                        if (column.State == PlusListboxItem.RowMergeState.Anzahl)
                        {
                            firstRow.CountDict[column.Value]++;
                        }
                        else if(column.State == PlusListboxItem.RowMergeState.Summe)
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
                        if(column.State == PlusListboxItem.RowMergeState.Anzahl)
                        {
                            info.CountDict.Add(column.Value.ToString(), 1);
                        }
                        if (column.State == PlusListboxItem.RowMergeState.Summe)
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
