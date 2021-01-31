using System.Data.SQLite;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "PADDING", Arguments = 4, FuncType = FunctionType.Scalar)]
    class Padding : SQLiteFunction
    {
        //value, type, counter, character
        public override object Invoke(object[] args)
        {
            string result = args[0].ToString();
            int counter = int.Parse(args[2].ToString());
            char character = char.Parse(args[3].ToString());
            switch (int.Parse(args[1].ToString()))
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
