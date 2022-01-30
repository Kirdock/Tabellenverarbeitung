using System;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "REPLACE_LEADING_ZERO", Arguments = 2, FuncType = FunctionType.Scalar)]
    class ReplaceLeadingZero : SQLiteFunction
    {

        public override object Invoke(object[] args)
        {
            string value = args[0].ToString();
            return value.StartsWith("0") ? $"{args[1]}{value.Substring(1)}" : value;
        }
    }
}
