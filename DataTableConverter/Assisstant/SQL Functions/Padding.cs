using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "PADDING", Arguments = 4, FuncType = FunctionType.Scalar)]
    class Padding : SQLiteFunction
    {
        //value, type, counter, character
        public override object Invoke(object[] args)
        {
            string result = args[0].ToString();
            int counter = (int)args[2];
            char character = (char)args[3];
            switch ((int)args[1])
            {
                case 0:
                    result = result.PadLeft(counter, character);
                    break;

                case 1:
                default:
                    result = result.PadRight(counter, character);
                    break;
            }
            return result;
        }
    }
}
