namespace DataTableConverter.View
{
    partial class Merge
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
            this.label1 = new System.Windows.Forms.Label();
            this.lblFormula = new System.Windows.Forms.Label();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.cbHeaders = new CheckComboBoxTest.CheckedComboBox();
            this.label18 = new System.Windows.Forms.Label();
            this.dgvMerge = new System.Windows.Forms.DataGridView();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.cbMergeOldColumn = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMerge)).BeginInit();
            this.SuspendLayout();
            // 
            // txtFormula
            // 
            this.txtFormula.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFormula.Location = new System.Drawing.Point(27, 141);
            this.txtFormula.Name = "txtFormula";
            this.txtFormula.Size = new System.Drawing.Size(341, 20);
            this.txtFormula.TabIndex = 2;
            // 
            // txtHeader
            // 
            this.txtHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHeader.Location = new System.Drawing.Point(27, 76);
            this.txtHeader.Name = "txtHeader";
            this.txtHeader.Size = new System.Drawing.Size(343, 20);
            this.txtHeader.TabIndex = 1;
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Location = new System.Drawing.Point(24, 60);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(122, 13);
            this.lblHeader.TabIndex = 5;
            this.lblHeader.Text = "Name der neuen Spalte:";
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
            this.lblFormula.Location = new System.Drawing.Point(24, 125);
            this.lblFormula.Name = "lblFormula";
            this.lblFormula.Size = new System.Drawing.Size(88, 13);
            this.lblFormula.TabIndex = 9;
            this.lblFormula.Text = "Standard-Format:";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConfirm.Location = new System.Drawing.Point(151, 417);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(101, 23);
            this.btnConfirm.TabIndex = 4;
            this.btnConfirm.Text = "Übernehmen";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // cbHeaders
            // 
            this.cbHeaders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbHeaders.CheckOnClick = true;
            this.cbHeaders.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cbHeaders.DropDownHeight = 1;
            this.cbHeaders.FormattingEnabled = true;
            this.cbHeaders.IntegralHeight = false;
            this.cbHeaders.Location = new System.Drawing.Point(27, 34);
            this.cbHeaders.Name = "cbHeaders";
            this.cbHeaders.Size = new System.Drawing.Size(341, 21);
            this.cbHeaders.TabIndex = 0;
            this.cbHeaders.ValueSeparator = ", ";
            this.cbHeaders.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cbHeaders_ItemCheck);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(24, 176);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(73, 13);
            this.label18.TabIndex = 14;
            this.label18.Text = "Bedingungen:";
            // 
            // dgvMerge
            // 
            this.dgvMerge.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvMerge.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMerge.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMerge.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvMerge.Location = new System.Drawing.Point(27, 192);
            this.dgvMerge.Name = "dgvMerge";
            this.dgvMerge.Size = new System.Drawing.Size(341, 219);
            this.dgvMerge.TabIndex = 3;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(118, 125);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(28, 13);
            this.linkLabel1.TabIndex = 15;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Hilfe";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // cbMergeOldColumn
            // 
            this.cbMergeOldColumn.AutoSize = true;
            this.cbMergeOldColumn.Location = new System.Drawing.Point(27, 102);
            this.cbMergeOldColumn.Name = "cbMergeOldColumn";
            this.cbMergeOldColumn.Size = new System.Drawing.Size(165, 17);
            this.cbMergeOldColumn.TabIndex = 16;
            this.cbMergeOldColumn.Text = "Alte Werte in ALT speichern?";
            this.cbMergeOldColumn.UseVisualStyleBackColor = true;
            // 
            // Merge
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 452);
            this.Controls.Add(this.cbMergeOldColumn);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.dgvMerge);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.lblFormula);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbHeaders);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.txtHeader);
            this.Controls.Add(this.txtFormula);
            this.Name = "Merge";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Formula";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Merge_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMerge)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtFormula;
        private System.Windows.Forms.TextBox txtHeader;
        private System.Windows.Forms.Label lblHeader;
        private CheckComboBoxTest.CheckedComboBox cbHeaders;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblFormula;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.DataGridView dgvMerge;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.CheckBox cbMergeOldColumn;
    }
}