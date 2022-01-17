using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "THOUSAND_SEPARATOR", Arguments = 1, FuncType = FunctionType.Scalar)]
    class ThousandSeparator : SQLiteFunction
    {

        public override object Invoke(object[] args)
        {
            decimal.TryParse(args[0].ToString(), out decimal result);
            int decimals = BitConverter.GetBytes(decimal.GetBits(result)[3])[2];
            return result.ToString($"N{decimals}");
        }
    }
}
