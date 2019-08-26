using DataTableConverter.Classes.WorkProcs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    [Serializable()]
    public class MergeFormat
    {
        public enum MergeColumns:int { Column = 0, Text = 1, Empty = 3, NotEmpty = 5, EmptyAll = 2, NotEmptyAll = 4}
        public DataTable Table { get; set; }
        public string Formula;

        public MergeFormat()
        {
            Table = new DataTable()
            {
                TableName = "MergeFormat"
            };
            Table.Columns.Add("Spalte", typeof(string));
            Table.Columns.Add("Text", typeof(string));
            Table.Columns.Add("Alle Spalten überprüfen", typeof(bool));
            Table.Columns.Add("Wenn Spalten leer", typeof(string));
            Table.Columns.Add("Alle Spalten überprüfen ", typeof(bool));
            Table.Columns.Add("Wenn Spalten nicht leer", typeof(string));
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (IsStringFormat())
            {
                builder.Append(Formula);
            }
            else if (Table.Columns.Count > 4) //== new format
            {
                foreach (DataRow row in Table.Rows)
                {
                    builder.Append("([").Append(row[(int)MergeColumns.Column]?.ToString()).Append("]");
                    builder.Append(row[(int)MergeColumns.Text]?.ToString());
                    builder.Append(" WENN_NICHT_LEER[").Append(row[(int)MergeColumns.NotEmpty]?.ToString()).Append("]");
                    builder.Append(" WENN_LEER[").Append(row[(int)MergeColumns.NotEmpty]?.ToString()).Append("]").Append(");");
                }
            }

            return builder.ToString();
        }

        internal bool IsStringFormat()
        {
            return !string.IsNullOrWhiteSpace(Formula);
        }

        internal IEnumerable<string> GetHeaders()
        {
            return GetColumn().Concat(GetHeadersEmpty()).Concat(GetHeadersNotEmpty()).Concat(GetHeadersOfFormula()).Distinct();
        }

        internal IEnumerable<string> GetColumn()
        {
            return Table.AsEnumerable().Select(row => row[(int)MergeColumns.Column]?.ToString()).Distinct().Where(header => !string.IsNullOrWhiteSpace(header));
        }

        internal IEnumerable<string> GetHeadersEmpty()
        {
            return Table.AsEnumerable().SelectMany(row => ProcMerge.GetHeaderOfFormula(row[(int)MergeColumns.Empty]?.ToString())).Distinct().Where(header => !string.IsNullOrWhiteSpace(header));
        }

        internal IEnumerable<string> GetHeadersNotEmpty()
        {
            return Table.AsEnumerable().SelectMany(row => ProcMerge.GetHeaderOfFormula(row[(int)MergeColumns.NotEmpty]?.ToString())).Distinct().Where(header => !string.IsNullOrWhiteSpace(header));
        }

        internal IEnumerable<string> GetHeadersOfFormula()
        {
            return ProcMerge.GetHeaderOfFormula(Formula);
        }

        internal void RenameHeaders(string oldName, string newName)
        {
            foreach(DataRow row in Table.Rows)
            {
                ProcMerge.RenameHeader(row, (int)MergeColumns.Column, oldName, newName);
                ProcMerge.RenameFormatHeader(row, (int)MergeColumns.Empty, oldName, newName);
                ProcMerge.RenameFormatHeader(row, (int)MergeColumns.NotEmpty, oldName, newName);
            }

            Formula = ProcMerge.RenameFormatHeader(Formula, oldName, newName);
        }
    }
}
