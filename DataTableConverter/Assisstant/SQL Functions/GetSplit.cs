using System;
using System.Data.SQLite;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "GETSPLIT", Arguments = 3, FuncType = FunctionType.Scalar)]
    class GetSplit : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            string[] result = args[0].ToString().Split(new string[] { args[1].ToString() }, StringSplitOptions.RemoveEmptyEntries);
            int destinationIndex = int.Parse(args[2].ToString());
            return destinationIndex >= result.Length ? string.Empty : result[destinationIndex];
        }
    }
}
