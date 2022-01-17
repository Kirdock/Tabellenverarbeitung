using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "DIVIDE", Arguments = 2, FuncType = FunctionType.Scalar)]
    class Divide : SQLiteFunction
    {

        public override object Invoke(object[] args)
        {
            decimal.TryParse(args[0].ToString(), out decimal dividend);
            decimal.TryParse(args[1].ToString(), out decimal divisor);
            return divisor != 0 ? (Math.Floor((dividend / divisor) * 100)/100).ToString("0.##") : "0";
        }
    }
}
