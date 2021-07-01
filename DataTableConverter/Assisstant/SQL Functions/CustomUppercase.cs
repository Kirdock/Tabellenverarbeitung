using System.Data.SQLite;
using System.Linq;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "CUSTOMUPPERCASE", Arguments = 2, FuncType = FunctionType.Scalar)]
    class CustomUppercase : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            string value = args[0].ToString();
            int type = int.Parse(args[1].ToString());
            switch (type)
            {
                //Everything upper case letters
                case 0:
                    value = value.ToUpper();
                    break;

                //Everything lower case letters
                case 1:
                    value = value.ToLower();
                    break;

                //First letter upper case
                case 2:
                    value = value.First().ToString().ToUpper() + value.Substring(1).ToLower();
                    break;

                //First letters upper case
                default:
                    value = FirstLettersUpperCase(value);
                    break;
            }
            return value;
        }

        private string FirstLettersUpperCase(string value)
        {
            char[] array = value.ToCharArray();
            // Handle the first letter in the string.
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
                else
                {
                    array[i] = char.ToLower(array[i]);
                }
            }
            return new string(array);
        }
    }
}
