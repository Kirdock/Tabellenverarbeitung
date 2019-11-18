namespace DataTableConverter.View
{
    partial class MaxRowLengthForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.TxtNewColumn = new System.Windows.Forms.TextBox();
            this.TxtShortcut = new System.Windows.Forms.TextBox();
            this.BtnConfirm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Kürzel:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(122, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Name der neuen Spalte:";
            // 
            // TxtNewColumn
            // 
            this.TxtNewColumn.Location = new System.Drawing.Point(152, 9);
            this.TxtNewColumn.Name = "TxtNewColumn";
            this.TxtNewColumn.Size = new System.Drawing.Size(167, 20);
            this.TxtNewColumn.TabIndex = 2;
            // 
            // TxtShortcut
            // 
            this.TxtShortcut.Location = new System.Drawing.Point(152, 42);
            this.TxtShortcut.Name = "TxtShortcut";
            this.TxtShortcut.Size = new System.Drawing.Size(167, 20);
            this.TxtShortcut.TabIndex = 3;
            // 
            // BtnConfirm
            // 
            this.BtnConfirm.Location = new System.Drawing.Point(126, 80);
            this.BtnConfirm.Name = "BtnConfirm";
            this.BtnConfirm.Size = new System.Drawing.Size(75, 23);
            this.BtnConfirm.TabIndex = 4;
            this.BtnConfirm.Text = "Bestätigen";
            this.BtnConfirm.UseVisualStyleBackColor = true;
            this.BtnConfirm.Click += new System.EventHandler(this.BtnConfirm_Click);
            // 
            // MaxRowLengthForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 115);
            this.Controls.Add(this.BtnConfirm);
            this.Controls.Add(this.TxtShortcut);
            this.Controls.Add(this.TxtNewColumn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MaxRowLengthForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Maximale Zeilenlänge";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TxtNewColumn;
        private System.Windows.Forms.TextBox TxtShortcut;
        private System.Windows.Forms.Button BtnConfirm;
    }
}