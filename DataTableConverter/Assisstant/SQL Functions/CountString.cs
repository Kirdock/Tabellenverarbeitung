using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "COUNTSTRING", Arguments = 2, FuncType = FunctionType.Scalar)]
    class CountString : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Regex.Matches(args[0].ToString(), args[1].ToString()).Count;
        }
    }
}
