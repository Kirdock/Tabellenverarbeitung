using System;
using System.Data.SQLite;

namespace DataTableConverter.Assisstant.SQL_Functions
{
    [SQLiteFunction(Name = "ROUND2", Arguments = 3, FuncType = FunctionType.Scalar)]
    class Round : SQLiteFunction
    {

        public override object Invoke(object[] args)
        {
            decimal.TryParse(args[0].ToString(), out decimal result);
            return RoundNumber(result, int.Parse(args[1].ToString()), int.Parse(args[2].ToString()));
        }

        private string RoundNumber(decimal number, int type, int decimals)
        {
            string result;
            switch (type)
            {
                //normal round
                case 0:
                    {
                        result = Math.Round(number, decimals, MidpointRounding.AwayFromZero).ToString();
                    }
                    break;

                //ceiling
                case 1:
                    {
                        result = RoundUp(number, decimals).ToString();
                    }
                    break;
                //floor
                default:
                    {
                        result = RoundDown(number, decimals).ToString();
                    }
                    break;
            }
            return result;
        }

        private double RoundUp(decimal input, int decimals)
        {
            double multiplier = Math.Pow(10, Convert.ToDouble(decimals));
            return Math.Ceiling((double)input * multiplier) / multiplier;
        }

        private double RoundDown(decimal input, int decimals)
        {
            double multiplier = Math.Pow(10, Convert.ToDouble(decimals));
            return Math.Floor((double)input * multiplier) / multiplier;
        }
    }
}
