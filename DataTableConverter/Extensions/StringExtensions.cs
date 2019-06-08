using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Extensions
{
    internal static class StringExtensions
    {
        internal static string AppendFileName(this string path, string name)
        {
            return Path.GetFileNameWithoutExtension(path) + name;
        }
    }
}
