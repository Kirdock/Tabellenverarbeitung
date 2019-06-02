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
            this.label1 = new System.Windows.Forms.Label();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.CbSaveAll = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.CmBFormat = new System.Windows.Forms.ComboBox();
            this.cmbFiles = new System.Windows.Forms.ComboBox();
            this.btnDeleteFile = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.gbFiles = new System.Windows.Forms.GroupBox();
            this.clbValues = new DataTableConverter.View.CustomControls.CountListbox();
            this.gbFiles.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmbColumn
            // 
            this.cmbColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColumn.FormattingEnabled = true;
            this.cmbColumn.Location = new System.Drawing.Point(12, 36);
            this.cmbColumn.Name = "cmbColumn";
            this.cmbColumn.Size = new System.Drawing.Size(121, 21);
            this.cmbColumn.TabIndex = 0;
            this.cmbColumn.SelectedIndexChanged += new System.EventHandler(this.cmbColumn_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Spalte:";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(12, 155);
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
            this.CbSaveAll.Location = new System.Drawing.Point(12, 119);
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
            this.label2.Location = new System.Drawing.Point(9, 65);
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
            this.CmBFormat.Location = new System.Drawing.Point(12, 81);
            this.CmBFormat.Name = "CmBFormat";
            this.CmBFormat.Size = new System.Drawing.Size(121, 21);
            this.CmBFormat.TabIndex = 7;
            // 
            // cmbFiles
            // 
            this.cmbFiles.FormattingEnabled = true;
            this.cmbFiles.Location = new System.Drawing.Point(25, 39);
            this.cmbFiles.Name = "cmbFiles";
            this.cmbFiles.Size = new System.Drawing.Size(163, 21);
            this.cmbFiles.TabIndex = 9;
            this.cmbFiles.SelectedIndexChanged += new System.EventHandler(this.cmbFiles_SelectedIndexChanged);
            this.cmbFiles.TextChanged += new System.EventHandler(this.cmbFiles_TextChanged);
            this.cmbFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cmbFiles_KeyDown);
            // 
            // btnDeleteFile
            // 
            this.btnDeleteFile.Location = new System.Drawing.Point(194, 37);
            this.btnDeleteFile.Name = "btnDeleteFile";
            this.btnDeleteFile.Size = new System.Drawing.Size(75, 23);
            this.btnDeleteFile.TabIndex = 12;
            this.btnDeleteFile.Text = "Löschen";
            this.btnDeleteFile.UseVisualStyleBackColor = true;
            this.btnDeleteFile.Click += new System.EventHandler(this.btnDeleteFile_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Dateiname:";
            // 
            // gbFiles
            // 
            this.gbFiles.Controls.Add(this.clbValues);
            this.gbFiles.Controls.Add(this.label3);
            this.gbFiles.Controls.Add(this.cmbFiles);
            this.gbFiles.Controls.Add(this.btnDeleteFile);
            this.gbFiles.Location = new System.Drawing.Point(156, 12);
            this.gbFiles.Name = "gbFiles";
            this.gbFiles.Size = new System.Drawing.Size(297, 360);
            this.gbFiles.TabIndex = 14;
            this.gbFiles.TabStop = false;
            this.gbFiles.Text = "Dateien";
            // 
            // clbValues
            // 
            this.clbValues.CheckOnClick = true;
            this.clbValues.FormattingEnabled = true;
            this.clbValues.Location = new System.Drawing.Point(25, 69);
            this.clbValues.Name = "clbValues";
            this.clbValues.Size = new System.Drawing.Size(244, 274);
            this.clbValues.TabIndex = 14;
            this.clbValues.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbValues_ItemCheck);
            // 
            // ExportCustom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 393);
            this.Controls.Add(this.gbFiles);
            this.Controls.Add(this.CmBFormat);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CbSaveAll);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbColumn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ExportCustom";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Benutzerdefiniertes Exportieren";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ExportCustom_FormClosing);
            this.gbFiles.ResumeLayout(false);
            this.gbFiles.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbColumn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.CheckBox CbSaveAll;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox CmBFormat;
        private System.Windows.Forms.ComboBox cmbFiles;
        private System.Windows.Forms.Button btnDeleteFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox gbFiles;
        private CustomControls.CountListbox clbValues;
    }
}