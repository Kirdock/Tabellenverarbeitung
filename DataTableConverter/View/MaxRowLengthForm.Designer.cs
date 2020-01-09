namespace DataTableConverter.View
{
    partial class MaxRowLengthForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.TxtNewColumn = new System.Windows.Forms.TextBox();
            this.TxtShortcut = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.BtnConfirm = new System.Windows.Forms.ToolStripMenuItem();
            this.CBMinLength = new System.Windows.Forms.CheckBox();
            this.NumMinLength = new System.Windows.Forms.NumericUpDown();
            this.LblColumn = new System.Windows.Forms.Label();
            this.CmBHeaders = new System.Windows.Forms.ComboBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumMinLength)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Kürzel:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(122, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Name der neuen Spalte:";
            // 
            // TxtNewColumn
            // 
            this.TxtNewColumn.Location = new System.Drawing.Point(152, 32);
            this.TxtNewColumn.Name = "TxtNewColumn";
            this.TxtNewColumn.Size = new System.Drawing.Size(167, 20);
            this.TxtNewColumn.TabIndex = 2;
            // 
            // TxtShortcut
            // 
            this.TxtShortcut.Location = new System.Drawing.Point(152, 65);
            this.TxtShortcut.Name = "TxtShortcut";
            this.TxtShortcut.Size = new System.Drawing.Size(167, 20);
            this.TxtShortcut.TabIndex = 3;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BtnConfirm});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(331, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // BtnConfirm
            // 
            this.BtnConfirm.Name = "BtnConfirm";
            this.BtnConfirm.Size = new System.Drawing.Size(74, 20);
            this.BtnConfirm.Text = "Bestätigen";
            this.BtnConfirm.Click += new System.EventHandler(this.BtnConfirm_Click);
            // 
            // CBMinLength
            // 
            this.CBMinLength.AutoSize = true;
            this.CBMinLength.Location = new System.Drawing.Point(15, 97);
            this.CBMinLength.Name = "CBMinLength";
            this.CBMinLength.Size = new System.Drawing.Size(92, 17);
            this.CBMinLength.TabIndex = 6;
            this.CBMinLength.Text = "Mindestlänge:";
            this.CBMinLength.UseVisualStyleBackColor = true;
            this.CBMinLength.CheckedChanged += new System.EventHandler(this.CBMinLength_CheckedChanged);
            // 
            // NumMinLength
            // 
            this.NumMinLength.Location = new System.Drawing.Point(152, 96);
            this.NumMinLength.Name = "NumMinLength";
            this.NumMinLength.Size = new System.Drawing.Size(60, 20);
            this.NumMinLength.TabIndex = 7;
            // 
            // LblColumn
            // 
            this.LblColumn.AutoSize = true;
            this.LblColumn.Location = new System.Drawing.Point(62, 125);
            this.LblColumn.Name = "LblColumn";
            this.LblColumn.Size = new System.Drawing.Size(40, 13);
            this.LblColumn.TabIndex = 9;
            this.LblColumn.Text = "Spalte:";
            // 
            // CmBHeaders
            // 
            this.CmBHeaders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmBHeaders.FormattingEnabled = true;
            this.CmBHeaders.Location = new System.Drawing.Point(152, 125);
            this.CmBHeaders.Name = "CmBHeaders";
            this.CmBHeaders.Size = new System.Drawing.Size(167, 21);
            this.CmBHeaders.TabIndex = 10;
            // 
            // MaxRowLengthForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 214);
            this.Controls.Add(this.CmBHeaders);
            this.Controls.Add(this.LblColumn);
            this.Controls.Add(this.NumMinLength);
            this.Controls.Add(this.CBMinLength);
            this.Controls.Add(this.TxtShortcut);
            this.Controls.Add(this.TxtNewColumn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MaxRowLengthForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Maximale Zeilenlänge";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumMinLength)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TxtNewColumn;
        private System.Windows.Forms.TextBox TxtShortcut;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem BtnConfirm;
        private System.Windows.Forms.CheckBox CBMinLength;
        private System.Windows.Forms.NumericUpDown NumMinLength;
        private System.Windows.Forms.Label LblColumn;
        private System.Windows.Forms.ComboBox CmBHeaders;
    }
}