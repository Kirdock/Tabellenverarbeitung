using System;
using System.Data.SQLite;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "CUSTOMSUBSTRING", Arguments = 6, FuncType = FunctionType.Scalar)]
    class CustomSubstring : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            string value = args[0].ToString();
            string result;
            string replaceText = args[1].ToString();
            int start = int.Parse(args[2].ToString());
            int end = int.Parse(args[3].ToString());
            bool replace = int.Parse(args[4].ToString()) == 1;
            Func<string, int, int, string> substring = int.Parse(args[5].ToString()) == 1 ? (Func<string, int, int, string>)SubstringReverse : Substring;
            if (!replace)
            {
                if (end == 0)
                {
                    result = start > value.Length ? string.Empty : substring(value, start - 1, -1);
                }
                else
                {
                    int length = (end - start);
                    result = start > value.Length ? string.Empty : length + start > value.Length ? substring(value, start - 1, -1) : substring(value, start - 1, length + 1);
                }
            }
            else
            {
                result = start > value.Length ? string.Empty : (substring(value, 0, start-1) + replaceText);

                if (end < value.Length && end != 0 && start <= value.Length)
                {
                    result += substring(value, end, -1);
                }
            }
            return result;
        }

        private string Substring(string value, int start, int end)
        {
            return end == -1 ? value.Substring(start) : value.Substring(start, end);
        }

        private string SubstringReverse(string value, int start, int end)
        {
            return end == -1 ? value.Substring(0, value.Length - start) : value.Substring(value.Length - end, end - start);
        }
    }
}
