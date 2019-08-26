namespace DataTableConverter.View
{
    partial class MergeFormatView
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
            this.dgTable = new System.Windows.Forms.DataGridView();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.RBSimple = new System.Windows.Forms.RadioButton();
            this.RBExtended = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.PanelExtended = new System.Windows.Forms.Panel();
            this.PanelSimple = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFormula = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgTable)).BeginInit();
            this.panel1.SuspendLayout();
            this.PanelExtended.SuspendLayout();
            this.PanelSimple.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgTable
            // 
            this.dgTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgTable.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgTable.Location = new System.Drawing.Point(0, 0);
            this.dgTable.Name = "dgTable";
            this.dgTable.Size = new System.Drawing.Size(716, 371);
            this.dgTable.TabIndex = 0;
            this.dgTable.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgTable_CellClick);
            this.dgTable.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgTable_CellFormatting);
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(12, 35);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(75, 23);
            this.btnConfirm.TabIndex = 1;
            this.btnConfirm.Text = "Bestätigen";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(12, 9);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(28, 13);
            this.linkLabel1.TabIndex = 11;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Hilfe";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // RBSimple
            // 
            this.RBSimple.AutoSize = true;
            this.RBSimple.Location = new System.Drawing.Point(201, 41);
            this.RBSimple.Name = "RBSimple";
            this.RBSimple.Size = new System.Drawing.Size(105, 17);
            this.RBSimple.TabIndex = 12;
            this.RBSimple.TabStop = true;
            this.RBSimple.Text = "Einfache Ansicht";
            this.RBSimple.UseVisualStyleBackColor = true;
            this.RBSimple.CheckedChanged += new System.EventHandler(this.RB_CheckedChanged);
            // 
            // RBExtended
            // 
            this.RBExtended.AutoSize = true;
            this.RBExtended.Location = new System.Drawing.Point(363, 41);
            this.RBExtended.Name = "RBExtended";
            this.RBExtended.Size = new System.Drawing.Size(110, 17);
            this.RBExtended.TabIndex = 13;
            this.RBExtended.TabStop = true;
            this.RBExtended.Text = "Erweiterte Ansicht";
            this.RBExtended.UseVisualStyleBackColor = true;
            this.RBExtended.CheckedChanged += new System.EventHandler(this.RB_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.PanelSimple);
            this.panel1.Controls.Add(this.PanelExtended);
            this.panel1.Location = new System.Drawing.Point(15, 95);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(716, 371);
            this.panel1.TabIndex = 14;
            // 
            // PanelExtended
            // 
            this.PanelExtended.Controls.Add(this.dgTable);
            this.PanelExtended.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelExtended.Location = new System.Drawing.Point(0, 0);
            this.PanelExtended.Name = "PanelExtended";
            this.PanelExtended.Size = new System.Drawing.Size(716, 371);
            this.PanelExtended.TabIndex = 0;
            // 
            // PanelSimple
            // 
            this.PanelSimple.Controls.Add(this.txtFormula);
            this.PanelSimple.Controls.Add(this.label1);
            this.PanelSimple.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelSimple.Location = new System.Drawing.Point(0, 0);
            this.PanelSimple.Name = "PanelSimple";
            this.PanelSimple.Size = new System.Drawing.Size(716, 371);
            this.PanelSimple.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Format:";
            // 
            // txtFormula
            // 
            this.txtFormula.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFormula.Location = new System.Drawing.Point(17, 47);
            this.txtFormula.Name = "txtFormula";
            this.txtFormula.Size = new System.Drawing.Size(684, 20);
            this.txtFormula.TabIndex = 1;
            // 
            // MergeFormatView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(749, 478);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.RBExtended);
            this.Controls.Add(this.RBSimple);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.btnConfirm);
            this.Name = "MergeFormatView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Format";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MergeFormatView_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dgTable)).EndInit();
            this.panel1.ResumeLayout(false);
            this.PanelExtended.ResumeLayout(false);
            this.PanelSimple.ResumeLayout(false);
            this.PanelSimple.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgTable;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.RadioButton RBSimple;
        private System.Windows.Forms.RadioButton RBExtended;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel PanelSimple;
        private System.Windows.Forms.TextBox txtFormula;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel PanelExtended;
    }
}