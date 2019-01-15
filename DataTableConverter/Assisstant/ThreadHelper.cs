using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataTableConverter.Assisstant
{
    class ThreadHelper
    {
        private List<Thread> Threads = new List<Thread>();
        private readonly Action Start, End;
        internal bool Abort;
        internal static ThreadAction Threadaction;

        internal ThreadHelper(Action start, Action end)
        {
            Start = start;
            End = end;
        }

        internal void StartThread(ThreadAction action)
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    Start();
                    Thread.CurrentThread.IsBackground = true;

                    action(ref Abort);
                }
                catch (Exception ex)
                {
                    ErrorHelper.LogMessage(ex);
                }
                finally
                {
                    End();
                    Threads.Remove(Thread.CurrentThread);
                }
            });
            Threads.Add(thread);
            thread.Start();
        }

        internal delegate void ThreadAction(ref bool abort);
    }
}
