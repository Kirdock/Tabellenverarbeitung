using System.Data.SQLite;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "CASESENSITIVE", FuncType = FunctionType.Collation)]
    class SQLiteSensitive : SQLiteFunction
    {
        public override int Compare(string a, string b)
        {
            return a.CompareTo(b);
        }
    }
}
