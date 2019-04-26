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
            ((System.ComponentModel.ISupportInitialize)(this.nbCount)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbColumn
            // 
            this.cmbColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColumn.FormattingEnabled = true;
            this.cmbColumn.Location = new System.Drawing.Point(12, 12);
            this.cmbColumn.Name = "cmbColumn";
            this.cmbColumn.Size = new System.Drawing.Size(150, 21);
            this.cmbColumn.TabIndex = 0;
            // 
            // btnConfirm
            // 
            this.btnConfirm.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnConfirm.Location = new System.Drawing.Point(177, 10);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(84, 23);
            this.btnConfirm.TabIndex = 1;
            this.btnConfirm.Text = "Bestätigen";
            this.btnConfirm.UseVisualStyleBackColor = true;
            // 
            // cbCount
            // 
            this.cbCount.AutoSize = true;
            this.cbCount.Location = new System.Drawing.Point(12, 85);
            this.cbCount.Name = "cbCount";
            this.cbCount.Size = new System.Drawing.Size(58, 17);
            this.cbCount.TabIndex = 2;
            this.cbCount.Text = "Anzahl";
            this.cbCount.UseVisualStyleBackColor = true;
            this.cbCount.CheckedChanged += new System.EventHandler(this.cbCount_CheckedChanged);
            // 
            // nbCount
            // 
            this.nbCount.Location = new System.Drawing.Point(91, 84);
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
            this.nbCount.Size = new System.Drawing.Size(71, 20);
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
            this.cbShowFromTo.Location = new System.Drawing.Point(12, 52);
            this.cbShowFromTo.Name = "cbShowFromTo";
            this.cbShowFromTo.Size = new System.Drawing.Size(131, 17);
            this.cbShowFromTo.TabIndex = 4;
            this.cbShowFromTo.Text = "\"Von\", \"Bis\" anzeigen";
            this.cbShowFromTo.UseVisualStyleBackColor = true;
            // 
            // ExportCount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 118);
            this.Controls.Add(this.cbShowFromTo);
            this.Controls.Add(this.nbCount);
            this.Controls.Add(this.cbCount);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.cmbColumn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ExportCount";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Zählen";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ExportCount_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.nbCount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbColumn;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.CheckBox cbCount;
        private System.Windows.Forms.NumericUpDown nbCount;
        private System.Windows.Forms.CheckBox cbShowFromTo;
    }
}