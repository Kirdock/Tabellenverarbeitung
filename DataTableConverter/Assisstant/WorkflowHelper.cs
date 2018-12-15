using DataTableConverter.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Assisstant
{
    class WorkflowHelper
    {

        internal static string getColumnsAsObjectArray(DataRow row, string[] columns, int[] subStringBegin, int[] subStringEnd, List<Tolerance> tolerances)
        {
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < columns.Length; i++)
            {
                #region Set Tolerances
                StringBuilder result = new StringBuilder(row[columns[i]].ToString().ToLower());
                foreach (Tolerance tol in tolerances)
                {
                    foreach (string t in tol.getColumnsAsArrayToLower())
                    {
                        result.Replace(t, tol.Name);
                    }
                }
                #endregion

                string resultString = result.ToString();

                #region Set Substring
                int begin = subStringBegin[i];
                int end = subStringEnd[i];
                if (begin != 0 && end != 0 && end >= begin)
                {
                    if (begin - 1 > resultString.Length)
                    {
                        resultString = string.Empty;
                    }
                    else
                    {
                        int count = end - begin + 1;
                        if (begin + count > resultString.Length)
                        {
                            count = resultString.Length - begin + 1;
                        }
                        resultString = resultString.Substring(begin - 1, count);
                    }
                }
                #endregion

                res.Append("|").Append(resultString);
            }
            return res.ToString();
        }

        internal static void checkHeaders(List<string> tableHeader, List<string> notFoundColumns, string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                string headerText = headers[i];
                if (!tableHeader.Contains(headerText.ToLower()))
                {
                    notFoundColumns.Add(headerText);
                }
            }
        }

        internal static string[] removeEmptyHeaders(string[] headers)
        {
            return headers.Where(header => !string.IsNullOrWhiteSpace(header)).ToArray();
        }
    }
}
