using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    class ProcAddTableColumns : WorkProc
    {
        public override void doWork(DataTable table, out string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure)
        {
            throw new NotImplementedException();
        }

        public override string[] GetHeaders()
        {
            throw new NotImplementedException();
        }

        public override void renameHeaders(string oldName, string newName)
        {
            throw new NotImplementedException();
        }

        internal static bool CheckFile(string filePath, ref string path)
        {
            path = Path.Combine(
                Path.GetDirectoryName(filePath),
                Path.GetFileNameWithoutExtension(filePath) + Properties.Settings.Default.PVMAddressText + Path.GetExtension(filePath));
            return File.Exists(path);
        }
    }
}
