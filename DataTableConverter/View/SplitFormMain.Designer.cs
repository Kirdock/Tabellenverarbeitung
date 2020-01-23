namespace DataTableConverter.View
{
    partial class SplitFormMain
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
            this.CmBHeader = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TxTSplitText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.TxTNewColumn = new System.Windows.Forms.TextBox();
            this.BtnConfirm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CmBHeader
            // 
            this.CmBHeader.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmBHeader.FormattingEnabled = true;
            this.CmBHeader.Location = new System.Drawing.Point(12, 23);
            this.CmBHeader.Name = "CmBHeader";
            this.CmBHeader.Size = new System.Drawing.Size(121, 21);
            this.CmBHeader.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Spalte:";
            // 
            // TxTSplitText
            // 
            this.TxTSplitText.Location = new System.Drawing.Point(162, 23);
            this.TxTSplitText.Name = "TxTSplitText";
            this.TxTSplitText.Size = new System.Drawing.Size(100, 20);
            this.TxTSplitText.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(159, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Text:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(300, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(122, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Name der neuen Spalte:";
            // 
            // TxTNewColumn
            // 
            this.TxTNewColumn.Location = new System.Drawing.Point(303, 23);
            this.TxTNewColumn.Name = "TxTNewColumn";
            this.TxTNewColumn.Size = new System.Drawing.Size(128, 20);
            this.TxTNewColumn.TabIndex = 5;
            // 
            // BtnConfirm
            // 
            this.BtnConfirm.Location = new System.Drawing.Point(464, 21);
            this.BtnConfirm.Name = "BtnConfirm";
            this.BtnConfirm.Size = new System.Drawing.Size(84, 23);
            this.BtnConfirm.TabIndex = 6;
            this.BtnConfirm.Text = "Bestätigen";
            this.BtnConfirm.UseVisualStyleBackColor = true;
            this.BtnConfirm.Click += new System.EventHandler(this.BtnConfirm_Click);
            // 
            // SplitFormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(574, 57);
            this.Controls.Add(this.BtnConfirm);
            this.Controls.Add(this.TxTNewColumn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TxTSplitText);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CmBHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "SplitFormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Aufteilen";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox CmBHeader;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TxTSplitText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TxTNewColumn;
        private System.Windows.Forms.Button BtnConfirm;
    }
}