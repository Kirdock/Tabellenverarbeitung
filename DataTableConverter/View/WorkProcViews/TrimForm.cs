﻿using DataTableConverter.Classes.WorkProcs;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DataTableConverter.View.WorkProcViews
{
    public partial class TrimForm : Form
    {
        internal ProcTrim Proc;
        internal TrimForm(List<string> aliases)
        {
            InitializeComponent();
            CCBHeaders.Items.AddRange(aliases.ToArray());
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (TxtTrimText.Text == string.Empty)
            {
                this.MessagesOK(MessageBoxIcon.Warning, "Das Zeichen darf nicht leer sein");
            }
            else
            {
                ProcTrim.TrimType type = RbTrimStart.Checked ? ProcTrim.TrimType.Start : RbTrimEnd.Checked ? ProcTrim.TrimType.End : ProcTrim.TrimType.Both;
                string[] checkedHeaders = GetSelectedHeaders();
                Proc = new ProcTrim(TxtTrimText.Text, type, CbTrimDeleteDouble.Checked, checkedHeaders.Length == 0, checkedHeaders);
                DialogResult = DialogResult.OK;
            }
        }

        internal string[] GetSelectedHeaders()
        {
            return ViewHelper.GetSelectedHeaders(CCBHeaders);
        }

        private void BtnCheckAll_Click(object sender, EventArgs e)
        {
            ViewHelper.CheckAllItemsOfCheckedCombobox(CCBHeaders, true);
        }

        private void BtnUncheckAll_Click(object sender, EventArgs e)
        {
            ViewHelper.CheckAllItemsOfCheckedCombobox(CCBHeaders, false);
        }
    }
}
