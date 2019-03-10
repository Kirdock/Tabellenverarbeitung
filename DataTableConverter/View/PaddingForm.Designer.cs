namespace DataTableConverter.View
{
    partial class PaddingForm
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
            this.RbRight = new System.Windows.Forms.RadioButton();
            this.RbLeft = new System.Windows.Forms.RadioButton();
            this.label25 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.nbPadCount = new System.Windows.Forms.NumericUpDown();
            this.label23 = new System.Windows.Forms.Label();
            this.TxtCharacter = new System.Windows.Forms.TextBox();
            this.cbPadNewColumn = new System.Windows.Forms.CheckBox();
            this.label19 = new System.Windows.Forms.Label();
            this.dgvPadConditions = new System.Windows.Forms.DataGridView();
            this.lblPadNewColumn = new System.Windows.Forms.Label();
            this.cbHeadersPad = new CheckComboBoxTest.CheckedComboBox();
            this.label21 = new System.Windows.Forms.Label();
            this.txtNewColumnPad = new System.Windows.Forms.TextBox();
            this.BtnConfirm = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nbPadCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPadConditions)).BeginInit();
            this.SuspendLayout();
            // 
            // RbRight
            // 
            this.RbRight.AutoSize = true;
            this.RbRight.Checked = true;
            this.RbRight.Location = new System.Drawing.Point(157, 26);
            this.RbRight.Name = "RbRight";
            this.RbRight.Size = new System.Drawing.Size(59, 17);
            this.RbRight.TabIndex = 35;
            this.RbRight.TabStop = true;
            this.RbRight.Text = "Rechts";
            this.RbRight.UseVisualStyleBackColor = true;
            // 
            // RbLeft
            // 
            this.RbLeft.AutoSize = true;
            this.RbLeft.Location = new System.Drawing.Point(98, 26);
            this.RbLeft.Name = "RbLeft";
            this.RbLeft.Size = new System.Drawing.Size(50, 17);
            this.RbLeft.TabIndex = 34;
            this.RbLeft.Text = "Links";
            this.RbLeft.UseVisualStyleBackColor = true;
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(120, 7);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(53, 13);
            this.label25.TabIndex = 33;
            this.label25.Text = "Richtung:";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(227, 9);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(42, 13);
            this.label24.TabIndex = 32;
            this.label24.Text = "Anzahl:";
            // 
            // nbPadCount
            // 
            this.nbPadCount.Location = new System.Drawing.Point(230, 24);
            this.nbPadCount.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.nbPadCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nbPadCount.Name = "nbPadCount";
            this.nbPadCount.Size = new System.Drawing.Size(54, 20);
            this.nbPadCount.TabIndex = 31;
            this.nbPadCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(12, 9);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(49, 13);
            this.label23.TabIndex = 30;
            this.label23.Text = "Zeichen:";
            // 
            // TxtCharacter
            // 
            this.TxtCharacter.Location = new System.Drawing.Point(13, 24);
            this.TxtCharacter.MaxLength = 1;
            this.TxtCharacter.Name = "TxtCharacter";
            this.TxtCharacter.Size = new System.Drawing.Size(61, 20);
            this.TxtCharacter.TabIndex = 29;
            // 
            // cbPadNewColumn
            // 
            this.cbPadNewColumn.AutoSize = true;
            this.cbPadNewColumn.Location = new System.Drawing.Point(15, 56);
            this.cbPadNewColumn.Name = "cbPadNewColumn";
            this.cbPadNewColumn.Size = new System.Drawing.Size(193, 17);
            this.cbPadNewColumn.TabIndex = 27;
            this.cbPadNewColumn.Text = "Ergebnis in neue Spalte schreiben?";
            this.cbPadNewColumn.UseVisualStyleBackColor = true;
            this.cbPadNewColumn.CheckedChanged += new System.EventHandler(this.cbPadNewColumn_CheckedChanged);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(15, 172);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(73, 13);
            this.label19.TabIndex = 25;
            this.label19.Text = "Bedingungen:";
            // 
            // dgvPadConditions
            // 
            this.dgvPadConditions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvPadConditions.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPadConditions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPadConditions.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvPadConditions.Location = new System.Drawing.Point(14, 188);
            this.dgvPadConditions.Name = "dgvPadConditions";
            this.dgvPadConditions.Size = new System.Drawing.Size(384, 255);
            this.dgvPadConditions.TabIndex = 24;
            // 
            // lblPadNewColumn
            // 
            this.lblPadNewColumn.AutoSize = true;
            this.lblPadNewColumn.Location = new System.Drawing.Point(11, 76);
            this.lblPadNewColumn.Name = "lblPadNewColumn";
            this.lblPadNewColumn.Size = new System.Drawing.Size(122, 13);
            this.lblPadNewColumn.TabIndex = 23;
            this.lblPadNewColumn.Text = "Name der neuen Spalte:";
            // 
            // cbHeadersPad
            // 
            this.cbHeadersPad.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbHeadersPad.CheckOnClick = true;
            this.cbHeadersPad.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cbHeadersPad.DropDownHeight = 1;
            this.cbHeadersPad.FormattingEnabled = true;
            this.cbHeadersPad.IntegralHeight = false;
            this.cbHeadersPad.Location = new System.Drawing.Point(15, 135);
            this.cbHeadersPad.Name = "cbHeadersPad";
            this.cbHeadersPad.Size = new System.Drawing.Size(383, 21);
            this.cbHeadersPad.TabIndex = 22;
            this.cbHeadersPad.ValueSeparator = ", ";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(11, 119);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(121, 13);
            this.label21.TabIndex = 21;
            this.label21.Text = "Angewendeten Spalten:";
            // 
            // txtNewColumnPad
            // 
            this.txtNewColumnPad.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNewColumnPad.Location = new System.Drawing.Point(15, 96);
            this.txtNewColumnPad.Name = "txtNewColumnPad";
            this.txtNewColumnPad.Size = new System.Drawing.Size(383, 20);
            this.txtNewColumnPad.TabIndex = 20;
            // 
            // BtnConfirm
            // 
            this.BtnConfirm.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnConfirm.Location = new System.Drawing.Point(157, 449);
            this.BtnConfirm.Name = "BtnConfirm";
            this.BtnConfirm.Size = new System.Drawing.Size(75, 23);
            this.BtnConfirm.TabIndex = 36;
            this.BtnConfirm.Text = "Bestätigen";
            this.BtnConfirm.UseVisualStyleBackColor = true;
            this.BtnConfirm.Click += new System.EventHandler(this.BtnConfirm_Click);
            // 
            // PaddingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 484);
            this.Controls.Add(this.BtnConfirm);
            this.Controls.Add(this.RbRight);
            this.Controls.Add(this.RbLeft);
            this.Controls.Add(this.label25);
            this.Controls.Add(this.label24);
            this.Controls.Add(this.nbPadCount);
            this.Controls.Add(this.label23);
            this.Controls.Add(this.TxtCharacter);
            this.Controls.Add(this.cbPadNewColumn);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.dgvPadConditions);
            this.Controls.Add(this.lblPadNewColumn);
            this.Controls.Add(this.cbHeadersPad);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.txtNewColumnPad);
            this.Name = "PaddingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Zeichen auffüllen";
            ((System.ComponentModel.ISupportInitialize)(this.nbPadCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPadConditions)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton RbRight;
        private System.Windows.Forms.RadioButton RbLeft;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.NumericUpDown nbPadCount;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.TextBox TxtCharacter;
        private System.Windows.Forms.CheckBox cbPadNewColumn;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.DataGridView dgvPadConditions;
        private System.Windows.Forms.Label lblPadNewColumn;
        private CheckComboBoxTest.CheckedComboBox cbHeadersPad;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox txtNewColumnPad;
        private System.Windows.Forms.Button BtnConfirm;
    }
}