using System;
using System.Data.SQLite;
using System.Text;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "CUSTOMSUBSTRING", Arguments = 6, FuncType = FunctionType.Scalar)]
    class CustomSubstring : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            string value = args[0].ToString();
            string replaceText = args[1].ToString();
            int startIndex = int.Parse(args[2].ToString()) -1;
            int endIndex = int.Parse(args[3].ToString()) - 1;
            bool replace = int.Parse(args[4].ToString()) == 1;
            bool reverse = int.Parse(args[5].ToString()) == 1;
            if(reverse)
            {
                int startBefore = startIndex;
                startIndex = endIndex == -1 ? 0 : value.Length - endIndex - 1;
                endIndex = value.Length - startBefore - 1;
            }
            if(endIndex >= value.Length)
            {
                endIndex = -1;
            }
            if(startIndex >= value.Length)
            {
                return string.Empty;
            }
            if (!replace)
            {
                if (endIndex == -1)
                {
                    return value.Substring(startIndex);
                }
                else
                {
                    return value.Substring(startIndex, endIndex - startIndex + 1);
                }
            }
            else
            {
                StringBuilder result = new StringBuilder(value.Substring(0, startIndex)).Append(replaceText);

                if (endIndex != -1)
                {
                    result.Append(value.Substring(endIndex + 1));
                }
                return result.ToString();
            }
        }
    }
}
