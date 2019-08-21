namespace DataTableConverter.View.WorkProcViews
{
    partial class TrimForm
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
            this.label10 = new System.Windows.Forms.Label();
            this.CbTrimDeleteDouble = new System.Windows.Forms.CheckBox();
            this.RbTrimStartEnd = new System.Windows.Forms.RadioButton();
            this.RbTrimEnd = new System.Windows.Forms.RadioButton();
            this.RbTrimStart = new System.Windows.Forms.RadioButton();
            this.lblTrimCharacter = new System.Windows.Forms.Label();
            this.TxtTrimText = new System.Windows.Forms.TextBox();
            this.BtnConfirm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(32, 202);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(248, 49);
            this.label10.TabIndex = 21;
            this.label10.Text = "Wenn das Zeichen öfter als 1 Mal hintereinander vorkommt, wird es auf 1 gesetzt. " +
    "Also zum Beispiel wird dann aus zwei Leerzeichen ein Leerzeichen";
            // 
            // CbTrimDeleteDouble
            // 
            this.CbTrimDeleteDouble.AutoSize = true;
            this.CbTrimDeleteDouble.Checked = true;
            this.CbTrimDeleteDouble.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CbTrimDeleteDouble.Location = new System.Drawing.Point(13, 176);
            this.CbTrimDeleteDouble.Name = "CbTrimDeleteDouble";
            this.CbTrimDeleteDouble.Size = new System.Drawing.Size(168, 17);
            this.CbTrimDeleteDouble.TabIndex = 20;
            this.CbTrimDeleteDouble.Text = "Doppelte Vorkommen löschen";
            this.CbTrimDeleteDouble.UseVisualStyleBackColor = true;
            // 
            // RbTrimStartEnd
            // 
            this.RbTrimStartEnd.AutoSize = true;
            this.RbTrimStartEnd.Checked = true;
            this.RbTrimStartEnd.Location = new System.Drawing.Point(12, 137);
            this.RbTrimStartEnd.Name = "RbTrimStartEnd";
            this.RbTrimStartEnd.Size = new System.Drawing.Size(108, 17);
            this.RbTrimStartEnd.TabIndex = 19;
            this.RbTrimStartEnd.TabStop = true;
            this.RbTrimStartEnd.Text = "Vorne und Hinten";
            this.RbTrimStartEnd.UseVisualStyleBackColor = true;
            // 
            // RbTrimEnd
            // 
            this.RbTrimEnd.AutoSize = true;
            this.RbTrimEnd.Location = new System.Drawing.Point(13, 106);
            this.RbTrimEnd.Name = "RbTrimEnd";
            this.RbTrimEnd.Size = new System.Drawing.Size(56, 17);
            this.RbTrimEnd.TabIndex = 18;
            this.RbTrimEnd.TabStop = true;
            this.RbTrimEnd.Text = "Hinten";
            this.RbTrimEnd.UseVisualStyleBackColor = true;
            // 
            // RbTrimStart
            // 
            this.RbTrimStart.AutoSize = true;
            this.RbTrimStart.Location = new System.Drawing.Point(13, 75);
            this.RbTrimStart.Name = "RbTrimStart";
            this.RbTrimStart.Size = new System.Drawing.Size(53, 17);
            this.RbTrimStart.TabIndex = 17;
            this.RbTrimStart.TabStop = true;
            this.RbTrimStart.Text = "Vorne";
            this.RbTrimStart.UseVisualStyleBackColor = true;
            // 
            // lblTrimCharacter
            // 
            this.lblTrimCharacter.AutoSize = true;
            this.lblTrimCharacter.Location = new System.Drawing.Point(9, 15);
            this.lblTrimCharacter.Name = "lblTrimCharacter";
            this.lblTrimCharacter.Size = new System.Drawing.Size(49, 13);
            this.lblTrimCharacter.TabIndex = 16;
            this.lblTrimCharacter.Text = "Zeichen:";
            // 
            // TxtTrimText
            // 
            this.TxtTrimText.Location = new System.Drawing.Point(12, 34);
            this.TxtTrimText.Name = "TxtTrimText";
            this.TxtTrimText.Size = new System.Drawing.Size(265, 20);
            this.TxtTrimText.TabIndex = 15;
            this.TxtTrimText.Text = " ";
            // 
            // BtnConfirm
            // 
            this.BtnConfirm.Location = new System.Drawing.Point(106, 272);
            this.BtnConfirm.Name = "BtnConfirm";
            this.BtnConfirm.Size = new System.Drawing.Size(75, 23);
            this.BtnConfirm.TabIndex = 22;
            this.BtnConfirm.Text = "Bestätigen";
            this.BtnConfirm.UseVisualStyleBackColor = true;
            this.BtnConfirm.Click += new System.EventHandler(this.BtnConfirm_Click);
            // 
            // TrimForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(297, 319);
            this.Controls.Add(this.BtnConfirm);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.CbTrimDeleteDouble);
            this.Controls.Add(this.RbTrimStartEnd);
            this.Controls.Add(this.RbTrimEnd);
            this.Controls.Add(this.RbTrimStart);
            this.Controls.Add(this.lblTrimCharacter);
            this.Controls.Add(this.TxtTrimText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "TrimForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Trim";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox CbTrimDeleteDouble;
        private System.Windows.Forms.RadioButton RbTrimStartEnd;
        private System.Windows.Forms.RadioButton RbTrimEnd;
        private System.Windows.Forms.RadioButton RbTrimStart;
        private System.Windows.Forms.Label lblTrimCharacter;
        private System.Windows.Forms.TextBox TxtTrimText;
        private System.Windows.Forms.Button BtnConfirm;
    }
}