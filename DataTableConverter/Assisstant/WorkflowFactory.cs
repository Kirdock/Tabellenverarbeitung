using DataTableConverter.Classes;
using DataTableConverter.Classes.WorkProcs;

namespace DataTableConverter.Assisstant
{
    class WorkflowFactory
    {
        internal static WorkProc CreateWorkProc(int type, int id, int ordinal, string name)
        {
            WorkProc newProc;
            switch (type)
            {
                //System-Proc
                case 1:
                    switch (id)
                    {
                        case 2:
                            newProc = new ProcMerge(ordinal, id, name);
                            break;

                        case 3:
                            newProc = new ProcOrder(ordinal, id, name);
                            break;

                        case 4:
                            newProc = new ProcUpLowCase(ordinal, id, name);
                            break;

                        case 5:
                            newProc = new ProcRound(ordinal, id, name);
                            break;

                        case 6:
                            newProc = new ProcPadding(ordinal, id, name);
                            break;

                        case 7:
                            newProc = new ProcNumber(ordinal, id, name);
                            break;

                        case 8:
                            newProc = new ProcSubstring(ordinal, id, name);
                            break;

                        case 9:
                            newProc = new ProcReplaceWhole(ordinal, id, name);
                            break;

                        case 10:
                            newProc = new ProcAddTableColumns(ordinal, id, name);
                            break;

                        case 11:
                            newProc = new ProcCompare(ordinal, id, name);
                            break;

                        case 12:
                            newProc = new ProcPVMExport(ordinal, id, name);
                            break;

                        case 13:
                            newProc = new ProcCount(ordinal, id, name);
                            break;

                        case 14:
                            newProc = new ProcSeparate(ordinal, id, name);
                            break;

                        case 15:
                            newProc = new ProcSearch(ordinal, id, name);
                            break;

                        case 16:
                            newProc = new ProcSplit(ordinal, id, name);
                            break;

                        case 17:
                            newProc = new ProcUser(ordinal, id, name)
                            {
                                IsSystem = true,
                                Procedure = new Proc()
                            };
                            break;

                        case 18:
                            newProc = new ProcMergeRows(ordinal, id, name);
                            break;

                        case 1:
                        default:
                            newProc = new ProcTrim(ordinal, id, name);
                            break;
                    }
                    break;

                //Duplicate
                case 2:
                    newProc = new ProcDuplicate(ordinal, id, name);
                    break;

                //User-Proc
                default:
                    newProc = new ProcUser(ordinal, id, name);
                    break;
            }
            return newProc;
        }
    }
}
