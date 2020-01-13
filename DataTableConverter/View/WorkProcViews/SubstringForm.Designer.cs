namespace DataTableConverter.View.WorkProcViews
{
    partial class SubstringForm
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
            this.components = new System.ComponentModel.Container();
            this.cbOldColumn = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbNewColumn = new System.Windows.Forms.CheckBox();
            this.lblNewColumn = new System.Windows.Forms.Label();
            this.txtNewColumn = new System.Windows.Forms.TextBox();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.nbEnd = new System.Windows.Forms.NumericUpDown();
            this.label30 = new System.Windows.Forms.Label();
            this.nbStart = new System.Windows.Forms.NumericUpDown();
            this.label29 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.cbHeaders = new CheckComboBoxTest.CheckedComboBox();
            this.cbSubstringText = new System.Windows.Forms.CheckBox();
            this.txtSubstringText = new System.Windows.Forms.TextBox();
            this.CBReverse = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.nbEnd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nbStart)).BeginInit();
            this.SuspendLayout();
            // 
            // cbOldColumn
            // 
            this.cbOldColumn.AutoSize = true;
            this.cbOldColumn.Location = new System.Drawing.Point(15, 189);
            this.cbOldColumn.Name = "cbOldColumn";
            this.cbOldColumn.Size = new System.Drawing.Size(165, 17);
            this.cbOldColumn.TabIndex = 32;
            this.cbOldColumn.Text = "Alte Werte in ALT speichern?";
            this.cbOldColumn.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 105);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 13);
            this.label2.TabIndex = 29;
            this.label2.Text = "Auswahl der Spalten:";
            // 
            // cbNewColumn
            // 
            this.cbNewColumn.AutoSize = true;
            this.cbNewColumn.Location = new System.Drawing.Point(15, 160);
            this.cbNewColumn.Name = "cbNewColumn";
            this.cbNewColumn.Size = new System.Drawing.Size(193, 17);
            this.cbNewColumn.TabIndex = 27;
            this.cbNewColumn.Text = "Ergebnis in neue Spalte schreiben?";
            this.cbNewColumn.UseVisualStyleBackColor = true;
            this.cbNewColumn.CheckedChanged += new System.EventHandler(this.cbNewColumn_CheckedChanged);
            // 
            // lblNewColumn
            // 
            this.lblNewColumn.AutoSize = true;
            this.lblNewColumn.Location = new System.Drawing.Point(12, 190);
            this.lblNewColumn.Name = "lblNewColumn";
            this.lblNewColumn.Size = new System.Drawing.Size(89, 13);
            this.lblNewColumn.TabIndex = 26;
            this.lblNewColumn.Text = "Name der Spalte:";
            // 
            // txtNewColumn
            // 
            this.txtNewColumn.Location = new System.Drawing.Point(107, 187);
            this.txtNewColumn.Name = "txtNewColumn";
            this.txtNewColumn.Size = new System.Drawing.Size(249, 20);
            this.txtNewColumn.TabIndex = 25;
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(107, 225);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(150, 23);
            this.btnConfirm.TabIndex = 24;
            this.btnConfirm.Text = "Übernehmen";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // nbEnd
            // 
            this.nbEnd.Location = new System.Drawing.Point(133, 22);
            this.nbEnd.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nbEnd.Name = "nbEnd";
            this.nbEnd.Size = new System.Drawing.Size(70, 20);
            this.nbEnd.TabIndex = 36;
            this.nbEnd.ValueChanged += new System.EventHandler(this.nbEnd_ValueChanged);
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(134, 8);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(65, 13);
            this.label30.TabIndex = 35;
            this.label30.Text = "Endposition:";
            this.toolTip1.SetToolTip(this.label30, "\"0\" = Rest nach Start");
            // 
            // nbStart
            // 
            this.nbStart.Location = new System.Drawing.Point(15, 22);
            this.nbStart.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nbStart.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nbStart.Name = "nbStart";
            this.nbStart.Size = new System.Drawing.Size(70, 20);
            this.nbStart.TabIndex = 34;
            this.nbStart.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nbStart.ValueChanged += new System.EventHandler(this.nbStart_ValueChanged);
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(12, 8);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(68, 13);
            this.label29.TabIndex = 33;
            this.label29.Text = "Startposition:";
            // 
            // toolTip1
            // 
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // cbHeaders
            // 
            this.cbHeaders.CheckOnClick = true;
            this.cbHeaders.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cbHeaders.DropDownHeight = 1;
            this.cbHeaders.FormattingEnabled = true;
            this.cbHeaders.IntegralHeight = false;
            this.cbHeaders.Location = new System.Drawing.Point(15, 121);
            this.cbHeaders.Name = "cbHeaders";
            this.cbHeaders.Size = new System.Drawing.Size(341, 21);
            this.cbHeaders.TabIndex = 28;
            this.cbHeaders.ValueSeparator = ", ";
            // 
            // cbSubstringText
            // 
            this.cbSubstringText.AutoSize = true;
            this.cbSubstringText.Location = new System.Drawing.Point(15, 59);
            this.cbSubstringText.Name = "cbSubstringText";
            this.cbSubstringText.Size = new System.Drawing.Size(229, 17);
            this.cbSubstringText.TabIndex = 37;
            this.cbSubstringText.Text = "Ausgewählten Bereich durch Text ersetzen";
            this.cbSubstringText.UseVisualStyleBackColor = true;
            this.cbSubstringText.CheckedChanged += new System.EventHandler(this.cbSubstringText_CheckedChanged);
            // 
            // txtSubstringText
            // 
            this.txtSubstringText.Location = new System.Drawing.Point(15, 82);
            this.txtSubstringText.Name = "txtSubstringText";
            this.txtSubstringText.Size = new System.Drawing.Size(341, 20);
            this.txtSubstringText.TabIndex = 38;
            this.txtSubstringText.Visible = false;
            // 
            // CBReverse
            // 
            this.CBReverse.AutoSize = true;
            this.CBReverse.Location = new System.Drawing.Point(222, 23);
            this.CBReverse.Name = "CBReverse";
            this.CBReverse.Size = new System.Drawing.Size(134, 17);
            this.CBReverse.TabIndex = 39;
            this.CBReverse.Text = "Von hinten nach vorne";
            this.CBReverse.UseVisualStyleBackColor = true;
            // 
            // SubstringForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(372, 260);
            this.Controls.Add(this.CBReverse);
            this.Controls.Add(this.txtSubstringText);
            this.Controls.Add(this.cbSubstringText);
            this.Controls.Add(this.nbEnd);
            this.Controls.Add(this.label30);
            this.Controls.Add(this.nbStart);
            this.Controls.Add(this.label29);
            this.Controls.Add(this.cbOldColumn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbHeaders);
            this.Controls.Add(this.cbNewColumn);
            this.Controls.Add(this.lblNewColumn);
            this.Controls.Add(this.txtNewColumn);
            this.Controls.Add(this.btnConfirm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "SubstringForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Substring";
            ((System.ComponentModel.ISupportInitialize)(this.nbEnd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nbStart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbOldColumn;
        private System.Windows.Forms.Label label2;
        private CheckComboBoxTest.CheckedComboBox cbHeaders;
        private System.Windows.Forms.CheckBox cbNewColumn;
        private System.Windows.Forms.Label lblNewColumn;
        private System.Windows.Forms.TextBox txtNewColumn;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.NumericUpDown nbEnd;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.NumericUpDown nbStart;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox cbSubstringText;
        private System.Windows.Forms.TextBox txtSubstringText;
        private System.Windows.Forms.CheckBox CBReverse;
    }
}