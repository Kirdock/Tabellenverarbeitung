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
            string replaceText = args[1].ToString();
            int start = int.Parse(args[2].ToString());
            int end = int.Parse(args[3].ToString());
            bool replace = bool.Parse(args[4].ToString());
            Func<string, int, int, string> substring = bool.Parse(args[5].ToString()) ? (Func<string, int, int, string>)SubstringReverse : Substring;
            if (!replace)
            {

                if (end == 0)
                {
                    value = start > value.Length ? string.Empty : substring(value, start - 1, 0);
                }
                else
                {
                    int length = (end - start);
                    value = start > value.Length ? string.Empty : length + start > value.Length ? substring(value, start - 1, 0) : substring(value, start - 1, length + 1);
                }
            }
            else
            {
                value = start > value.Length ? string.Empty : substring(value, 0, start - 1) + replaceText;

                if (end < value.Length && end != 0 && start <= value.Length)
                {
                    value += substring(value, end, 0);
                }
            }
            return value;
        }

        private string Substring(string value, int start, int end = 0)
        {
            return end == 0 ? value.Substring(start) : value.Substring(start, end);
        }

        private string SubstringReverse(string value, int start, int end = 0)
        {
            return end == 0 ? value.Substring(0, value.Length - start) : value.Substring(value.Length - end, end - start);
        }
    }
}
