
namespace DataTableConverter.View.WorkProcViews
{
    partial class PVMExport
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
            this.btnUncheckAll = new System.Windows.Forms.Button();
            this.btnCheckAll = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.RBExcel = new System.Windows.Forms.RadioButton();
            this.RBCSV = new System.Windows.Forms.RadioButton();
            this.RBDBASE = new System.Windows.Forms.RadioButton();
            this.cbHeaders = new CheckComboBoxTest.CheckedComboBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnUncheckAll
            // 
            this.btnUncheckAll.Location = new System.Drawing.Point(200, 61);
            this.btnUncheckAll.Name = "btnUncheckAll";
            this.btnUncheckAll.Size = new System.Drawing.Size(101, 23);
            this.btnUncheckAll.TabIndex = 15;
            this.btnUncheckAll.Text = "Alle abwählen";
            this.btnUncheckAll.UseVisualStyleBackColor = true;
            this.btnUncheckAll.Click += new System.EventHandler(this.btnUncheckAll_Click);
            // 
            // btnCheckAll
            // 
            this.btnCheckAll.Location = new System.Drawing.Point(93, 61);
            this.btnCheckAll.Name = "btnCheckAll";
            this.btnCheckAll.Size = new System.Drawing.Size(101, 23);
            this.btnCheckAll.TabIndex = 14;
            this.btnCheckAll.Text = "Alle auswählen";
            this.btnCheckAll.UseVisualStyleBackColor = true;
            this.btnCheckAll.Click += new System.EventHandler(this.btnCheckAll_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Auswahl der Spalten:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.RBExcel);
            this.groupBox1.Controls.Add(this.RBCSV);
            this.groupBox1.Controls.Add(this.RBDBASE);
            this.groupBox1.Location = new System.Drawing.Point(25, 103);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(341, 74);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Speichern als";
            // 
            // RBExcel
            // 
            this.RBExcel.AutoSize = true;
            this.RBExcel.Location = new System.Drawing.Point(211, 34);
            this.RBExcel.Name = "RBExcel";
            this.RBExcel.Size = new System.Drawing.Size(51, 17);
            this.RBExcel.TabIndex = 2;
            this.RBExcel.Text = "Excel";
            this.RBExcel.UseVisualStyleBackColor = true;
            // 
            // RBCSV
            // 
            this.RBCSV.AutoSize = true;
            this.RBCSV.Checked = true;
            this.RBCSV.Location = new System.Drawing.Point(159, 34);
            this.RBCSV.Name = "RBCSV";
            this.RBCSV.Size = new System.Drawing.Size(46, 17);
            this.RBCSV.TabIndex = 1;
            this.RBCSV.TabStop = true;
            this.RBCSV.Text = "CSV";
            this.RBCSV.UseVisualStyleBackColor = true;
            // 
            // RBDBASE
            // 
            this.RBDBASE.AutoSize = true;
            this.RBDBASE.Location = new System.Drawing.Point(92, 34);
            this.RBDBASE.Name = "RBDBASE";
            this.RBDBASE.Size = new System.Drawing.Size(61, 17);
            this.RBDBASE.TabIndex = 0;
            this.RBDBASE.Text = "DBASE";
            this.RBDBASE.UseVisualStyleBackColor = true;
            // 
            // cbHeaders
            // 
            this.cbHeaders.CheckOnClick = true;
            this.cbHeaders.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cbHeaders.DropDownHeight = 1;
            this.cbHeaders.FormattingEnabled = true;
            this.cbHeaders.IntegralHeight = false;
            this.cbHeaders.Location = new System.Drawing.Point(25, 34);
            this.cbHeaders.Name = "cbHeaders";
            this.cbHeaders.Size = new System.Drawing.Size(341, 21);
            this.cbHeaders.TabIndex = 12;
            this.cbHeaders.ValueSeparator = ", ";
            // 
            // PVMExport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 189);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnUncheckAll);
            this.Controls.Add(this.btnCheckAll);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbHeaders);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "PVMExport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PVMExport";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PVMExport_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnUncheckAll;
        private System.Windows.Forms.Button btnCheckAll;
        private System.Windows.Forms.Label label1;
        private CheckComboBoxTest.CheckedComboBox cbHeaders;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton RBExcel;
        private System.Windows.Forms.RadioButton RBCSV;
        private System.Windows.Forms.RadioButton RBDBASE;
    }
}