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
        internal static void StartLoadingBar(this ProgressBar progressBar, int max, Form mainForm)
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
            catch (Exception ex)
            {
                ErrorHelper.LogMessage(ex, mainForm, false);
            }
        }

        internal static void UpdateLoadingBar(this ProgressBar progressBar, Form mainForm)
        {
            try
            {
                progressBar.Invoke(new MethodInvoker(() =>
                {
                    if (progressBar.Value < progressBar.Maximum)
                    {
                        progressBar.Value++;
                    }
                    else
                    {
                        progressBar.Value = progressBar.Maximum = 0;
                    }
                }));
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage(ex, mainForm, false);
            }
        }
    }
}
