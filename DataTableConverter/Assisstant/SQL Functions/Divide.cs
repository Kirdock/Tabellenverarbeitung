using System;
using System.Data.SQLite;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "DIVIDE", Arguments = 3, FuncType = FunctionType.Scalar)]
    class Divide : SQLiteFunction
    {

        public override object Invoke(object[] args)
        {
            decimal.TryParse(args[0].ToString(), out decimal dividend);
            decimal.TryParse(args[1].ToString(), out decimal divisor);
            string numberFormat = (long)args[2] == 1 ? "F2" : "0.##"; // bool becomes 1 or 0 (int64)
            return (divisor != 0 ? (Math.Floor((dividend / divisor) * 100)/100) : 0).ToString(numberFormat);
        }
    }
}
