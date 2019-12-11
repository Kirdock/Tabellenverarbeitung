namespace DataTableConverter.View.WorkProcViews
{
    partial class SearchForm
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
            this.label36 = new System.Windows.Forms.Label();
            this.TxtSearchText = new System.Windows.Forms.TextBox();
            this.label34 = new System.Windows.Forms.Label();
            this.NbSearchTo = new System.Windows.Forms.NumericUpDown();
            this.label31 = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.NbSearchFrom = new System.Windows.Forms.NumericUpDown();
            this.TxtSearchNewColumn = new System.Windows.Forms.TextBox();
            this.label33 = new System.Windows.Forms.Label();
            this.CmBHeader = new System.Windows.Forms.ComboBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.bestätigenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CBTotal = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.NbSearchTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NbSearchFrom)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(5, 112);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(166, 13);
            this.label36.TabIndex = 36;
            this.label36.Text = "Spalte in der gesucht werden soll:";
            // 
            // TxtSearchText
            // 
            this.TxtSearchText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtSearchText.Location = new System.Drawing.Point(8, 59);
            this.TxtSearchText.Name = "TxtSearchText";
            this.TxtSearchText.Size = new System.Drawing.Size(231, 20);
            this.TxtSearchText.TabIndex = 1;
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(9, 40);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(52, 13);
            this.label34.TabIndex = 34;
            this.label34.Text = "Suchtext:";
            // 
            // NbSearchTo
            // 
            this.NbSearchTo.Location = new System.Drawing.Point(169, 231);
            this.NbSearchTo.Maximum = new decimal(new int[] {
            1410065408,
            2,
            0,
            0});
            this.NbSearchTo.Minimum = new decimal(new int[] {
            1410065408,
            2,
            0,
            -2147483648});
            this.NbSearchTo.Name = "NbSearchTo";
            this.NbSearchTo.Size = new System.Drawing.Size(70, 20);
            this.NbSearchTo.TabIndex = 5;
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(167, 209);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(66, 13);
            this.label31.TabIndex = 32;
            this.label31.Text = "Endnummer:";
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(3, 209);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(69, 13);
            this.label32.TabIndex = 31;
            this.label32.Text = "Startnummer:";
            // 
            // NbSearchFrom
            // 
            this.NbSearchFrom.Location = new System.Drawing.Point(8, 231);
            this.NbSearchFrom.Maximum = new decimal(new int[] {
            1410065408,
            2,
            0,
            0});
            this.NbSearchFrom.Minimum = new decimal(new int[] {
            1410065408,
            2,
            0,
            -2147483648});
            this.NbSearchFrom.Name = "NbSearchFrom";
            this.NbSearchFrom.Size = new System.Drawing.Size(70, 20);
            this.NbSearchFrom.TabIndex = 4;
            // 
            // TxtSearchNewColumn
            // 
            this.TxtSearchNewColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtSearchNewColumn.Location = new System.Drawing.Point(8, 178);
            this.TxtSearchNewColumn.Name = "TxtSearchNewColumn";
            this.TxtSearchNewColumn.Size = new System.Drawing.Size(231, 20);
            this.TxtSearchNewColumn.TabIndex = 3;
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(3, 162);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(122, 13);
            this.label33.TabIndex = 28;
            this.label33.Text = "Name der neuen Spalte:";
            // 
            // CmBHeader
            // 
            this.CmBHeader.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmBHeader.FormattingEnabled = true;
            this.CmBHeader.Location = new System.Drawing.Point(8, 138);
            this.CmBHeader.Name = "CmBHeader";
            this.CmBHeader.Size = new System.Drawing.Size(231, 21);
            this.CmBHeader.TabIndex = 2;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bestätigenToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(245, 24);
            this.menuStrip1.TabIndex = 38;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // bestätigenToolStripMenuItem
            // 
            this.bestätigenToolStripMenuItem.Name = "bestätigenToolStripMenuItem";
            this.bestätigenToolStripMenuItem.Size = new System.Drawing.Size(74, 20);
            this.bestätigenToolStripMenuItem.Text = "Bestätigen";
            this.bestätigenToolStripMenuItem.Click += new System.EventHandler(this.bestätigenToolStripMenuItem_Click);
            // 
            // CBTotal
            // 
            this.CBTotal.AutoSize = true;
            this.CBTotal.Location = new System.Drawing.Point(8, 85);
            this.CBTotal.Name = "CBTotal";
            this.CBTotal.Size = new System.Drawing.Size(148, 17);
            this.CBTotal.TabIndex = 39;
            this.CBTotal.Text = "Genaue Übereinstimmung";
            this.CBTotal.UseVisualStyleBackColor = true;
            // 
            // SearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(245, 275);
            this.Controls.Add(this.CBTotal);
            this.Controls.Add(this.CmBHeader);
            this.Controls.Add(this.label36);
            this.Controls.Add(this.TxtSearchText);
            this.Controls.Add(this.label34);
            this.Controls.Add(this.NbSearchTo);
            this.Controls.Add(this.label31);
            this.Controls.Add(this.label32);
            this.Controls.Add(this.NbSearchFrom);
            this.Controls.Add(this.TxtSearchNewColumn);
            this.Controls.Add(this.label33);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "SearchForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Suchen";
            ((System.ComponentModel.ISupportInitialize)(this.NbSearchTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NbSearchFrom)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.TextBox TxtSearchText;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.NumericUpDown NbSearchTo;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.NumericUpDown NbSearchFrom;
        private System.Windows.Forms.TextBox TxtSearchNewColumn;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.ComboBox CmBHeader;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem bestätigenToolStripMenuItem;
        private System.Windows.Forms.CheckBox CBTotal;
    }
}