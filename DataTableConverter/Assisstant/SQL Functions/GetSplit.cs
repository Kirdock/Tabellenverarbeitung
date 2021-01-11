using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "GETSPLIT", Arguments = 3, FuncType = FunctionType.Scalar)]
    class GetSplit: SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            string[] result = args[0].ToString().Split(new string[] { args[1].ToString() }, StringSplitOptions.RemoveEmptyEntries);
            int destinationIndex = (int)args[2];
            return destinationIndex >= result.Length ? string.Empty : result[destinationIndex];
        }
    }
}
