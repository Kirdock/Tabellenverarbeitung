using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    [Serializable()]
    internal class MergeFormat
    {
        public enum MergeColumns:int { Column = 0, Text = 1, Empty = 2, NotEmpty = 3}
        public DataTable Table { get; set; }

        public MergeFormat()
        {
            Table = new DataTable()
            {
                TableName = "MergeFormat"
            };
            Table.Columns.Add("Spalte", typeof(string));
            Table.Columns.Add("Text", typeof(string));
            Table.Columns.Add("Wenn Spalten leer", typeof(string));
            Table.Columns.Add("Wenn Spalten nicht leer", typeof(string));
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach(DataRow row in Table.Rows)
            {
                builder.Append("([").Append(row[(int)MergeColumns.Column]?.ToString()).Append("]");
                builder.Append(row[(int)MergeColumns.Text]?.ToString());
                builder.Append(" WENN_NICHT_LEER[").Append(row[(int)MergeColumns.NotEmpty]?.ToString()).Append("]");
                builder.Append(" WENN_LEER[").Append(row[(int)MergeColumns.NotEmpty]?.ToString()).Append("]").Append(");");
            }

            return builder.ToString();
        }
    }
}
