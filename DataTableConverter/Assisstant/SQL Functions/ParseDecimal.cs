using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "PARSEDECIMAL", Arguments = 1, FuncType = FunctionType.Scalar)]
    class ParseDecimal : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            decimal.TryParse(args[0].ToString(), out decimal result);
            return result;
        }
    }
}
