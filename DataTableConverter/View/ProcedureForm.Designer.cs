namespace DataTableConverter.View
{
    partial class ProcedureForm
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
            this.CbCheckWord = new System.Windows.Forms.CheckBox();
            this.cbCheckTotal = new System.Windows.Forms.CheckBox();
            this.DGVProcedure = new System.Windows.Forms.DataGridView();
            this.CBLeaveEmpty = new System.Windows.Forms.CheckBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.bestätigenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.DGVProcedure)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // CbCheckWord
            // 
            this.CbCheckWord.AutoSize = true;
            this.CbCheckWord.Location = new System.Drawing.Point(188, 28);
            this.CbCheckWord.Name = "CbCheckWord";
            this.CbCheckWord.Size = new System.Drawing.Size(128, 17);
            this.CbCheckWord.TabIndex = 14;
            this.CbCheckWord.Text = "Wortübereinstimmung";
            this.CbCheckWord.UseVisualStyleBackColor = true;
            // 
            // cbCheckTotal
            // 
            this.cbCheckTotal.AutoSize = true;
            this.cbCheckTotal.Location = new System.Drawing.Point(12, 28);
            this.cbCheckTotal.Name = "cbCheckTotal";
            this.cbCheckTotal.Size = new System.Drawing.Size(148, 17);
            this.cbCheckTotal.TabIndex = 13;
            this.cbCheckTotal.Text = "Genaue Übereinstimmung";
            this.cbCheckTotal.UseVisualStyleBackColor = true;
            // 
            // DGVProcedure
            // 
            this.DGVProcedure.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DGVProcedure.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DGVProcedure.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGVProcedure.Location = new System.Drawing.Point(-1, 51);
            this.DGVProcedure.Name = "DGVProcedure";
            this.DGVProcedure.RowHeadersVisible = false;
            this.DGVProcedure.Size = new System.Drawing.Size(536, 366);
            this.DGVProcedure.TabIndex = 12;
            // 
            // CBLeaveEmpty
            // 
            this.CBLeaveEmpty.AutoSize = true;
            this.CBLeaveEmpty.Location = new System.Drawing.Point(332, 28);
            this.CBLeaveEmpty.Name = "CBLeaveEmpty";
            this.CBLeaveEmpty.Size = new System.Drawing.Size(201, 17);
            this.CBLeaveEmpty.TabIndex = 16;
            this.CBLeaveEmpty.Text = "Bei Nichtübereinstimmung leer lassen";
            this.CBLeaveEmpty.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bestätigenToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(536, 24);
            this.menuStrip1.TabIndex = 17;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // bestätigenToolStripMenuItem
            // 
            this.bestätigenToolStripMenuItem.Name = "bestätigenToolStripMenuItem";
            this.bestätigenToolStripMenuItem.Size = new System.Drawing.Size(74, 20);
            this.bestätigenToolStripMenuItem.Text = "Bestätigen";
            this.bestätigenToolStripMenuItem.Click += new System.EventHandler(this.BtnConfirm_Click);
            // 
            // ProcedureForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(536, 417);
            this.Controls.Add(this.CBLeaveEmpty);
            this.Controls.Add(this.CbCheckWord);
            this.Controls.Add(this.cbCheckTotal);
            this.Controls.Add(this.DGVProcedure);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ProcedureForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Suchen & Ersetzen";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProcedureForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.DGVProcedure)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox CbCheckWord;
        private System.Windows.Forms.CheckBox cbCheckTotal;
        private System.Windows.Forms.DataGridView DGVProcedure;
        private System.Windows.Forms.CheckBox CBLeaveEmpty;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem bestätigenToolStripMenuItem;
    }
}