namespace DataTableConverter.View
{
    partial class SettingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabSettings = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.cLocked = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.cRequired = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.tpFileShortcuts = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.cbPVMSaveFormat = new System.Windows.Forms.ComboBox();
            this.cbSplitPVM = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtInvalidColumn = new System.Windows.Forms.TextBox();
            this.txtFailAddressValue = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtPVM = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtRightAddress = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtFailAddress = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.txtOldAffix = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.cbHeaderUpperCase = new System.Windows.Forms.CheckBox();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.cbAutoSavePVM = new System.Windows.Forms.CheckBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.tabSettings.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tpFileShortcuts.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabSettings
            // 
            this.tabSettings.Controls.Add(this.tabPage1);
            this.tabSettings.Controls.Add(this.tpFileShortcuts);
            this.tabSettings.Controls.Add(this.tabPage2);
            this.tabSettings.Controls.Add(this.tabPage3);
            this.tabSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabSettings.Location = new System.Drawing.Point(0, 0);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.SelectedIndex = 0;
            this.tabSettings.Size = new System.Drawing.Size(800, 450);
            this.tabSettings.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.cLocked);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.cRequired);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(792, 424);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Farben";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // cLocked
            // 
            this.cLocked.Location = new System.Drawing.Point(79, 63);
            this.cLocked.Name = "cLocked";
            this.cLocked.Size = new System.Drawing.Size(20, 20);
            this.cLocked.TabIndex = 2;
            this.cLocked.Click += new System.EventHandler(this.cRequired_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Gesperrt:";
            // 
            // cRequired
            // 
            this.cRequired.Location = new System.Drawing.Point(79, 17);
            this.cRequired.Name = "cRequired";
            this.cRequired.Size = new System.Drawing.Size(20, 20);
            this.cRequired.TabIndex = 1;
            this.cRequired.Click += new System.EventHandler(this.cRequired_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Mussfelder:";
            // 
            // tpFileShortcuts
            // 
            this.tpFileShortcuts.Controls.Add(this.label8);
            this.tpFileShortcuts.Controls.Add(this.cbPVMSaveFormat);
            this.tpFileShortcuts.Controls.Add(this.cbSplitPVM);
            this.tpFileShortcuts.Controls.Add(this.label7);
            this.tpFileShortcuts.Controls.Add(this.txtInvalidColumn);
            this.tpFileShortcuts.Controls.Add(this.txtFailAddressValue);
            this.tpFileShortcuts.Controls.Add(this.label6);
            this.tpFileShortcuts.Controls.Add(this.txtPVM);
            this.tpFileShortcuts.Controls.Add(this.label5);
            this.tpFileShortcuts.Controls.Add(this.txtRightAddress);
            this.tpFileShortcuts.Controls.Add(this.label4);
            this.tpFileShortcuts.Controls.Add(this.txtFailAddress);
            this.tpFileShortcuts.Controls.Add(this.label3);
            this.tpFileShortcuts.Location = new System.Drawing.Point(4, 22);
            this.tpFileShortcuts.Name = "tpFileShortcuts";
            this.tpFileShortcuts.Padding = new System.Windows.Forms.Padding(3);
            this.tpFileShortcuts.Size = new System.Drawing.Size(792, 424);
            this.tpFileShortcuts.TabIndex = 1;
            this.tpFileShortcuts.Text = "Dateikürzel";
            this.tpFileShortcuts.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(250, 216);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 13);
            this.label8.TabIndex = 12;
            this.label8.Text = "Format:";
            // 
            // cbPVMSaveFormat
            // 
            this.cbPVMSaveFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPVMSaveFormat.FormattingEnabled = true;
            this.cbPVMSaveFormat.Items.AddRange(new object[] {
            "CSV",
            "DBASE",
            "Excel"});
            this.cbPVMSaveFormat.Location = new System.Drawing.Point(298, 211);
            this.cbPVMSaveFormat.Name = "cbPVMSaveFormat";
            this.cbPVMSaveFormat.Size = new System.Drawing.Size(121, 21);
            this.cbPVMSaveFormat.TabIndex = 11;
            // 
            // cbSplitPVM
            // 
            this.cbSplitPVM.AutoSize = true;
            this.cbSplitPVM.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbSplitPVM.Location = new System.Drawing.Point(6, 215);
            this.cbSplitPVM.Name = "cbSplitPVM";
            this.cbSplitPVM.Size = new System.Drawing.Size(217, 17);
            this.cbSplitPVM.TabIndex = 10;
            this.cbSplitPVM.Text = "Aufteilen der Adressen nach PVM-Import";
            this.cbSplitPVM.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 141);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(157, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "Spalte der ungültigen Adressen:";
            // 
            // txtInvalidColumn
            // 
            this.txtInvalidColumn.Location = new System.Drawing.Point(192, 138);
            this.txtInvalidColumn.Name = "txtInvalidColumn";
            this.txtInvalidColumn.Size = new System.Drawing.Size(100, 20);
            this.txtInvalidColumn.TabIndex = 8;
            // 
            // txtFailAddressValue
            // 
            this.txtFailAddressValue.Location = new System.Drawing.Point(192, 176);
            this.txtFailAddressValue.Name = "txtFailAddressValue";
            this.txtFailAddressValue.Size = new System.Drawing.Size(100, 20);
            this.txtFailAddressValue.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 176);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(178, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Identifizierung fehlerhafter Adressen:";
            // 
            // txtPVM
            // 
            this.txtPVM.Location = new System.Drawing.Point(192, 97);
            this.txtPVM.Name = "txtPVM";
            this.txtPVM.Size = new System.Drawing.Size(100, 20);
            this.txtPVM.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 97);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(108, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Kürzel für PVM Datei:";
            // 
            // txtRightAddress
            // 
            this.txtRightAddress.Location = new System.Drawing.Point(192, 58);
            this.txtRightAddress.Name = "txtRightAddress";
            this.txtRightAddress.Size = new System.Drawing.Size(100, 20);
            this.txtRightAddress.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 58);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(147, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Kürzel der richtigen Adressen:";
            // 
            // txtFailAddress
            // 
            this.txtFailAddress.Location = new System.Drawing.Point(192, 18);
            this.txtFailAddress.Name = "txtFailAddress";
            this.txtFailAddress.Size = new System.Drawing.Size(100, 20);
            this.txtFailAddress.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(163, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Kürzel der fehlerhaften Adressen:";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.cbAutoSavePVM);
            this.tabPage2.Controls.Add(this.txtOldAffix);
            this.tabPage2.Controls.Add(this.label9);
            this.tabPage2.Controls.Add(this.cbHeaderUpperCase);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(792, 424);
            this.tabPage2.TabIndex = 2;
            this.tabPage2.Text = "Sonstiges";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // txtOldAffix
            // 
            this.txtOldAffix.Location = new System.Drawing.Point(217, 42);
            this.txtOldAffix.Name = "txtOldAffix";
            this.txtOldAffix.Size = new System.Drawing.Size(100, 20);
            this.txtOldAffix.TabIndex = 2;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 49);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(169, 13);
            this.label9.TabIndex = 1;
            this.label9.Text = "Alte Werte in ALT speichern. Affix:";
            // 
            // cbHeaderUpperCase
            // 
            this.cbHeaderUpperCase.AutoSize = true;
            this.cbHeaderUpperCase.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbHeaderUpperCase.Location = new System.Drawing.Point(8, 18);
            this.cbHeaderUpperCase.Name = "cbHeaderUpperCase";
            this.cbHeaderUpperCase.Size = new System.Drawing.Size(224, 17);
            this.cbHeaderUpperCase.TabIndex = 0;
            this.cbHeaderUpperCase.Text = "Überschriften in Großbuchstaben einlesen";
            this.cbHeaderUpperCase.UseVisualStyleBackColor = true;
            // 
            // cbAutoSavePVM
            // 
            this.cbAutoSavePVM.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbAutoSavePVM.Location = new System.Drawing.Point(8, 87);
            this.cbAutoSavePVM.Name = "cbAutoSavePVM";
            this.cbAutoSavePVM.Size = new System.Drawing.Size(224, 17);
            this.cbAutoSavePVM.TabIndex = 3;
            this.cbAutoSavePVM.Text = "PVM automatisch speichern";
            this.cbAutoSavePVM.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.label18);
            this.tabPage3.Controls.Add(this.label17);
            this.tabPage3.Controls.Add(this.label16);
            this.tabPage3.Controls.Add(this.label15);
            this.tabPage3.Controls.Add(this.label14);
            this.tabPage3.Controls.Add(this.label13);
            this.tabPage3.Controls.Add(this.label12);
            this.tabPage3.Controls.Add(this.label11);
            this.tabPage3.Controls.Add(this.label10);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(792, 424);
            this.tabPage3.TabIndex = 3;
            this.tabPage3.Text = "Hilfe";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(8, 14);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(72, 20);
            this.label10.TabIndex = 0;
            this.label10.Text = "Formular";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(9, 52);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(82, 13);
            this.label11.TabIndex = 1;
            this.label11.Text = "Spaltenangabe:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(9, 85);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(225, 13);
            this.label12.TabIndex = 2;
            this.label12.Text = "Bedingte Spalten (die erste nicht-leere Spalte):";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(291, 52);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(49, 13);
            this.label13.TabIndex = 3;
            this.label13.Text = "[Spalte1]";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(291, 85);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(145, 13);
            this.label14.TabIndex = 4;
            this.label14.Text = "([Spalte1] [Spalte2] [Spalte3])";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(9, 118);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(79, 13);
            this.label15.TabIndex = 5;
            this.label15.Text = "Bedingter Text:";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(291, 118);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(80, 13);
            this.label16.TabIndex = 6;
            this.label16.Text = "[Spalte1|R] abc";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(291, 147);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(78, 13);
            this.label17.TabIndex = 7;
            this.label17.Text = "abc [Spalte1|L]";
            // 
            // label18
            // 
            this.label18.Location = new System.Drawing.Point(466, 118);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(200, 42);
            this.label18.TabIndex = 8;
            this.label18.Text = "|R, |L: Text rechts bzw. links wird nicht dazugeschrieben, falls die Spalte leer " +
    "ist";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabSettings);
            this.Name = "SettingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Einstellungen";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingForm_FormClosing);
            this.tabSettings.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tpFileShortcuts.ResumeLayout(false);
            this.tpFileShortcuts.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabSettings;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Panel cRequired;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Panel cLocked;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage tpFileShortcuts;
        private System.Windows.Forms.TextBox txtPVM;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtRightAddress;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtFailAddress;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtFailAddressValue;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtInvalidColumn;
        private System.Windows.Forms.CheckBox cbSplitPVM;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.CheckBox cbHeaderUpperCase;
        private System.Windows.Forms.ComboBox cbPVMSaveFormat;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtOldAffix;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox cbAutoSavePVM;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
    }
}