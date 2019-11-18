namespace DataTableConverter.View
{
    partial class SelectEncoding
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
            this.CmBEncoding = new System.Windows.Forms.ComboBox();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CmBEncoding
            // 
            this.CmBEncoding.Dock = System.Windows.Forms.DockStyle.Top;
            this.CmBEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmBEncoding.FormattingEnabled = true;
            this.CmBEncoding.Location = new System.Drawing.Point(0, 0);
            this.CmBEncoding.Name = "CmBEncoding";
            this.CmBEncoding.Size = new System.Drawing.Size(295, 21);
            this.CmBEncoding.TabIndex = 0;
            // 
            // btnConfirm
            // 
            this.btnConfirm.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnConfirm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnConfirm.Location = new System.Drawing.Point(0, 21);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(295, 27);
            this.btnConfirm.TabIndex = 1;
            this.btnConfirm.Text = "Bestätigen";
            this.btnConfirm.UseVisualStyleBackColor = true;
            // 
            // SelectEncoding
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(295, 48);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.CmBEncoding);
            this.Name = "SelectEncoding";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CodePage";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox CmBEncoding;
        private System.Windows.Forms.Button btnConfirm;
    }
}