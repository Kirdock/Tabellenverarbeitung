using System.IO;

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
