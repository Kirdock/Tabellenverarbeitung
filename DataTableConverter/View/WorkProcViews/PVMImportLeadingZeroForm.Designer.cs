
namespace DataTableConverter.View.WorkProcViews
{
    partial class PVMImportLeadingZeroForm
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
            this.labelLeadingColumn = new System.Windows.Forms.Label();
            this.textBoxCharacter = new System.Windows.Forms.TextBox();
            this.labelLeadingCharacter = new System.Windows.Forms.Label();
            this.comboBoxColumnReplaceLeadingZero = new System.Windows.Forms.ComboBox();
            this.buttonConfirm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelLeadingColumn
            // 
            this.labelLeadingColumn.AutoSize = true;
            this.labelLeadingColumn.Location = new System.Drawing.Point(14, 40);
            this.labelLeadingColumn.Name = "labelLeadingColumn";
            this.labelLeadingColumn.Size = new System.Drawing.Size(40, 13);
            this.labelLeadingColumn.TabIndex = 25;
            this.labelLeadingColumn.Text = "Spalte:";
            // 
            // textBoxCharacter
            // 
            this.textBoxCharacter.Location = new System.Drawing.Point(75, 11);
            this.textBoxCharacter.Name = "textBoxCharacter";
            this.textBoxCharacter.Size = new System.Drawing.Size(192, 20);
            this.textBoxCharacter.TabIndex = 24;
            // 
            // labelLeadingCharacter
            // 
            this.labelLeadingCharacter.AutoSize = true;
            this.labelLeadingCharacter.Location = new System.Drawing.Point(14, 14);
            this.labelLeadingCharacter.Name = "labelLeadingCharacter";
            this.labelLeadingCharacter.Size = new System.Drawing.Size(49, 13);
            this.labelLeadingCharacter.TabIndex = 23;
            this.labelLeadingCharacter.Text = "Zeichen:";
            // 
            // comboBoxColumnReplaceLeadingZero
            // 
            this.comboBoxColumnReplaceLeadingZero.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxColumnReplaceLeadingZero.FormattingEnabled = true;
            this.comboBoxColumnReplaceLeadingZero.Location = new System.Drawing.Point(75, 37);
            this.comboBoxColumnReplaceLeadingZero.Name = "comboBoxColumnReplaceLeadingZero";
            this.comboBoxColumnReplaceLeadingZero.Size = new System.Drawing.Size(192, 21);
            this.comboBoxColumnReplaceLeadingZero.TabIndex = 22;
            // 
            // buttonConfirm
            // 
            this.buttonConfirm.Location = new System.Drawing.Point(107, 82);
            this.buttonConfirm.Name = "buttonConfirm";
            this.buttonConfirm.Size = new System.Drawing.Size(75, 23);
            this.buttonConfirm.TabIndex = 26;
            this.buttonConfirm.Text = "Bestätigen";
            this.buttonConfirm.UseVisualStyleBackColor = true;
            this.buttonConfirm.Click += new System.EventHandler(this.buttonConfirm_Click);
            // 
            // PVMImportLeadingZeroForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(279, 117);
            this.Controls.Add(this.buttonConfirm);
            this.Controls.Add(this.labelLeadingColumn);
            this.Controls.Add(this.textBoxCharacter);
            this.Controls.Add(this.labelLeadingCharacter);
            this.Controls.Add(this.comboBoxColumnReplaceLeadingZero);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "PVMImportLeadingZeroForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Führende Null ersetzen";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelLeadingColumn;
        private System.Windows.Forms.TextBox textBoxCharacter;
        private System.Windows.Forms.Label labelLeadingCharacter;
        private System.Windows.Forms.ComboBox comboBoxColumnReplaceLeadingZero;
        private System.Windows.Forms.Button buttonConfirm;
    }
}