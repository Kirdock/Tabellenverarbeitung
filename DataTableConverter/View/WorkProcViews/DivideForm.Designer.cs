
namespace DataTableConverter.View.WorkProcViews
{
    partial class DivideForm
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
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnUncheckAll = new System.Windows.Forms.Button();
            this.btnCheckAll = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cbHeaders = new CheckComboBoxTest.CheckedComboBox();
            this.cbNewColumn = new System.Windows.Forms.CheckBox();
            this.lblHeader = new System.Windows.Forms.Label();
            this.txtHeader = new System.Windows.Forms.TextBox();
            this.NumDivisor = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxDecimals = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.NumDivisor)).BeginInit();
            this.SuspendLayout();
            // 
            // cbOldColumn
            // 
            this.cbOldColumn.AutoSize = true;
            this.cbOldColumn.Location = new System.Drawing.Point(27, 169);
            this.cbOldColumn.Name = "cbOldColumn";
            this.cbOldColumn.Size = new System.Drawing.Size(165, 17);
            this.cbOldColumn.TabIndex = 22;
            this.cbOldColumn.Text = "Alte Werte in ALT schreiben?";
            this.cbOldColumn.UseVisualStyleBackColor = true;
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(139, 196);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(101, 23);
            this.btnConfirm.TabIndex = 21;
            this.btnConfirm.Text = "Übernehmen";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // btnUncheckAll
            // 
            this.btnUncheckAll.Location = new System.Drawing.Point(202, 104);
            this.btnUncheckAll.Name = "btnUncheckAll";
            this.btnUncheckAll.Size = new System.Drawing.Size(101, 23);
            this.btnUncheckAll.TabIndex = 20;
            this.btnUncheckAll.Text = "Alle abwählen";
            this.btnUncheckAll.UseVisualStyleBackColor = true;
            this.btnUncheckAll.Click += new System.EventHandler(this.btnUncheckAll_Click);
            // 
            // btnCheckAll
            // 
            this.btnCheckAll.Location = new System.Drawing.Point(95, 104);
            this.btnCheckAll.Name = "btnCheckAll";
            this.btnCheckAll.Size = new System.Drawing.Size(101, 23);
            this.btnCheckAll.TabIndex = 19;
            this.btnCheckAll.Text = "Alle auswählen";
            this.btnCheckAll.UseVisualStyleBackColor = true;
            this.btnCheckAll.Click += new System.EventHandler(this.btnCheckAll_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Auswahl der Spalten:";
            // 
            // cbHeaders
            // 
            this.cbHeaders.CheckOnClick = true;
            this.cbHeaders.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cbHeaders.DropDownHeight = 1;
            this.cbHeaders.FormattingEnabled = true;
            this.cbHeaders.IntegralHeight = false;
            this.cbHeaders.Location = new System.Drawing.Point(27, 77);
            this.cbHeaders.Name = "cbHeaders";
            this.cbHeaders.Size = new System.Drawing.Size(341, 21);
            this.cbHeaders.TabIndex = 17;
            this.cbHeaders.ValueSeparator = ", ";
            // 
            // cbNewColumn
            // 
            this.cbNewColumn.AutoSize = true;
            this.cbNewColumn.Location = new System.Drawing.Point(27, 143);
            this.cbNewColumn.Name = "cbNewColumn";
            this.cbNewColumn.Size = new System.Drawing.Size(193, 17);
            this.cbNewColumn.TabIndex = 16;
            this.cbNewColumn.Text = "Ergebnis in neue Spalte schreiben?";
            this.cbNewColumn.UseVisualStyleBackColor = true;
            this.cbNewColumn.CheckedChanged += new System.EventHandler(this.cbNewColumn_CheckedChanged);
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Location = new System.Drawing.Point(24, 173);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(89, 13);
            this.lblHeader.TabIndex = 15;
            this.lblHeader.Text = "Name der Spalte:";
            // 
            // txtHeader
            // 
            this.txtHeader.Location = new System.Drawing.Point(119, 170);
            this.txtHeader.Name = "txtHeader";
            this.txtHeader.Size = new System.Drawing.Size(249, 20);
            this.txtHeader.TabIndex = 14;
            // 
            // NumDivisor
            // 
            this.NumDivisor.Location = new System.Drawing.Point(27, 25);
            this.NumDivisor.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.NumDivisor.Name = "NumDivisor";
            this.NumDivisor.Size = new System.Drawing.Size(120, 20);
            this.NumDivisor.TabIndex = 23;
            this.NumDivisor.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 24;
            this.label2.Text = "Divisor";
            // 
            // checkBoxDecimals
            // 
            this.checkBoxDecimals.AutoSize = true;
            this.checkBoxDecimals.Checked = true;
            this.checkBoxDecimals.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDecimals.Location = new System.Drawing.Point(160, 26);
            this.checkBoxDecimals.Name = "checkBoxDecimals";
            this.checkBoxDecimals.Size = new System.Drawing.Size(194, 17);
            this.checkBoxDecimals.TabIndex = 25;
            this.checkBoxDecimals.Text = "Immer zwei Dezimalstellen anzeigen";
            this.checkBoxDecimals.UseVisualStyleBackColor = true;
            // 
            // DivideForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 238);
            this.Controls.Add(this.checkBoxDecimals);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.NumDivisor);
            this.Controls.Add(this.cbOldColumn);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.btnUncheckAll);
            this.Controls.Add(this.btnCheckAll);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbHeaders);
            this.Controls.Add(this.cbNewColumn);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.txtHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "DivideForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dividieren";
            ((System.ComponentModel.ISupportInitialize)(this.NumDivisor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbOldColumn;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnUncheckAll;
        private System.Windows.Forms.Button btnCheckAll;
        private System.Windows.Forms.Label label1;
        private CheckComboBoxTest.CheckedComboBox cbHeaders;
        private System.Windows.Forms.CheckBox cbNewColumn;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.TextBox txtHeader;
        private System.Windows.Forms.NumericUpDown NumDivisor;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBoxDecimals;
    }
}