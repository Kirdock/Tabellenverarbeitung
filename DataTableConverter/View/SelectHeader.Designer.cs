namespace DataTableConverter.View
{
    partial class SelectHeader
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
            this.dataSourceLabel = new System.Windows.Forms.Label();
            this.BtnConfirm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CmBHeaders
            // 
            this.CmBHeaders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmBHeaders.FormattingEnabled = true;
            this.CmBHeaders.Location = new System.Drawing.Point(48, 35);
            this.CmBHeaders.Name = "CmBHeaders";
            this.CmBHeaders.Size = new System.Drawing.Size(235, 21);
            this.CmBHeaders.TabIndex = 0;
            // 
            // dataSourceLabel
            // 
            this.dataSourceLabel.AutoSize = true;
            this.dataSourceLabel.Location = new System.Drawing.Point(45, 19);
            this.dataSourceLabel.Name = "dataSourceLabel";
            this.dataSourceLabel.Size = new System.Drawing.Size(40, 13);
            this.dataSourceLabel.TabIndex = 1;
            this.dataSourceLabel.Text = "Spalte:";
            // 
            // BtnConfirm
            // 
            this.BtnConfirm.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BtnConfirm.Location = new System.Drawing.Point(125, 70);
            this.BtnConfirm.Name = "BtnConfirm";
            this.BtnConfirm.Size = new System.Drawing.Size(75, 23);
            this.BtnConfirm.TabIndex = 2;
            this.BtnConfirm.Text = "Bestätigen";
            this.BtnConfirm.UseVisualStyleBackColor = true;
            // 
            // SelectHeader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(339, 105);
            this.Controls.Add(this.BtnConfirm);
            this.Controls.Add(this.dataSourceLabel);
            this.Controls.Add(this.CmBHeaders);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "SelectHeader";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Auswahl der Spalte";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox CmBHeaders;
        private System.Windows.Forms.Label dataSourceLabel;
        private System.Windows.Forms.Button BtnConfirm;
    }
}