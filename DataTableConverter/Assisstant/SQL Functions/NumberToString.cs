using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "TOSTRING", Arguments = 2, FuncType = FunctionType.Scalar)]
    class NumberToString : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return ((decimal)args[0]).ToString(args[1].ToString());
        }
    }
}
