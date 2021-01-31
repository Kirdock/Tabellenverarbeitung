using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "CUSTOMTRIM", Arguments = 4, FuncType = FunctionType.Scalar)]
    class CustomTrim : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            string value = args[0].ToString();
            char[] charArray = args[1].ToString().ToArray();
            bool deleteDouble = bool.Parse(args[2].ToString());
            int type = int.Parse(args[3].ToString());

            if (deleteDouble)
            {
                foreach (char c in charArray)
                {
                    Regex regex = new Regex("[" + c + "]{2,}", RegexOptions.None);
                    value = regex.Replace(GetTrimmed(value, charArray, type), c.ToString());
                }
            }
            else
            {
                value = GetTrimmed(value, charArray, type);
            }
            return value;
        }

        private string GetTrimmed(string text, char[] charArray, int type)
        {
            string result;
            switch (type)
            {
                case 0:
                    result = text.TrimStart(charArray);
                    break;

                case 1:
                    result = text.TrimEnd(charArray);
                    break;

                case 2:
                default:
                    result = text.Trim(charArray);
                    break;
            }
            return result;
        }
    }
}
