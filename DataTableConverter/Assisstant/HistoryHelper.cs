using DataTableConverter.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Assisstant
{
    class HistoryHelper
    {
        internal List<History> history;
        internal string OrderString { get; set; }
        private int historyPointer;

        internal HistoryHelper()
        {
            history = new List<History>();
            historyPointer = -1;
            OrderString = string.Empty;
        }

        internal void ResetHistory()
        {
            history.Clear();
            historyPointer = -1;
        }

        internal void AddHistory(History his, string order)
        {
            historyPointer++;
            his.Order = order;
            if (historyPointer >= history.Count)
            {
                history.Add(his);
            }
            else
            {
                history[historyPointer] = his;
            }

            AdjustHistory();
        }

        private void AdjustHistory()
        {
            for (int i = historyPointer + 1; i < history.Count;)
            {
                history.RemoveAt(i);
            }
        }

        internal DataTable GoBack(DataTable table, string orderBefore)
        {
            DataTable result = table;
            if (historyPointer >= 0)
            {
                //Umwandlung
                //es muss nur eine Version in der History gespeichert werden
                //bei "Rückgängig" formt man den aktuellen Wert um
                //d.h. wenn CellChange
                //Value = "meinText" in der History
                //tableCell = "meinText" und Value in der History = alter Wert von tableCell
                //NUR alte Werte in der History. Die neuen sind eh in der Tabelle und werden bei "rückgängig" richtig getauscht
                result = TakeOverHistory(table, orderBefore);
                historyPointer--;
            }
            return result;
        }
        internal DataTable Repeat(DataTable table, string orderBefore)
        {
            DataTable result = table;
            int maxIndex = history.Count - 1;
            if (historyPointer < maxIndex)
            {
                historyPointer++;
                result = TakeOverHistory(table, orderBefore);
            }
            return result;
        }

        private DataTable TakeOverHistory(DataTable table, string orderBefore, History His = null)
        {
            bool isIteration;
            History his;
            if (isIteration = His != null)
            {
                his = His;
            }
            else
            {
                his = history[historyPointer];
            }
            switch (his.State)
            {
                case State.CellValueChange:
                    string textOld = his.NewText;
                    his.NewText = table.Rows[his.RowIndex].ItemArray[his.ColumnIndex]?.ToString();
                    table.Rows[his.RowIndex].SetField(his.ColumnIndex, textOld);
                    break;

                case State.ValueChange:
                    List<CellMatrix> oldValues = his.Table;
                    foreach (CellMatrix matrix in oldValues.Where(cm => cm.bigChange == null))
                    {
                        string temp = matrix.Value;
                        matrix.Value = table.Rows[matrix.RowIndex].ItemArray[matrix.ColumnIndex]?.ToString();
                        table.Rows[matrix.RowIndex].SetField(matrix.ColumnIndex, temp);
                    }
                    oldValues = oldValues.Where(cm => cm.bigChange != null).ToList();
                    if(oldValues.Count > 0)
                    {
                        List<CellMatrix> rowDelete = oldValues.Where(matrix => matrix.bigChange.State == State.DeleteRow).OrderBy(cm => cm.bigChange.RowIndex).ToList();
                        List<CellMatrix> columnInsert = oldValues.Where(matrix => matrix.bigChange.State == State.InsertColumn).OrderByDescending(cm => cm.bigChange.ColumnIndex).ToList();
                        List<CellMatrix> columnDelete = oldValues.Where(matrix => matrix.bigChange.State == State.DeleteColumn).OrderBy(cm => cm.bigChange.ColumnIndex).ToList();
                        List<CellMatrix> rowInsert = oldValues.Where(matrix => matrix.bigChange.State == State.InsertRow).OrderByDescending(cm => cm.bigChange.RowIndex).ToList();

                        foreach (CellMatrix matrix in rowDelete.Concat(columnInsert).Concat(columnDelete).Concat(rowInsert))
                        {
                            table = TakeOverHistory(table, orderBefore, matrix.bigChange);
                        }
                    }
                    
                    break;

                case State.DeleteColumn:
                    his.State = State.InsertColumn;
                    DataTable newTable = new DataTable();
                    for (int i = 0; i <= table.Columns.Count; i++)
                    {
                        if (i == his.ColumnIndex)
                        {
                            newTable.Columns.Add(his.Column[0].ColumnName);
                        }
                        if (i < table.Columns.Count)
                        {
                            newTable.Columns.Add(table.Columns[i].ColumnName);
                        }
                    }

                    for (int y = 0; y < table.Rows.Count; y++)
                    {
                        object[] newArray = new object[table.Rows[y].ItemArray.Length + 1];
                        Array.Copy(table.Rows[y].ItemArray, 0, newArray, 0, his.ColumnIndex); //kein Index sondern length
                        newArray[his.ColumnIndex] = his.ColumnValues[0][y];
                        Array.Copy(table.Rows[y].ItemArray, his.ColumnIndex, newArray, his.ColumnIndex + 1, table.Rows[y].ItemArray.Length - his.ColumnIndex);
                        newTable.Rows.Add(newArray);
                    }
                    his.Column = null;
                    table = newTable;
                    break;

                case State.DeleteRow:
                    DataRow row = table.NewRow();
                    row.ItemArray = his.Row[0];
                    his.State = State.InsertRow;
                    table.Rows.InsertAt(row, his.RowIndex);
                    his.Row = null;
                    break;

                case State.HeaderChange:
                    string oldHeader = his.NewText;
                    his.NewText = table.Columns[his.ColumnIndex].ColumnName;
                    table.Columns[his.ColumnIndex].ColumnName = oldHeader;
                    break;

                case State.HeadersChange:
                    object[] oldHeaders = his.Row[0];
                    object[] newHeaders = DataHelper.HeadersOfDataTable(table);
                    for (int i = 0; i < oldHeaders.Length; i++)
                    {
                        int index;
                        if ((index = table.Columns.IndexOf(oldHeaders[i].ToString())) != -1)
                        {
                            table.Columns[index].ColumnName = table.Columns[index].ColumnName + "1";
                        }
                        table.Columns[i].ColumnName = oldHeaders[i].ToString();
                    }
                    his.Row[0] = newHeaders;
                    break;

                case State.InsertColumn:
                    DataColumn[] col = new DataColumn[1];

                    col[0] = table.Columns[his.ColumnIndex];
                    his.Column = col;
                    object[][] newValues = new object[1][];
                    newValues[0] = DataHelper.ColumnValues(table, his.ColumnIndex);
                    his.ColumnValues = newValues;
                    table.Columns.RemoveAt(his.ColumnIndex);
                    his.State = State.DeleteColumn;
                    break;

                case State.InsertRow:
                    object[][] newItem = new object[1][];
                    newItem[0] = table.Rows[his.RowIndex].ItemArray;
                    his.Row = newItem;
                    table.Rows.RemoveAt(his.RowIndex);
                    his.State = State.DeleteRow;
                    break;

                case State.AddColumnsAndRows:
                    int count = 0;

                    #region Delete Rows
                    object[][] newItems = new object[table.Rows.Count - his.RowIndex][];
                    for (int i = his.RowIndex; i < table.Rows.Count; i++)
                    {
                        newItems[count] = table.Rows[i].ItemArray;
                        count++;
                    }
                    his.Row = newItems;

                    for (int i = his.RowIndex; i < table.Rows.Count;)
                    {
                        table.Rows.RemoveAt(his.RowIndex);
                    }

                    #endregion

                    count = 0;
                    #region Delete Columns
                    DataColumn[] columns = null;
                    if (table.Columns.Count != his.ColumnIndex) //Nur ausführen, wenn Spalten hinzugekommen sind
                    {
                        columns = new DataColumn[table.Columns.Count - his.ColumnIndex];  //ab ColumnIndex ist neu

                        for (int i = his.ColumnIndex; i < table.Columns.Count; i++)
                        {

                            columns[count] = table.Columns[i];
                            //newValue[count] = getColumnValues(table, i);
                            count++;
                        }
                        //his.ColumnValues = newValue;

                        for (int i = his.ColumnIndex; i < table.Columns.Count;)
                        {
                            table.Columns.RemoveAt(i);
                        }
                    }
                    his.Column = columns;
                    #endregion
                    his.State = State.DeleteColumnsAndRows;
                    break;

                case State.DeleteColumnsAndRows:

                    for (int i = 0; his.Column != null && i < his.Column.Length; i++)
                    {
                        table.Columns.Add(his.Column[i].ColumnName);
                    }

                    his.Column = null;

                    foreach (object[] item in his.Row)
                    {
                        DataRow newRow = table.NewRow();
                        newRow.ItemArray = item;

                        table.Rows.Add(newRow);
                    }
                    his.Row = null;
                    his.State = State.AddColumnsAndRows;
                    break;
            }

            if (!isIteration)
            {
                OrderString = his.Order;
                his.Order = orderBefore;
            }
            return table;
        }
    }
}
