using System.Data.SQLite;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "NO_CASE", FuncType = FunctionType.Collation)]
    class SQLiteNoCase : SQLiteFunction
    {
        public override int Compare(string a, string b)
        {
            return string.Compare(a, b, true);
        }
    }
}
