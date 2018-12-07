namespace DataTableConverter.View
{
    partial class Formula
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
            this.txtFormula = new System.Windows.Forms.TextBox();
            this.txtHeader = new System.Windows.Forms.TextBox();
            this.lblHeader = new System.Windows.Forms.Label();
            this.cbNewColumn = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblFormula = new System.Windows.Forms.Label();
            this.btnCheckAll = new System.Windows.Forms.Button();
            this.cbHeaders = new CheckComboBoxTest.CheckedComboBox();
            this.btnUncheckAll = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtFormula
            // 
            this.txtFormula.Location = new System.Drawing.Point(27, 95);
            this.txtFormula.Name = "txtFormula";
            this.txtFormula.Size = new System.Drawing.Size(341, 20);
            this.txtFormula.TabIndex = 0;
            this.txtFormula.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtFormula_KeyDown);
            // 
            // txtHeader
            // 
            this.txtHeader.Location = new System.Drawing.Point(119, 157);
            this.txtHeader.Name = "txtHeader";
            this.txtHeader.Size = new System.Drawing.Size(249, 20);
            this.txtHeader.TabIndex = 4;
            this.txtHeader.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtFormula_KeyDown);
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Location = new System.Drawing.Point(24, 160);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(89, 13);
            this.lblHeader.TabIndex = 5;
            this.lblHeader.Text = "Name der Spalte:";
            // 
            // cbNewColumn
            // 
            this.cbNewColumn.AutoSize = true;
            this.cbNewColumn.Location = new System.Drawing.Point(27, 130);
            this.cbNewColumn.Name = "cbNewColumn";
            this.cbNewColumn.Size = new System.Drawing.Size(193, 17);
            this.cbNewColumn.TabIndex = 6;
            this.cbNewColumn.Text = "Ergebnis in neue Spalte schreiben?";
            this.cbNewColumn.UseVisualStyleBackColor = true;
            this.cbNewColumn.CheckedChanged += new System.EventHandler(this.cbNewColumn_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Auswahl der Spalten:";
            // 
            // lblFormula
            // 
            this.lblFormula.AutoSize = true;
            this.lblFormula.Location = new System.Drawing.Point(24, 79);
            this.lblFormula.Name = "lblFormula";
            this.lblFormula.Size = new System.Drawing.Size(42, 13);
            this.lblFormula.TabIndex = 9;
            this.lblFormula.Text = "Format:";
            // 
            // btnCheckAll
            // 
            this.btnCheckAll.Location = new System.Drawing.Point(95, 61);
            this.btnCheckAll.Name = "btnCheckAll";
            this.btnCheckAll.Size = new System.Drawing.Size(101, 23);
            this.btnCheckAll.TabIndex = 10;
            this.btnCheckAll.Text = "Alle auswählen";
            this.btnCheckAll.UseVisualStyleBackColor = true;
            this.btnCheckAll.Click += new System.EventHandler(this.btnCheckAll_Click);
            // 
            // cbHeaders
            // 
            this.cbHeaders.CheckOnClick = true;
            this.cbHeaders.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cbHeaders.DropDownHeight = 1;
            this.cbHeaders.FormattingEnabled = true;
            this.cbHeaders.IntegralHeight = false;
            this.cbHeaders.Location = new System.Drawing.Point(27, 34);
            this.cbHeaders.Name = "cbHeaders";
            this.cbHeaders.Size = new System.Drawing.Size(341, 21);
            this.cbHeaders.TabIndex = 7;
            this.cbHeaders.ValueSeparator = ", ";
            this.cbHeaders.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cbHeaders_ItemCheck);
            this.cbHeaders.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtFormula_KeyDown);
            // 
            // btnUncheckAll
            // 
            this.btnUncheckAll.Location = new System.Drawing.Point(202, 61);
            this.btnUncheckAll.Name = "btnUncheckAll";
            this.btnUncheckAll.Size = new System.Drawing.Size(101, 23);
            this.btnUncheckAll.TabIndex = 11;
            this.btnUncheckAll.Text = "Alle abwählen";
            this.btnUncheckAll.UseVisualStyleBackColor = true;
            this.btnUncheckAll.Click += new System.EventHandler(this.btnUncheckAll_Click);
            // 
            // Formula
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 203);
            this.Controls.Add(this.btnUncheckAll);
            this.Controls.Add(this.btnCheckAll);
            this.Controls.Add(this.lblFormula);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbHeaders);
            this.Controls.Add(this.cbNewColumn);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.txtHeader);
            this.Controls.Add(this.txtFormula);
            this.Name = "Formula";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Formula";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Formula_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtFormula;
        private System.Windows.Forms.TextBox txtHeader;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.CheckBox cbNewColumn;
        private CheckComboBoxTest.CheckedComboBox cbHeaders;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblFormula;
        private System.Windows.Forms.Button btnCheckAll;
        private System.Windows.Forms.Button btnUncheckAll;
    }
}