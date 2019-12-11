﻿namespace DataTableConverter.View
{
    partial class ProcedureForm
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
            this.CbCheckWord = new System.Windows.Forms.CheckBox();
            this.cbCheckTotal = new System.Windows.Forms.CheckBox();
            this.DGVProcedure = new System.Windows.Forms.DataGridView();
            this.BtnConfirm = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.DGVProcedure)).BeginInit();
            this.SuspendLayout();
            // 
            // CbCheckWord
            // 
            this.CbCheckWord.AutoSize = true;
            this.CbCheckWord.Location = new System.Drawing.Point(185, 6);
            this.CbCheckWord.Name = "CbCheckWord";
            this.CbCheckWord.Size = new System.Drawing.Size(128, 17);
            this.CbCheckWord.TabIndex = 14;
            this.CbCheckWord.Text = "Wortübereinstimmung";
            this.CbCheckWord.UseVisualStyleBackColor = true;
            // 
            // cbCheckTotal
            // 
            this.cbCheckTotal.AutoSize = true;
            this.cbCheckTotal.Location = new System.Drawing.Point(9, 6);
            this.cbCheckTotal.Name = "cbCheckTotal";
            this.cbCheckTotal.Size = new System.Drawing.Size(148, 17);
            this.cbCheckTotal.TabIndex = 13;
            this.cbCheckTotal.Text = "Genaue Übereinstimmung";
            this.cbCheckTotal.UseVisualStyleBackColor = true;
            // 
            // DGVProcedure
            // 
            this.DGVProcedure.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DGVProcedure.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DGVProcedure.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGVProcedure.Location = new System.Drawing.Point(0, 31);
            this.DGVProcedure.Name = "DGVProcedure";
            this.DGVProcedure.RowHeadersVisible = false;
            this.DGVProcedure.Size = new System.Drawing.Size(474, 359);
            this.DGVProcedure.TabIndex = 12;
            // 
            // BtnConfirm
            // 
            this.BtnConfirm.Location = new System.Drawing.Point(0, 390);
            this.BtnConfirm.Name = "BtnConfirm";
            this.BtnConfirm.Size = new System.Drawing.Size(474, 32);
            this.BtnConfirm.TabIndex = 15;
            this.BtnConfirm.Text = "Bestätigen";
            this.BtnConfirm.UseVisualStyleBackColor = true;
            this.BtnConfirm.Click += new System.EventHandler(this.BtnConfirm_Click);
            // 
            // ProcedureForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 420);
            this.Controls.Add(this.BtnConfirm);
            this.Controls.Add(this.CbCheckWord);
            this.Controls.Add(this.cbCheckTotal);
            this.Controls.Add(this.DGVProcedure);
            this.Name = "ProcedureForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Suchen & Ersetzen";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProcedureForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.DGVProcedure)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox CbCheckWord;
        private System.Windows.Forms.CheckBox cbCheckTotal;
        private System.Windows.Forms.DataGridView DGVProcedure;
        private System.Windows.Forms.Button BtnConfirm;
    }
}