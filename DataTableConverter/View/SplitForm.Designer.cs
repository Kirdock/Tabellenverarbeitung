namespace DataTableConverter.View
{
    partial class SplitForm
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
            this.cmbHeaders = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSplitString = new System.Windows.Forms.TextBox();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmbHeaders
            // 
            this.cmbHeaders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHeaders.FormattingEnabled = true;
            this.cmbHeaders.Location = new System.Drawing.Point(12, 32);
            this.cmbHeaders.Name = "cmbHeaders";
            this.cmbHeaders.Size = new System.Drawing.Size(121, 21);
            this.cmbHeaders.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Spalten:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(157, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Aufteilen nach:";
            // 
            // txtSplitString
            // 
            this.txtSplitString.Location = new System.Drawing.Point(160, 33);
            this.txtSplitString.Name = "txtSplitString";
            this.txtSplitString.Size = new System.Drawing.Size(135, 20);
            this.txtSplitString.TabIndex = 3;
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(334, 30);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(92, 23);
            this.btnConfirm.TabIndex = 4;
            this.btnConfirm.Text = "Bestätigen";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // SplitForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(459, 75);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.txtSplitString);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbHeaders);
            this.Name = "SplitForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Spalte aufteilen";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbHeaders;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSplitString;
        private System.Windows.Forms.Button btnConfirm;
    }
}