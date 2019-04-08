using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class SettingForm : Form
    {
        public SettingForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            cRequired.BackColor = Properties.Settings.Default.RequiredField;
            cLocked.BackColor = Properties.Settings.Default.Locked;
            txtFailAddress.Text = Properties.Settings.Default.FailAddressText;
            txtRightAddress.Text = Properties.Settings.Default.RightAddressText;
            txtPVM.Text = Properties.Settings.Default.PVMAddressText;
            txtFailAddressValue.Text = Properties.Settings.Default.FailAddressValue;
            txtInvalidColumn.Text = Properties.Settings.Default.InvalidColumnName;
        }

        private void SettingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsValid())
            {
                Properties.Settings.Default.RequiredField = cRequired.BackColor;
                Properties.Settings.Default.Locked = cLocked.BackColor;
                Properties.Settings.Default.FailAddressText = txtFailAddress.Text;
                Properties.Settings.Default.RightAddressText = txtRightAddress.Text;
                Properties.Settings.Default.PVMAddressText = txtPVM.Text;
                Properties.Settings.Default.FailAddressValue = txtFailAddressValue.Text;
                Properties.Settings.Default.InvalidColumnName = txtInvalidColumn.Text;
                Properties.Settings.Default.Save();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private bool IsValid()
        {
            bool valid = true;
            TextBox[] textBoxes = new TextBox[]
            {
                txtFailAddress,
                txtRightAddress,
                txtPVM,
                txtFailAddressValue,
                txtInvalidColumn
            };
            foreach(TextBox textBox in textBoxes)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    MessageHandler.MessagesOK(MessageBoxIcon.Warning, "Bitte geben Sie eine Bezeichnung ein");
                    valid = false;
                    tabSettings.SelectedIndex = 1;
                    textBox.Focus();
                    break;

                }
            }
            return valid;
        }

        private void cRequired_Click(object s, EventArgs e)
        {
            Panel sender = ((Panel)s);
            colorDialog1.Color = sender.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                sender.BackColor = colorDialog1.Color;
            }
        }
    }
}
