using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    [SQLiteFunction(Name = "NATURALSORT", FuncType = FunctionType.Collation)]
    class SQLiteComparator :SQLiteFunction
    {
        [SuppressUnmanagedCodeSecurity]
        internal static class SafeNativeMethods
        {
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
            public static extern int StrCmpLogicalW(string psz1, string psz2);
        }

        public override int Compare(string a, string b)
        {
            return SafeNativeMethods.StrCmpLogicalW(a, b);
        }
    }
}
