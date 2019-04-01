namespace DataTableConverter.View
{
    partial class RoundForm
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
            this.numDec = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnUncheckAll = new System.Windows.Forms.Button();
            this.btnCheckAll = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cbHeaders = new CheckComboBoxTest.CheckedComboBox();
            this.cbNewColumn = new System.Windows.Forms.CheckBox();
            this.lblHeader = new System.Windows.Forms.Label();
            this.txtHeader = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.CmBRound = new System.Windows.Forms.ComboBox();
            this.cbOldColumn = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numDec)).BeginInit();
            this.SuspendLayout();
            // 
            // numDec
            // 
            this.numDec.Location = new System.Drawing.Point(12, 30);
            this.numDec.Name = "numDec";
            this.numDec.Size = new System.Drawing.Size(107, 20);
            this.numDec.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Dezimalstellen:";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(107, 205);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(150, 23);
            this.btnConfirm.TabIndex = 2;
            this.btnConfirm.Text = "Übernehmen";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // btnUncheckAll
            // 
            this.btnUncheckAll.Location = new System.Drawing.Point(190, 111);
            this.btnUncheckAll.Name = "btnUncheckAll";
            this.btnUncheckAll.Size = new System.Drawing.Size(101, 23);
            this.btnUncheckAll.TabIndex = 20;
            this.btnUncheckAll.Text = "Alle abwählen";
            this.btnUncheckAll.UseVisualStyleBackColor = true;
            this.btnUncheckAll.Click += new System.EventHandler(this.btnUncheckAll_Click);
            // 
            // btnCheckAll
            // 
            this.btnCheckAll.Location = new System.Drawing.Point(83, 111);
            this.btnCheckAll.Name = "btnCheckAll";
            this.btnCheckAll.Size = new System.Drawing.Size(101, 23);
            this.btnCheckAll.TabIndex = 19;
            this.btnCheckAll.Text = "Alle auswählen";
            this.btnCheckAll.UseVisualStyleBackColor = true;
            this.btnCheckAll.Click += new System.EventHandler(this.btnCheckAll_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Auswahl der Spalten:";
            // 
            // cbHeaders
            // 
            this.cbHeaders.CheckOnClick = true;
            this.cbHeaders.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cbHeaders.DropDownHeight = 1;
            this.cbHeaders.FormattingEnabled = true;
            this.cbHeaders.IntegralHeight = false;
            this.cbHeaders.Location = new System.Drawing.Point(15, 84);
            this.cbHeaders.Name = "cbHeaders";
            this.cbHeaders.Size = new System.Drawing.Size(341, 21);
            this.cbHeaders.TabIndex = 16;
            this.cbHeaders.ValueSeparator = ", ";
            // 
            // cbNewColumn
            // 
            this.cbNewColumn.AutoSize = true;
            this.cbNewColumn.Location = new System.Drawing.Point(15, 140);
            this.cbNewColumn.Name = "cbNewColumn";
            this.cbNewColumn.Size = new System.Drawing.Size(193, 17);
            this.cbNewColumn.TabIndex = 15;
            this.cbNewColumn.Text = "Ergebnis in neue Spalte schreiben?";
            this.cbNewColumn.UseVisualStyleBackColor = true;
            this.cbNewColumn.CheckedChanged += new System.EventHandler(this.cbNewColumn_CheckedChanged);
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Location = new System.Drawing.Point(12, 170);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(89, 13);
            this.lblHeader.TabIndex = 14;
            this.lblHeader.Text = "Name der Spalte:";
            // 
            // txtHeader
            // 
            this.txtHeader.Location = new System.Drawing.Point(107, 167);
            this.txtHeader.Name = "txtHeader";
            this.txtHeader.Size = new System.Drawing.Size(249, 20);
            this.txtHeader.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(187, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(23, 13);
            this.label3.TabIndex = 21;
            this.label3.Text = "Art:";
            // 
            // CmBRound
            // 
            this.CmBRound.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmBRound.FormattingEnabled = true;
            this.CmBRound.Items.AddRange(new object[] {
            "Runden",
            "Aufrunden",
            "Abrunden"});
            this.CmBRound.Location = new System.Drawing.Point(190, 29);
            this.CmBRound.Name = "CmBRound";
            this.CmBRound.Size = new System.Drawing.Size(166, 21);
            this.CmBRound.TabIndex = 22;
            // 
            // cbOldColumn
            // 
            this.cbOldColumn.AutoSize = true;
            this.cbOldColumn.Location = new System.Drawing.Point(15, 169);
            this.cbOldColumn.Name = "cbOldColumn";
            this.cbOldColumn.Size = new System.Drawing.Size(165, 17);
            this.cbOldColumn.TabIndex = 23;
            this.cbOldColumn.Text = "Alte Werte in ALT speichern?";
            this.cbOldColumn.UseVisualStyleBackColor = true;
            // 
            // RoundForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 245);
            this.Controls.Add(this.cbOldColumn);
            this.Controls.Add(this.CmBRound);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnUncheckAll);
            this.Controls.Add(this.btnCheckAll);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbHeaders);
            this.Controls.Add(this.cbNewColumn);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.txtHeader);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numDec);
            this.Name = "RoundForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Runden";
            ((System.ComponentModel.ISupportInitialize)(this.numDec)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown numDec;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnUncheckAll;
        private System.Windows.Forms.Button btnCheckAll;
        private System.Windows.Forms.Label label2;
        private CheckComboBoxTest.CheckedComboBox cbHeaders;
        private System.Windows.Forms.CheckBox cbNewColumn;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.TextBox txtHeader;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox CmBRound;
        private System.Windows.Forms.CheckBox cbOldColumn;
    }
}