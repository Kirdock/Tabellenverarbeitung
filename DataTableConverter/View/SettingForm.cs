using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class SettingForm : Form
    {
        internal enum Tabs { Color, Shortcut, Table, Other, Help};

        internal SettingForm(Tabs tab = Tabs.Color)
        {
            InitializeComponent();
            LoadSettings();
            tabSettings.SelectedIndex = (int)tab;
            InitDataGridView();
        }

        private void InitDataGridView()
        {
            //column, text, empty, not empty
            dgvFormat.Rows.Add(new object[] { "Titel 1", string.Empty, true, "[Titel 2] [Titel 3]" });
            dgvFormat.Rows.Add(new object[] { "Titel 2", string.Empty, true, "[Titel 1] [Titel 3]" });
            dgvFormat.Rows.Add(new object[] { "Titel 3", string.Empty,true, "[Titel 1] [Titel 2]" });
            dgvFormat.Rows.Add(new object[] {});


            DgVSecondExample.Rows.Add(new object[] { "Straße"});
            DgVSecondExample.Rows.Add(new object[] { string.Empty, "_", false, string.Empty, false, "[Straße] [HNR] [Stock] [Tuer]" });
            DgVSecondExample.Rows.Add(new object[] { "HNR" });
            DgVSecondExample.Rows.Add(new object[] { string.Empty, "_/_", false, string.Empty, false, "[HNR] [Stock] [Tuer]" });
            DgVSecondExample.Rows.Add(new object[] { "Stock" });
            DgVSecondExample.Rows.Add(new object[] { string.Empty, "_/_", false, string.Empty, false, "[Stock] [Tuer]" });
            DgVSecondExample.Rows.Add(new object[] { "Tuer" });
            DgVSecondExample.Rows.Add(new object[] { });
        }

        private void LoadSettings()
        {
            cRequired.BackColor = Properties.Settings.Default.RequiredField;
            PSeparateColor.BackColor = Properties.Settings.Default.SeparateColor;
            cLocked.BackColor = Properties.Settings.Default.Locked;
            txtFailAddress.Text = Properties.Settings.Default.FailAddressText;
            txtRightAddress.Text = Properties.Settings.Default.RightAddressText;
            txtPVM.Text = Properties.Settings.Default.PVMAddressText;
            txtFailAddressValue.Text = Properties.Settings.Default.FailAddressValue;
            txtInvalidColumn.Text = Properties.Settings.Default.InvalidColumnName;
            cbSplitPVM.Checked = Properties.Settings.Default.SplitPVM;
            cbHeaderUpperCase.Checked = Properties.Settings.Default.ImportHeaderUpperCase;
            cbPVMSaveFormat.SelectedIndex = Properties.Settings.Default.PVMSaveFormat;
            txtOldAffix.Text = Properties.Settings.Default.OldAffix;
            cbAutoSavePVM.Checked = Properties.Settings.Default.AutoSavePVM;
            cbFullWidthImport.Checked = Properties.Settings.Default.FullWidthImport;
            NbFontSize.Value = Properties.Settings.Default.TableFontSize;
            NbRowHeight.Value = Properties.Settings.Default.RowHeight;
            TxtSettingPath.Text = ExportHelper.ProjectPath;
            NbFontSizeListBox.Value = Properties.Settings.Default.ListBoxFontSize;
            NbRowHeightListBox.Value = Properties.Settings.Default.ListBoxRowHeight;
            CBPvmSaveTwice.Checked = Properties.Settings.Default.PVMSaveTwice;
            TxTPVMIdentifier.Text = Properties.Settings.Default.PVMIdentifier;
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
                Properties.Settings.Default.SplitPVM = cbSplitPVM.Checked;
                Properties.Settings.Default.ImportHeaderUpperCase = cbHeaderUpperCase.Checked;
                Properties.Settings.Default.PVMSaveFormat = cbPVMSaveFormat.SelectedIndex;
                Properties.Settings.Default.OldAffix = txtOldAffix.Text;
                Properties.Settings.Default.AutoSavePVM = cbAutoSavePVM.Checked;
                Properties.Settings.Default.FullWidthImport = cbFullWidthImport.Checked;
                Properties.Settings.Default.TableFontSize = (int)NbFontSize.Value;
                Properties.Settings.Default.RowHeight = (int)NbRowHeight.Value;
                Properties.Settings.Default.ListBoxFontSize = (int)NbFontSizeListBox.Value;
                Properties.Settings.Default.ListBoxRowHeight = (int)NbRowHeightListBox.Value;
                Properties.Settings.Default.PVMSaveTwice = CBPvmSaveTwice.Checked;
                Properties.Settings.Default.PVMIdentifier = TxTPVMIdentifier.Text;
                Properties.Settings.Default.SeparateColor = PSeparateColor.BackColor;
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
                txtFailAddressValue,
                txtInvalidColumn,
                txtOldAffix
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

        private void llSourceCode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Kirdock/Tabellenverarbeitung");
        }

        private void BtnSearchFolder_Click(object sender, EventArgs e)
        {
            OpenFileDialog folderBrowser = new OpenFileDialog
            {
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Ordnerauswahl"
            };
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                string folderPath = Path.Combine(Path.GetDirectoryName(folderBrowser.FileName), ExportHelper.ProjectName);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                foreach (string file in Directory.GetFiles(ExportHelper.ProjectPath))
                {
                    FileInfo mFile = new FileInfo(file);
                    string path = Path.Combine(folderPath, mFile.Name);
                    // to remove name collisions
                    if (!File.Exists(path))
                    {
                        mFile.MoveTo(path);
                    }
                }
                foreach (string directory in Directory.GetDirectories(ExportHelper.ProjectPath))
                {
                    string path = Path.Combine(folderPath, Path.GetFileName(directory));
                    if (!Directory.Exists(path))
                    {

                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(directory, path);
                    }
                }

                Properties.Settings.Default.SettingPath = TxtSettingPath.Text = folderPath;
            }
            folderBrowser.Dispose();
        }

        private void DgVSecondExample_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
