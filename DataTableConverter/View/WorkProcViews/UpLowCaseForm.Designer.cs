namespace DataTableConverter.View
{
    partial class UpLowCaseForm
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
            this.clbHeaders = new System.Windows.Forms.CheckedListBox();
            this.cbAllColumns = new System.Windows.Forms.CheckBox();
            this.cmbOption = new System.Windows.Forms.ComboBox();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // clbHeaders
            // 
            this.clbHeaders.CheckOnClick = true;
            this.clbHeaders.FormattingEnabled = true;
            this.clbHeaders.HorizontalScrollbar = true;
            this.clbHeaders.Location = new System.Drawing.Point(12, 73);
            this.clbHeaders.Name = "clbHeaders";
            this.clbHeaders.Size = new System.Drawing.Size(205, 184);
            this.clbHeaders.TabIndex = 0;
            // 
            // cbAllColumns
            // 
            this.cbAllColumns.AutoSize = true;
            this.cbAllColumns.Location = new System.Drawing.Point(12, 50);
            this.cbAllColumns.Name = "cbAllColumns";
            this.cbAllColumns.Size = new System.Drawing.Size(159, 17);
            this.cbAllColumns.TabIndex = 1;
            this.cbAllColumns.Text = "Auf alle Spalten anwenden?";
            this.cbAllColumns.UseVisualStyleBackColor = true;
            this.cbAllColumns.CheckedChanged += new System.EventHandler(this.cbAllColumns_CheckedChanged);
            // 
            // cmbOption
            // 
            this.cmbOption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOption.FormattingEnabled = true;
            this.cmbOption.Items.AddRange(new object[] {
            "Alles Großbuchstaben",
            "Alles Kleinbuchstaben",
            "Erster Buchstabe groß",
            "Erste Buchstaben groß"});
            this.cmbOption.Location = new System.Drawing.Point(12, 12);
            this.cmbOption.Name = "cmbOption";
            this.cmbOption.Size = new System.Drawing.Size(205, 21);
            this.cmbOption.TabIndex = 2;
            // 
            // btnSubmit
            // 
            this.btnSubmit.Location = new System.Drawing.Point(61, 264);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(92, 23);
            this.btnSubmit.TabIndex = 3;
            this.btnSubmit.Text = "Übernehmen";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // UpLowCaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(232, 299);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.cmbOption);
            this.Controls.Add(this.cbAllColumns);
            this.Controls.Add(this.clbHeaders);
            this.Name = "UpLowCaseForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "UpLowCaseForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox clbHeaders;
        private System.Windows.Forms.CheckBox cbAllColumns;
        private System.Windows.Forms.ComboBox cmbOption;
        private System.Windows.Forms.Button btnSubmit;
    }
}