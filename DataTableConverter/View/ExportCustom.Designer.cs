namespace DataTableConverter.View
{
    partial class ExportCustom
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
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblSearch = new System.Windows.Forms.Label();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.CbSaveAll = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.CmBFormat = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cmbColumn
            // 
            this.cmbColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColumn.FormattingEnabled = true;
            this.cmbColumn.Location = new System.Drawing.Point(29, 33);
            this.cmbColumn.Name = "cmbColumn";
            this.cmbColumn.Size = new System.Drawing.Size(121, 21);
            this.cmbColumn.TabIndex = 0;
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(184, 33);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(129, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Spalte:";
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(181, 17);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(124, 13);
            this.lblSearch.TabIndex = 3;
            this.lblSearch.Text = "Übereinstimmender Text:";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(115, 118);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(93, 23);
            this.btnConfirm.TabIndex = 4;
            this.btnConfirm.Text = "Bestätigen";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // CbSaveAll
            // 
            this.CbSaveAll.AutoSize = true;
            this.CbSaveAll.Location = new System.Drawing.Point(184, 80);
            this.CbSaveAll.Name = "CbSaveAll";
            this.CbSaveAll.Size = new System.Drawing.Size(130, 17);
            this.CbSaveAll.TabIndex = 5;
            this.CbSaveAll.Text = "Alle Werte speichern?";
            this.CbSaveAll.UseVisualStyleBackColor = true;
            this.CbSaveAll.CheckedChanged += new System.EventHandler(this.CbSaveAll_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Dateiformat:";
            // 
            // CmBFormat
            // 
            this.CmBFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmBFormat.FormattingEnabled = true;
            this.CmBFormat.Items.AddRange(new object[] {
            "CSV",
            "DBASE",
            "Excel"});
            this.CmBFormat.Location = new System.Drawing.Point(29, 78);
            this.CmBFormat.Name = "CmBFormat";
            this.CmBFormat.Size = new System.Drawing.Size(121, 21);
            this.CmBFormat.TabIndex = 7;
            // 
            // ExportCustom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(337, 152);
            this.Controls.Add(this.CmBFormat);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CbSaveAll);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.cmbColumn);
            this.Name = "ExportCustom";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Benutzerdefiniertes Exportieren";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ExportCustom_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbColumn;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.CheckBox CbSaveAll;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox CmBFormat;
    }
}