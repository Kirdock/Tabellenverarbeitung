﻿namespace DataTableConverter.View
{
    partial class MergeColumns
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
            this.CmBHeaders = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.BtnCheckAll = new System.Windows.Forms.Button();
            this.BtnUncheckAll = new System.Windows.Forms.Button();
            this.ClBHeaders = new DataTableConverter.View.CustomControls.PlusListbox();
            this.label2 = new System.Windows.Forms.Label();
            this.CBSeparator = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // CmBHeaders
            // 
            this.CmBHeaders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmBHeaders.FormattingEnabled = true;
            this.CmBHeaders.Location = new System.Drawing.Point(12, 27);
            this.CmBHeaders.Name = "CmBHeaders";
            this.CmBHeaders.Size = new System.Drawing.Size(121, 21);
            this.CmBHeaders.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Identifizierende Spalte:";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(12, 308);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(121, 23);
            this.btnConfirm.TabIndex = 3;
            this.btnConfirm.Text = "Bestätigen";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // BtnCheckAll
            // 
            this.BtnCheckAll.Location = new System.Drawing.Point(173, 25);
            this.BtnCheckAll.Name = "BtnCheckAll";
            this.BtnCheckAll.Size = new System.Drawing.Size(91, 23);
            this.BtnCheckAll.TabIndex = 4;
            this.BtnCheckAll.Text = "Alles auswählen";
            this.BtnCheckAll.UseVisualStyleBackColor = true;
            this.BtnCheckAll.Click += new System.EventHandler(this.BtnCheckAll_Click);
            // 
            // BtnUncheckAll
            // 
            this.BtnUncheckAll.Location = new System.Drawing.Point(357, 25);
            this.BtnUncheckAll.Name = "BtnUncheckAll";
            this.BtnUncheckAll.Size = new System.Drawing.Size(91, 23);
            this.BtnUncheckAll.TabIndex = 5;
            this.BtnUncheckAll.Text = "Alles abwählen";
            this.BtnUncheckAll.UseVisualStyleBackColor = true;
            this.BtnUncheckAll.Click += new System.EventHandler(this.BtnUncheckAll_Click);
            // 
            // ClBHeaders
            // 
            this.ClBHeaders.CheckOnClick = true;
            this.ClBHeaders.FormattingEnabled = true;
            this.ClBHeaders.HorizontalScrollbar = true;
            this.ClBHeaders.Location = new System.Drawing.Point(173, 57);
            this.ClBHeaders.Name = "ClBHeaders";
            this.ClBHeaders.Size = new System.Drawing.Size(275, 274);
            this.ClBHeaders.TabIndex = 6;
            this.ClBHeaders.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ClBHeaders_MouseDown);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(469, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(193, 48);
            this.label2.TabIndex = 7;
            this.label2.Text = "Per Rechtsklick legt man bei einer Spalte fest, ob sie gezählt oder addiert werde" +
    "n soll";
            // 
            // CBSeparator
            // 
            this.CBSeparator.AutoSize = true;
            this.CBSeparator.Location = new System.Drawing.Point(12, 57);
            this.CBSeparator.Name = "CBSeparator";
            this.CBSeparator.Size = new System.Drawing.Size(145, 17);
            this.CBSeparator.TabIndex = 8;
            this.CBSeparator.Text = "Tausender-Trennzeichen";
            this.CBSeparator.UseVisualStyleBackColor = true;
            // 
            // MergeColumns
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.CBSeparator);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ClBHeaders);
            this.Controls.Add(this.BtnUncheckAll);
            this.Controls.Add(this.BtnCheckAll);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CmBHeaders);
            this.Name = "MergeColumns";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Zeilen zusammenfügen";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox CmBHeaders;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button BtnCheckAll;
        private System.Windows.Forms.Button BtnUncheckAll;
        private CustomControls.PlusListbox ClBHeaders;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox CBSeparator;
    }
}