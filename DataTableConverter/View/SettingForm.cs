﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class SettingForm : Form
    {
        internal enum Tabs { Duplciate, Shortcut, Color, Table, Other, Help };

        internal SettingForm(Tabs tab = Tabs.Duplciate)
        {
            InitializeComponent();
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            LblVersion.Text = fvi.FileVersion;
            LoadSettings();
            tabSettings.SelectedIndex = (int)tab;
            InitDataGridView();
        }

        private void InitDataGridView()
        {
            //column, text, empty, not empty
            dgvFormat.Rows.Add(new object[] { "Titel 1", string.Empty, true, "[Titel 2] [Titel 3]" });
            dgvFormat.Rows.Add(new object[] { "Titel 2", string.Empty, true, "[Titel 1] [Titel 3]" });
            dgvFormat.Rows.Add(new object[] { "Titel 3", string.Empty, true, "[Titel 1] [Titel 2]" });
            dgvFormat.Rows.Add(new object[] { });


            DgVSecondExample.Rows.Add(new object[] { "Straße" });
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
            CbImportWorkflowAuto.Checked = Properties.Settings.Default.ImportWorkflowAuto;
            CbSeparateSelectable.Checked = Properties.Settings.Default.SeparateSelectable;
            NumMaxRows.Value = Properties.Settings.Default.MaxRows;
            CBTrimImport.Checked = Properties.Settings.Default.TrimImport;
            CBLoadHiddenRows.Checked = Properties.Settings.Default.UnhideRows;
            CBLoadHiddenColumns.Checked = Properties.Settings.Default.UnhideColumns;
            CBDuplicateCaseSensitive.Checked = Properties.Settings.Default.DuplicateCaseSensitive;
            TxTShortcut.Text = new KeysConverter().ConvertToString(Properties.Settings.Default.EditShortcut);
            checkBoxLeadingZero.Checked = Properties.Settings.Default.PVMLeadingZero;
            textBoxLeadingZeroText.Text = Properties.Settings.Default.PVMLeadingZeroText;
            textBoxLeadingZeroAlias.Text = Properties.Settings.Default.PVMLeadingZeroAlias;
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
                Properties.Settings.Default.ImportWorkflowAuto = CbImportWorkflowAuto.Checked;
                Properties.Settings.Default.SeparateSelectable = CbSeparateSelectable.Checked;
                Properties.Settings.Default.MaxRows = NumMaxRows.Value;
                Properties.Settings.Default.TrimImport = CBTrimImport.Checked;
                Properties.Settings.Default.UnhideRows = CBLoadHiddenRows.Checked;
                Properties.Settings.Default.UnhideColumns = CBLoadHiddenColumns.Checked;
                Properties.Settings.Default.DuplicateCaseSensitive = CBDuplicateCaseSensitive.Checked;
                Properties.Settings.Default.PVMLeadingZero = checkBoxLeadingZero.Checked;
                Properties.Settings.Default.PVMLeadingZeroText = textBoxLeadingZeroText.Text;
                Properties.Settings.Default.PVMLeadingZeroAlias = textBoxLeadingZeroAlias.Text;
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
            foreach (TextBox textBox in textBoxes)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    this.MessagesOK(MessageBoxIcon.Warning, "Bitte geben Sie eine Bezeichnung ein");
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
            if (colorDialog1.ShowDialog(this) == DialogResult.OK)
            {
                sender.BackColor = colorDialog1.Color;
            }
        }

        private void llSourceCode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/Kirdock/Tabellenverarbeitung");
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
            if (folderBrowser.ShowDialog(this) == DialogResult.OK)
            {
                string folderPath = Path.GetDirectoryName(folderBrowser.FileName);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                DialogResult result = MessageHandler.MessagesYesNo(this, MessageBoxIcon.Information, "Soll der Inhalt des alten Ordners in den neuen kopiert werden?");
                if (result == DialogResult.Yes)
                {
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
                }

                Properties.Settings.Default.SettingPath = TxtSettingPath.Text = folderPath;
            }
            folderBrowser.Dispose();
        }

        private void TxTShortcut_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Back)
            {
                Keys modifierKeys = e.Modifiers;
                Keys pressedKey = e.KeyData ^ modifierKeys; //remove modifier keys

                if (modifierKeys != Keys.None && pressedKey != Keys.None)
                {
                    var converter = new KeysConverter();
                    Properties.Settings.Default.EditShortcut = e.KeyData;
                    TxTShortcut.Text = converter.ConvertToString(e.KeyData);
                }
            }
            else
            {
                e.Handled = false;
                e.SuppressKeyPress = true;

                TxTShortcut.Text = string.Empty;
            }
        }

        private void TxTShortcut_Click(object sender, EventArgs e)
        {
            if (ModifierKeys != Keys.None)
            {
                TxTShortcut.KeyDown -= TxTShortcut_KeyDown;

                var converter = new KeysConverter();
                var keyData = ModifierKeys | Keys.LButton;
                Properties.Settings.Default.EditShortcut = keyData;
                TxTShortcut.Text = converter.ConvertToString(keyData);
                ActiveControl = label40;

                TxTShortcut.KeyDown += TxTShortcut_KeyDown;
            }
        }
    }
}
