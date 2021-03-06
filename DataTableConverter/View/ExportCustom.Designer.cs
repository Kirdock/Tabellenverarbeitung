﻿namespace DataTableConverter.View
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
            this.BtnDeleteFile = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.lblSumCount = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.CmBFileNames = new System.Windows.Forms.ComboBox();
            this.BtnAdd = new System.Windows.Forms.Button();
            this.BtnRename = new System.Windows.Forms.Button();
            this.BtnCheckAll = new System.Windows.Forms.Button();
            this.BtnUncheckAll = new System.Windows.Forms.Button();
            this.CbContinuedNumber = new System.Windows.Forms.CheckBox();
            this.TxtContinuedNumber = new System.Windows.Forms.TextBox();
            this.LblContinuedNumber = new System.Windows.Forms.Label();
            this.clbValues = new DataTableConverter.View.CustomControls.CountListbox();
            this.SuspendLayout();
            // 
            // cmbColumn
            // 
            this.cmbColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColumn.FormattingEnabled = true;
            this.cmbColumn.Location = new System.Drawing.Point(12, 194);
            this.cmbColumn.Name = "cmbColumn";
            this.cmbColumn.Size = new System.Drawing.Size(121, 21);
            this.cmbColumn.TabIndex = 0;
            this.cmbColumn.SelectedIndexChanged += new System.EventHandler(this.cmbColumn_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 178);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Spalte:";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(336, 19);
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
            this.CbSaveAll.Location = new System.Drawing.Point(11, 230);
            this.CbSaveAll.Name = "CbSaveAll";
            this.CbSaveAll.Size = new System.Drawing.Size(160, 17);
            this.CbSaveAll.TabIndex = 5;
            this.CbSaveAll.Text = "Alle Werte einzeln speichern";
            this.CbSaveAll.UseVisualStyleBackColor = true;
            this.CbSaveAll.CheckedChanged += new System.EventHandler(this.CbSaveAll_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(149, 178);
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
            this.CmBFormat.Location = new System.Drawing.Point(152, 194);
            this.CmBFormat.Name = "CmBFormat";
            this.CmBFormat.Size = new System.Drawing.Size(121, 21);
            this.CmBFormat.TabIndex = 7;
            this.CmBFormat.SelectedIndexChanged += new System.EventHandler(this.CmBFormat_SelectedIndexChanged);
            // 
            // BtnDeleteFile
            // 
            this.BtnDeleteFile.Location = new System.Drawing.Point(198, 19);
            this.BtnDeleteFile.Name = "BtnDeleteFile";
            this.BtnDeleteFile.Size = new System.Drawing.Size(75, 23);
            this.BtnDeleteFile.TabIndex = 12;
            this.BtnDeleteFile.Text = "Löschen";
            this.BtnDeleteFile.UseVisualStyleBackColor = true;
            this.BtnDeleteFile.Click += new System.EventHandler(this.btnDeleteFile_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 129);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Dateien:";
            // 
            // lblSumCount
            // 
            this.lblSumCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSumCount.AutoSize = true;
            this.lblSumCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSumCount.Location = new System.Drawing.Point(132, 510);
            this.lblSumCount.Name = "lblSumCount";
            this.lblSumCount.Size = new System.Drawing.Size(18, 20);
            this.lblSumCount.TabIndex = 16;
            this.lblSumCount.Text = "0";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(9, 510);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(117, 20);
            this.label4.TabIndex = 15;
            this.label4.Text = "Gesamtanzahl:";
            // 
            // CmBFileNames
            // 
            this.CmBFileNames.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmBFileNames.FormattingEnabled = true;
            this.CmBFileNames.Location = new System.Drawing.Point(12, 143);
            this.CmBFileNames.Name = "CmBFileNames";
            this.CmBFileNames.Size = new System.Drawing.Size(261, 21);
            this.CmBFileNames.TabIndex = 17;
            this.CmBFileNames.SelectedIndexChanged += new System.EventHandler(this.CmBFileNames_SelectedIndexChanged);
            // 
            // BtnAdd
            // 
            this.BtnAdd.Location = new System.Drawing.Point(11, 19);
            this.BtnAdd.Name = "BtnAdd";
            this.BtnAdd.Size = new System.Drawing.Size(75, 23);
            this.BtnAdd.TabIndex = 18;
            this.BtnAdd.Text = "Hinzufügen";
            this.BtnAdd.UseVisualStyleBackColor = true;
            this.BtnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
            // 
            // BtnRename
            // 
            this.BtnRename.Location = new System.Drawing.Point(92, 19);
            this.BtnRename.Name = "BtnRename";
            this.BtnRename.Size = new System.Drawing.Size(100, 23);
            this.BtnRename.TabIndex = 19;
            this.BtnRename.Text = "Umbenennen";
            this.BtnRename.UseVisualStyleBackColor = true;
            this.BtnRename.Click += new System.EventHandler(this.BtnRename_Click);
            // 
            // BtnCheckAll
            // 
            this.BtnCheckAll.Location = new System.Drawing.Point(198, 226);
            this.BtnCheckAll.Name = "BtnCheckAll";
            this.BtnCheckAll.Size = new System.Drawing.Size(108, 23);
            this.BtnCheckAll.TabIndex = 20;
            this.BtnCheckAll.Text = "Alles auswählen";
            this.BtnCheckAll.UseVisualStyleBackColor = true;
            this.BtnCheckAll.Click += new System.EventHandler(this.BtnCheckAll_Click);
            // 
            // BtnUncheckAll
            // 
            this.BtnUncheckAll.Location = new System.Drawing.Point(321, 226);
            this.BtnUncheckAll.Name = "BtnUncheckAll";
            this.BtnUncheckAll.Size = new System.Drawing.Size(108, 23);
            this.BtnUncheckAll.TabIndex = 21;
            this.BtnUncheckAll.Text = "Alles abwählen";
            this.BtnUncheckAll.UseVisualStyleBackColor = true;
            this.BtnUncheckAll.Click += new System.EventHandler(this.BtnUncheckAll_Click);
            // 
            // CbContinuedNumber
            // 
            this.CbContinuedNumber.AutoSize = true;
            this.CbContinuedNumber.Location = new System.Drawing.Point(13, 58);
            this.CbContinuedNumber.Name = "CbContinuedNumber";
            this.CbContinuedNumber.Size = new System.Drawing.Size(182, 17);
            this.CbContinuedNumber.TabIndex = 22;
            this.CbContinuedNumber.Text = "Fortlaufende Nummer hinzufügen";
            this.CbContinuedNumber.UseVisualStyleBackColor = true;
            this.CbContinuedNumber.CheckedChanged += new System.EventHandler(this.CbContinuedNumber_CheckedChanged);
            // 
            // TxtContinuedNumber
            // 
            this.TxtContinuedNumber.Location = new System.Drawing.Point(11, 99);
            this.TxtContinuedNumber.Name = "TxtContinuedNumber";
            this.TxtContinuedNumber.Size = new System.Drawing.Size(262, 20);
            this.TxtContinuedNumber.TabIndex = 23;
            this.TxtContinuedNumber.Text = "FTNR";
            this.TxtContinuedNumber.Visible = false;
            // 
            // LblContinuedNumber
            // 
            this.LblContinuedNumber.AutoSize = true;
            this.LblContinuedNumber.Location = new System.Drawing.Point(9, 78);
            this.LblContinuedNumber.Name = "LblContinuedNumber";
            this.LblContinuedNumber.Size = new System.Drawing.Size(72, 13);
            this.LblContinuedNumber.TabIndex = 24;
            this.LblContinuedNumber.Text = "Spaltenname:";
            this.LblContinuedNumber.Visible = false;
            // 
            // clbValues
            // 
            this.clbValues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clbValues.CheckOnClick = true;
            this.clbValues.FormattingEnabled = true;
            this.clbValues.HorizontalScrollbar = true;
            this.clbValues.Location = new System.Drawing.Point(12, 259);
            this.clbValues.Name = "clbValues";
            this.clbValues.Size = new System.Drawing.Size(417, 229);
            this.clbValues.TabIndex = 14;
            this.clbValues.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbValues_ItemCheck);
            // 
            // ExportCustom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(441, 544);
            this.Controls.Add(this.LblContinuedNumber);
            this.Controls.Add(this.TxtContinuedNumber);
            this.Controls.Add(this.CbContinuedNumber);
            this.Controls.Add(this.BtnUncheckAll);
            this.Controls.Add(this.BtnCheckAll);
            this.Controls.Add(this.BtnRename);
            this.Controls.Add(this.CmBFormat);
            this.Controls.Add(this.BtnAdd);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CmBFileNames);
            this.Controls.Add(this.CbSaveAll);
            this.Controls.Add(this.lblSumCount);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.clbValues);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbColumn);
            this.Controls.Add(this.BtnDeleteFile);
            this.MinimumSize = new System.Drawing.Size(457, 500);
            this.Name = "ExportCustom";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Trennen";
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
        private System.Windows.Forms.Button BtnDeleteFile;
        private System.Windows.Forms.Label label3;
        private CustomControls.CountListbox clbValues;
        private System.Windows.Forms.Label lblSumCount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button BtnAdd;
        private System.Windows.Forms.ComboBox CmBFileNames;
        private System.Windows.Forms.Button BtnRename;
        private System.Windows.Forms.Button BtnCheckAll;
        private System.Windows.Forms.Button BtnUncheckAll;
        private System.Windows.Forms.CheckBox CbContinuedNumber;
        private System.Windows.Forms.TextBox TxtContinuedNumber;
        private System.Windows.Forms.Label LblContinuedNumber;
    }
}