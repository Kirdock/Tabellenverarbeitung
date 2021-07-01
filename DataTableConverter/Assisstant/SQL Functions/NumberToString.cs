using System.Data.SQLite;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "TOSTRING", Arguments = 2, FuncType = FunctionType.Scalar)]
    class NumberToString : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            decimal.TryParse(args[0].ToString(), out decimal result);
            return result.ToString(args[1].ToString());
        }
    }
}
