using System.Collections.Generic;
using System.Data;
using System.Linq;

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

    }

}
