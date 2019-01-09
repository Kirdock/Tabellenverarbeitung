using DataTableConverter.Assisstant;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    internal class ProcUpLowCase : WorkProc
    {
        public static readonly string ClassName = "Groß-/Kleinschreibung";
        public int Option; //0: UpperCase; 1: LowerCase, 2: first letter UpperCase, 3: first letters Uppercase
        public bool AllColumns { get; set; }
        public override string[] GetHeaders()
        {
            return AllColumns ? new string[0] : WorkflowHelper.RemoveEmptyHeaders(Columns.Rows.Cast<DataRow>().Select(dr => dr.ItemArray.Length > 0 ? dr.ItemArray[0].ToString() : null).ToArray());
        }

        public ProcUpLowCase(int ordinal, int id,string name) : base(ordinal, id, name) {
            Option = 0;
        }

        public ProcUpLowCase(string[] columns, bool allColumns, int option)
        {
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            foreach(string col in columns)
            {
                Columns.Rows.Add(col);
            }
            AllColumns = allColumns;
            Option = option;
        }

        public override void renameHeaders(string oldName, string newName)
        {
            foreach (DataRow row in Columns.Rows)
            {
                if (row.ItemArray[0].ToString() == oldName)
                {
                    row.SetField(0, newName);
                }
            }
        }

        public override void doWork(DataTable table, out string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure)
        {

            string[] columns = GetHeaders();
            sortingOrder = string.Empty;
            
            
            
            List<int> headerIndices = DataHelper.getHeaderIndices(table, columns);
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < row.ItemArray.Length; i++)
                {

                    if ((columns == null || columns.Length <1 || headerIndices.Contains(i)))
                    {
                        string value = row.ItemArray[i].ToString();
                        switch (Option)
                        {
                            //Alles Großbuchstaben
                            case 0:
                                value = value.ToUpper();
                                break;

                            //Alles Kleinbuchstaben
                            case 1:
                                value = value.ToLower();
                                break;

                            //Erster Buchstabe groß
                            case 2:
                                value = value.First().ToString().ToUpper() + value.Substring(1).ToLower();
                                break;

                            //Erste Buchstaben groß
                            default:
                                value = firstLettersUpperCase(value);
                                break;
                        }
                        row.SetField(i, value);
                    }
                }
            }
        }

        private string firstLettersUpperCase(string value)
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
