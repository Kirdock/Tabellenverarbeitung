using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Exceptions
{
    class TableNotFoundException : Exception
    {
        public TableNotFoundException(string tableName) : base(FormatMessage(tableName)){}

        public TableNotFoundException(params string[] tables) : base(FormatMessage(tables)) { }

        private static string FormatMessage(string tableName)
        {
            return $"Die Tabelle \"{tableName}\" konnte nicht gefunden werden";
        }

        private static string FormatMessage(string[] tables)
        {
            return $"Die Tabellen \"{string.Join("\"",tables)}\" konnten nicht gefunden werden";
        }
    }
}
