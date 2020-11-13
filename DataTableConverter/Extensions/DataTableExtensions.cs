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
