using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.Assisstant
{
    class StringComparer
    {
        [SuppressUnmanagedCodeSecurity]
        internal static class SafeNativeMethods
        {
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
            public static extern int StrCmpLogicalW(string psz1, string psz2);
        }

        public static int Compare(string a, string b)
        {
            return SafeNativeMethods.StrCmpLogicalW(a, b);
        }
    }

    class NaturalStringComparer : IComparer<string>
    {
        private readonly int mySortFlipper = 1;

        internal NaturalStringComparer()
        {

        }

        internal NaturalStringComparer(SortOrder sort)
        {
            mySortFlipper = sort == SortOrder.Ascending ? 1 : -1;
        }

        public int Compare(string x, string y)
        {
            return mySortFlipper * StringComparer.Compare(x ?? string.Empty, y ?? string.Empty);
        }
    }

    class CustomSort: IComparer<CustomSortItem>
    {
        private readonly bool Flag = true;
        internal CustomSort()
        {

        }

        internal CustomSort(bool flag)
        {
            Flag = flag;
        }

        public int Compare(CustomSortItem x, CustomSortItem y)
        {
            int result = 0;
            if (x.UpperHalf && y.UpperHalf || !x.UpperHalf && !y.UpperHalf)
            {
                result = x.Index.CompareTo(y.Index);
            }
            else
            {
                result = y.Index == x.Index && (Flag && y.UpperHalf || !Flag && x.UpperHalf) || y.Index > x.Index ? -1 : 1;
            }
            
            return result;
        }
    }

    class CustomSortItem
    {
        internal int Index;
        internal bool UpperHalf;

        public CustomSortItem(int index, bool flag)
        {
            Index = index;
            UpperHalf = flag;
        }
    }
}
