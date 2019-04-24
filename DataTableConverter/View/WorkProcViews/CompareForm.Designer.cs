namespace DataTableConverter.View.WorkProcViews
{
    partial class CompareForm
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
            this.cbOldColumn = new System.Windows.Forms.CheckBox();
            this.cbNewColumn = new System.Windows.Forms.CheckBox();
            this.lblPadNewColumn = new System.Windows.Forms.Label();
            this.txtNewColumn = new System.Windows.Forms.TextBox();
            this.cbFirstColumn = new System.Windows.Forms.ComboBox();
            this.cbSecondColumn = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbOldColumn
            // 
            this.cbOldColumn.AutoSize = true;
            this.cbOldColumn.Location = new System.Drawing.Point(10, 48);
            this.cbOldColumn.Name = "cbOldColumn";
            this.cbOldColumn.Size = new System.Drawing.Size(165, 17);
            this.cbOldColumn.TabIndex = 41;
            this.cbOldColumn.Text = "Alte Werte in ALT speichern?";
            this.cbOldColumn.UseVisualStyleBackColor = true;
            // 
            // cbNewColumn
            // 
            this.cbNewColumn.AutoSize = true;
            this.cbNewColumn.Location = new System.Drawing.Point(12, 12);
            this.cbNewColumn.Name = "cbNewColumn";
            this.cbNewColumn.Size = new System.Drawing.Size(193, 17);
            this.cbNewColumn.TabIndex = 40;
            this.cbNewColumn.Text = "Ergebnis in neue Spalte schreiben?";
            this.cbNewColumn.UseVisualStyleBackColor = true;
            this.cbNewColumn.CheckedChanged += new System.EventHandler(this.cbNewColumn_CheckedChanged);
            // 
            // lblPadNewColumn
            // 
            this.lblPadNewColumn.AutoSize = true;
            this.lblPadNewColumn.Location = new System.Drawing.Point(8, 32);
            this.lblPadNewColumn.Name = "lblPadNewColumn";
            this.lblPadNewColumn.Size = new System.Drawing.Size(122, 13);
            this.lblPadNewColumn.TabIndex = 39;
            this.lblPadNewColumn.Text = "Name der neuen Spalte:";
            // 
            // txtNewColumn
            // 
            this.txtNewColumn.Location = new System.Drawing.Point(12, 52);
            this.txtNewColumn.Name = "txtNewColumn";
            this.txtNewColumn.Size = new System.Drawing.Size(383, 20);
            this.txtNewColumn.TabIndex = 38;
            // 
            // cbFirstColumn
            // 
            this.cbFirstColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFirstColumn.FormattingEnabled = true;
            this.cbFirstColumn.Location = new System.Drawing.Point(11, 100);
            this.cbFirstColumn.Name = "cbFirstColumn";
            this.cbFirstColumn.Size = new System.Drawing.Size(384, 21);
            this.cbFirstColumn.TabIndex = 42;
            // 
            // cbSecondColumn
            // 
            this.cbSecondColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSecondColumn.FormattingEnabled = true;
            this.cbSecondColumn.Location = new System.Drawing.Point(11, 152);
            this.cbSecondColumn.Name = "cbSecondColumn";
            this.cbSecondColumn.Size = new System.Drawing.Size(384, 21);
            this.cbSecondColumn.TabIndex = 43;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 84);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 44;
            this.label1.Text = "Erste Spalte:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 136);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 45;
            this.label2.Text = "Zweite Spalte:";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(149, 194);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(106, 23);
            this.btnConfirm.TabIndex = 46;
            this.btnConfirm.Text = "Übernehmen";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // CompareForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(409, 235);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbSecondColumn);
            this.Controls.Add(this.cbFirstColumn);
            this.Controls.Add(this.cbOldColumn);
            this.Controls.Add(this.cbNewColumn);
            this.Controls.Add(this.lblPadNewColumn);
            this.Controls.Add(this.txtNewColumn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "CompareForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Spalten vergleichen";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbOldColumn;
        private System.Windows.Forms.CheckBox cbNewColumn;
        private System.Windows.Forms.Label lblPadNewColumn;
        private System.Windows.Forms.TextBox txtNewColumn;
        private System.Windows.Forms.ComboBox cbFirstColumn;
        private System.Windows.Forms.ComboBox cbSecondColumn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnConfirm;
    }
}