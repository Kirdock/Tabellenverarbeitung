namespace DataTableConverter.View
{
    partial class SeparatorForm
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
            this.DGVSeparators = new System.Windows.Forms.DataGridView();
            this.BtnConfirm = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.DGVSeparators)).BeginInit();
            this.SuspendLayout();
            // 
            // DGVSeparators
            // 
            this.DGVSeparators.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGVSeparators.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DGVSeparators.Location = new System.Drawing.Point(0, 0);
            this.DGVSeparators.Name = "DGVSeparators";
            this.DGVSeparators.Size = new System.Drawing.Size(800, 450);
            this.DGVSeparators.TabIndex = 1;
            this.DGVSeparators.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGVSeparators_CellEndEdit);
            // 
            // BtnConfirm
            // 
            this.BtnConfirm.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BtnConfirm.Location = new System.Drawing.Point(0, 420);
            this.BtnConfirm.Name = "BtnConfirm";
            this.BtnConfirm.Size = new System.Drawing.Size(800, 30);
            this.BtnConfirm.TabIndex = 2;
            this.BtnConfirm.Text = "Bestätigen";
            this.BtnConfirm.UseVisualStyleBackColor = true;
            this.BtnConfirm.Click += new System.EventHandler(this.BtnConfirm_Click);
            // 
            // SeparatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.BtnConfirm);
            this.Controls.Add(this.DGVSeparators);
            this.Name = "SeparatorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Trennzeichen";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SeparatorForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.DGVSeparators)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.DataGridView DGVSeparators;
        private System.Windows.Forms.Button BtnConfirm;
    }
}