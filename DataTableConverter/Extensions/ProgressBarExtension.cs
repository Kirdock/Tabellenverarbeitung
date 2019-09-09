using DataTableConverter.Assisstant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.Extensions
{
    internal static class ProgressBarExtension
    {
        internal static void StartLoadingBar(this ProgressBar progressBar, int max)
        {
            try
            {
                progressBar.Invoke(new MethodInvoker(() =>
                {
                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.Value = 0;
                    progressBar.Maximum = max;
                }));
            }
            catch
            {
                
            }
        }

        internal static void UpdateLoadingBar(this ProgressBar progressBar)
        {
            try
            {
                progressBar.Invoke(new MethodInvoker(() =>
                {
                    progressBar.Value++;
                }));
            }
            catch
            {
                
            }
        }
    }
}
