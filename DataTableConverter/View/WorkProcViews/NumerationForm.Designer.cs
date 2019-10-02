namespace DataTableConverter.View.WorkProcViews
{
    partial class NumerationForm
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
            this.cbNumberRepeat = new System.Windows.Forms.CheckBox();
            this.nbNumberEnd = new System.Windows.Forms.NumericUpDown();
            this.label28 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.nbNumberStart = new System.Windows.Forms.NumericUpDown();
            this.txtNewColumn = new System.Windows.Forms.TextBox();
            this.label26 = new System.Windows.Forms.Label();
            this.btnConfirm = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nbNumberEnd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nbNumberStart)).BeginInit();
            this.SuspendLayout();
            // 
            // cbNumberRepeat
            // 
            this.cbNumberRepeat.AutoSize = true;
            this.cbNumberRepeat.Location = new System.Drawing.Point(13, 121);
            this.cbNumberRepeat.Name = "cbNumberRepeat";
            this.cbNumberRepeat.Size = new System.Drawing.Size(215, 17);
            this.cbNumberRepeat.TabIndex = 31;
            this.cbNumberRepeat.Text = "Nummerierung nach Ende wiederholen?";
            this.cbNumberRepeat.UseVisualStyleBackColor = true;
            // 
            // nbNumberEnd
            // 
            this.nbNumberEnd.Location = new System.Drawing.Point(133, 87);
            this.nbNumberEnd.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nbNumberEnd.Minimum = new decimal(new int[] {
            1000000,
            0,
            0,
            -2147483648});
            this.nbNumberEnd.Name = "nbNumberEnd";
            this.nbNumberEnd.Size = new System.Drawing.Size(70, 20);
            this.nbNumberEnd.TabIndex = 30;
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(130, 67);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(66, 13);
            this.label28.TabIndex = 29;
            this.label28.Text = "Endnummer:";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(12, 65);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(69, 13);
            this.label27.TabIndex = 28;
            this.label27.Text = "Startnummer:";
            // 
            // nbNumberStart
            // 
            this.nbNumberStart.Location = new System.Drawing.Point(12, 87);
            this.nbNumberStart.Maximum = new decimal(new int[] {
            1410065408,
            2,
            0,
            0});
            this.nbNumberStart.Minimum = new decimal(new int[] {
            1000000,
            0,
            0,
            -2147483648});
            this.nbNumberStart.Name = "nbNumberStart";
            this.nbNumberStart.Size = new System.Drawing.Size(70, 20);
            this.nbNumberStart.TabIndex = 27;
            // 
            // txtNewColumn
            // 
            this.txtNewColumn.Location = new System.Drawing.Point(12, 37);
            this.txtNewColumn.Name = "txtNewColumn";
            this.txtNewColumn.Size = new System.Drawing.Size(206, 20);
            this.txtNewColumn.TabIndex = 26;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(12, 18);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(122, 13);
            this.label26.TabIndex = 25;
            this.label26.Text = "Name der neuen Spalte:";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(76, 163);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(75, 23);
            this.btnConfirm.TabIndex = 32;
            this.btnConfirm.Text = "Bestätigen";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // NumerationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(248, 202);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.cbNumberRepeat);
            this.Controls.Add(this.nbNumberEnd);
            this.Controls.Add(this.label28);
            this.Controls.Add(this.label27);
            this.Controls.Add(this.nbNumberStart);
            this.Controls.Add(this.txtNewColumn);
            this.Controls.Add(this.label26);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "NumerationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nummerierung";
            ((System.ComponentModel.ISupportInitialize)(this.nbNumberEnd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nbNumberStart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbNumberRepeat;
        private System.Windows.Forms.NumericUpDown nbNumberEnd;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.NumericUpDown nbNumberStart;
        private System.Windows.Forms.TextBox txtNewColumn;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Button btnConfirm;
    }
}