namespace DataTableConverter.View
{
    partial class ClipboardForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.bestätigenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spalteAufteilenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dgTable = new System.Windows.Forms.DataGridView();
            this.ctxRow = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.zeileLöschenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zeileHinzufügenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spalteLöschenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spalteHinzufügenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgTable)).BeginInit();
            this.ctxRow.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bestätigenToolStripMenuItem,
            this.spalteAufteilenToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // bestätigenToolStripMenuItem
            // 
            this.bestätigenToolStripMenuItem.Name = "bestätigenToolStripMenuItem";
            this.bestätigenToolStripMenuItem.Size = new System.Drawing.Size(74, 20);
            this.bestätigenToolStripMenuItem.Text = "Bestätigen";
            this.bestätigenToolStripMenuItem.Click += new System.EventHandler(this.bestätigenToolStripMenuItem_Click);
            // 
            // spalteAufteilenToolStripMenuItem
            // 
            this.spalteAufteilenToolStripMenuItem.Name = "spalteAufteilenToolStripMenuItem";
            this.spalteAufteilenToolStripMenuItem.Size = new System.Drawing.Size(100, 20);
            this.spalteAufteilenToolStripMenuItem.Text = "Spalte aufteilen";
            this.spalteAufteilenToolStripMenuItem.Click += new System.EventHandler(this.spalteAufteilenToolStripMenuItem_Click);
            // 
            // dgTable
            // 
            this.dgTable.AllowUserToOrderColumns = true;
            this.dgTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgTable.Location = new System.Drawing.Point(0, 24);
            this.dgTable.Name = "dgTable";
            this.dgTable.Size = new System.Drawing.Size(800, 426);
            this.dgTable.TabIndex = 1;
            this.dgTable.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dgTable_MouseClick);
            // 
            // ctxRow
            // 
            this.ctxRow.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zeileLöschenToolStripMenuItem,
            this.zeileHinzufügenToolStripMenuItem,
            this.spalteLöschenToolStripMenuItem,
            this.spalteHinzufügenToolStripMenuItem});
            this.ctxRow.Name = "contextMenuStrip1";
            this.ctxRow.Size = new System.Drawing.Size(157, 92);
            // 
            // zeileLöschenToolStripMenuItem
            // 
            this.zeileLöschenToolStripMenuItem.Name = "zeileLöschenToolStripMenuItem";
            this.zeileLöschenToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.zeileLöschenToolStripMenuItem.Text = "Zeile löschen";
            this.zeileLöschenToolStripMenuItem.Click += new System.EventHandler(this.zeileLöschenToolStripMenuItem_Click);
            // 
            // zeileHinzufügenToolStripMenuItem
            // 
            this.zeileHinzufügenToolStripMenuItem.Name = "zeileHinzufügenToolStripMenuItem";
            this.zeileHinzufügenToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.zeileHinzufügenToolStripMenuItem.Text = "Zeile einfügen";
            this.zeileHinzufügenToolStripMenuItem.Click += new System.EventHandler(this.zeileHinzufügenToolStripMenuItem_Click);
            // 
            // spalteLöschenToolStripMenuItem
            // 
            this.spalteLöschenToolStripMenuItem.Name = "spalteLöschenToolStripMenuItem";
            this.spalteLöschenToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.spalteLöschenToolStripMenuItem.Text = "Spalte löschen";
            this.spalteLöschenToolStripMenuItem.Click += new System.EventHandler(this.spalteLöschenToolStripMenuItem_Click);
            // 
            // spalteHinzufügenToolStripMenuItem
            // 
            this.spalteHinzufügenToolStripMenuItem.Name = "spalteHinzufügenToolStripMenuItem";
            this.spalteHinzufügenToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.spalteHinzufügenToolStripMenuItem.Text = "Spalte einfügen";
            this.spalteHinzufügenToolStripMenuItem.Click += new System.EventHandler(this.spalteHinzufügenToolStripMenuItem_Click);
            // 
            // ClipboardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dgTable);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ClipboardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Zwischenablage";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgTable)).EndInit();
            this.ctxRow.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem bestätigenToolStripMenuItem;
        private System.Windows.Forms.DataGridView dgTable;
        private System.Windows.Forms.ToolStripMenuItem spalteAufteilenToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip ctxRow;
        private System.Windows.Forms.ToolStripMenuItem zeileLöschenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zeileHinzufügenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spalteLöschenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spalteHinzufügenToolStripMenuItem;
    }
}