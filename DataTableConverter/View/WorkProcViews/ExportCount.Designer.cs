namespace DataTableConverter.View
{
    partial class ExportCount
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
            this.cmbColumn = new System.Windows.Forms.ComboBox();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.cbCount = new System.Windows.Forms.CheckBox();
            this.nbCount = new System.Windows.Forms.NumericUpDown();
            this.cbShowFromTo = new System.Windows.Forms.CheckBox();
            this.GbOneColumn = new System.Windows.Forms.GroupBox();
            this.RbOneColumn = new System.Windows.Forms.RadioButton();
            this.RbTwoColumns = new System.Windows.Forms.RadioButton();
            this.GbTwoColumns = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbSecondSecondColumn = new System.Windows.Forms.ComboBox();
            this.CmbSecondFirstColumn = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.LblCount = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nbCount)).BeginInit();
            this.GbOneColumn.SuspendLayout();
            this.GbTwoColumns.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmbColumn
            // 
            this.cmbColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColumn.FormattingEnabled = true;
            this.cmbColumn.Location = new System.Drawing.Point(6, 32);
            this.cmbColumn.Name = "cmbColumn";
            this.cmbColumn.Size = new System.Drawing.Size(188, 21);
            this.cmbColumn.TabIndex = 0;
            // 
            // btnConfirm
            // 
            this.btnConfirm.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnConfirm.Location = new System.Drawing.Point(65, 208);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(84, 23);
            this.btnConfirm.TabIndex = 1;
            this.btnConfirm.Text = "Bestätigen";
            this.btnConfirm.UseVisualStyleBackColor = true;
            // 
            // cbCount
            // 
            this.cbCount.AutoSize = true;
            this.cbCount.Location = new System.Drawing.Point(6, 105);
            this.cbCount.Name = "cbCount";
            this.cbCount.Size = new System.Drawing.Size(58, 17);
            this.cbCount.TabIndex = 2;
            this.cbCount.Text = "Anzahl";
            this.cbCount.UseVisualStyleBackColor = true;
            this.cbCount.CheckedChanged += new System.EventHandler(this.cbCount_CheckedChanged);
            // 
            // nbCount
            // 
            this.nbCount.Location = new System.Drawing.Point(85, 104);
            this.nbCount.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nbCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nbCount.Name = "nbCount";
            this.nbCount.Size = new System.Drawing.Size(109, 20);
            this.nbCount.TabIndex = 3;
            this.nbCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nbCount.Visible = false;
            // 
            // cbShowFromTo
            // 
            this.cbShowFromTo.AutoSize = true;
            this.cbShowFromTo.Location = new System.Drawing.Point(6, 72);
            this.cbShowFromTo.Name = "cbShowFromTo";
            this.cbShowFromTo.Size = new System.Drawing.Size(131, 17);
            this.cbShowFromTo.TabIndex = 4;
            this.cbShowFromTo.Text = "\"Von\", \"Bis\" anzeigen";
            this.cbShowFromTo.UseVisualStyleBackColor = true;
            // 
            // GbOneColumn
            // 
            this.GbOneColumn.Controls.Add(this.cmbColumn);
            this.GbOneColumn.Controls.Add(this.cbShowFromTo);
            this.GbOneColumn.Controls.Add(this.nbCount);
            this.GbOneColumn.Controls.Add(this.cbCount);
            this.GbOneColumn.Location = new System.Drawing.Point(12, 51);
            this.GbOneColumn.Name = "GbOneColumn";
            this.GbOneColumn.Size = new System.Drawing.Size(203, 140);
            this.GbOneColumn.TabIndex = 5;
            this.GbOneColumn.TabStop = false;
            this.GbOneColumn.Text = "Eine Spalte";
            // 
            // RbOneColumn
            // 
            this.RbOneColumn.AutoSize = true;
            this.RbOneColumn.Location = new System.Drawing.Point(18, 12);
            this.RbOneColumn.Name = "RbOneColumn";
            this.RbOneColumn.Size = new System.Drawing.Size(79, 17);
            this.RbOneColumn.TabIndex = 6;
            this.RbOneColumn.TabStop = true;
            this.RbOneColumn.Text = "Eine Spalte";
            this.RbOneColumn.UseVisualStyleBackColor = true;
            this.RbOneColumn.CheckedChanged += new System.EventHandler(this.RbColumn_CheckedChanged);
            // 
            // RbTwoColumns
            // 
            this.RbTwoColumns.AutoSize = true;
            this.RbTwoColumns.Location = new System.Drawing.Point(119, 12);
            this.RbTwoColumns.Name = "RbTwoColumns";
            this.RbTwoColumns.Size = new System.Drawing.Size(87, 17);
            this.RbTwoColumns.TabIndex = 7;
            this.RbTwoColumns.TabStop = true;
            this.RbTwoColumns.Text = "Zwei Spalten";
            this.RbTwoColumns.UseVisualStyleBackColor = true;
            this.RbTwoColumns.CheckedChanged += new System.EventHandler(this.RbColumn_CheckedChanged);
            // 
            // GbTwoColumns
            // 
            this.GbTwoColumns.Controls.Add(this.LblCount);
            this.GbTwoColumns.Controls.Add(this.label3);
            this.GbTwoColumns.Controls.Add(this.label2);
            this.GbTwoColumns.Controls.Add(this.cmbSecondSecondColumn);
            this.GbTwoColumns.Controls.Add(this.CmbSecondFirstColumn);
            this.GbTwoColumns.Controls.Add(this.label1);
            this.GbTwoColumns.Location = new System.Drawing.Point(11, 51);
            this.GbTwoColumns.Name = "GbTwoColumns";
            this.GbTwoColumns.Size = new System.Drawing.Size(204, 140);
            this.GbTwoColumns.TabIndex = 6;
            this.GbTwoColumns.TabStop = false;
            this.GbTwoColumns.Text = "Zwei Spalten";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Spalte 2:";
            // 
            // cmbSecondSecondColumn
            // 
            this.cmbSecondSecondColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSecondSecondColumn.FormattingEnabled = true;
            this.cmbSecondSecondColumn.Location = new System.Drawing.Point(5, 86);
            this.cmbSecondSecondColumn.Name = "cmbSecondSecondColumn";
            this.cmbSecondSecondColumn.Size = new System.Drawing.Size(188, 21);
            this.cmbSecondSecondColumn.TabIndex = 2;
            this.cmbSecondSecondColumn.SelectedIndexChanged += new System.EventHandler(this.CmbSecondFirstColumn_SelectedIndexChanged);
            // 
            // CmbSecondFirstColumn
            // 
            this.CmbSecondFirstColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbSecondFirstColumn.FormattingEnabled = true;
            this.CmbSecondFirstColumn.Location = new System.Drawing.Point(6, 32);
            this.CmbSecondFirstColumn.Name = "CmbSecondFirstColumn";
            this.CmbSecondFirstColumn.Size = new System.Drawing.Size(188, 21);
            this.CmbSecondFirstColumn.TabIndex = 1;
            this.CmbSecondFirstColumn.SelectedIndexChanged += new System.EventHandler(this.CmbSecondFirstColumn_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Spalte 1:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(51, 124);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Anzahl:";
            // 
            // LblCount
            // 
            this.LblCount.AutoSize = true;
            this.LblCount.Location = new System.Drawing.Point(96, 124);
            this.LblCount.Name = "LblCount";
            this.LblCount.Size = new System.Drawing.Size(13, 13);
            this.LblCount.TabIndex = 5;
            this.LblCount.Text = "0";
            // 
            // ExportCount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(231, 254);
            this.Controls.Add(this.GbTwoColumns);
            this.Controls.Add(this.GbOneColumn);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.RbTwoColumns);
            this.Controls.Add(this.RbOneColumn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ExportCount";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Zählen";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ExportCount_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.nbCount)).EndInit();
            this.GbOneColumn.ResumeLayout(false);
            this.GbOneColumn.PerformLayout();
            this.GbTwoColumns.ResumeLayout(false);
            this.GbTwoColumns.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbColumn;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.CheckBox cbCount;
        private System.Windows.Forms.NumericUpDown nbCount;
        private System.Windows.Forms.CheckBox cbShowFromTo;
        private System.Windows.Forms.GroupBox GbOneColumn;
        private System.Windows.Forms.RadioButton RbOneColumn;
        private System.Windows.Forms.RadioButton RbTwoColumns;
        private System.Windows.Forms.GroupBox GbTwoColumns;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbSecondSecondColumn;
        private System.Windows.Forms.ComboBox CmbSecondFirstColumn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label LblCount;
        private System.Windows.Forms.Label label3;
    }
}