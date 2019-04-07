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
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.tpFileShortcuts = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.txtFailAddress = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtRightAddress = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtPVM = new System.Windows.Forms.TextBox();
            this.tabSettings.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tpFileShortcuts.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabSettings
            // 
            this.tabSettings.Controls.Add(this.tabPage1);
            this.tabSettings.Controls.Add(this.tpFileShortcuts);
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
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(163, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Kürzel der fehlerhaften Adressen:";
            // 
            // txtFailAddress
            // 
            this.txtFailAddress.Location = new System.Drawing.Point(192, 18);
            this.txtFailAddress.Name = "txtFailAddress";
            this.txtFailAddress.Size = new System.Drawing.Size(100, 20);
            this.txtFailAddress.TabIndex = 1;
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
            // txtRightAddress
            // 
            this.txtRightAddress.Location = new System.Drawing.Point(192, 58);
            this.txtRightAddress.Name = "txtRightAddress";
            this.txtRightAddress.Size = new System.Drawing.Size(100, 20);
            this.txtRightAddress.TabIndex = 3;
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
            // txtPVM
            // 
            this.txtPVM.Location = new System.Drawing.Point(192, 97);
            this.txtPVM.Name = "txtPVM";
            this.txtPVM.Size = new System.Drawing.Size(100, 20);
            this.txtPVM.TabIndex = 5;
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
    }
}