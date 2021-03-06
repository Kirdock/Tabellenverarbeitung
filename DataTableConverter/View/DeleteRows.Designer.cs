﻿namespace DataTableConverter.View
{
    partial class DeleteRows
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
            this.NbStart = new System.Windows.Forms.NumericUpDown();
            this.NbEnd = new System.Windows.Forms.NumericUpDown();
            this.NbSingle = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BtnConfirmMulti = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.BtnConfirmSingle = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.BtnConfirmMatchText = new System.Windows.Forms.Button();
            this.TxtValue = new System.Windows.Forms.TextBox();
            this.CmBHeaders = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.CBEquals = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.NbStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NbEnd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NbSingle)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // NbStart
            // 
            this.NbStart.Location = new System.Drawing.Point(11, 45);
            this.NbStart.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.NbStart.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NbStart.Name = "NbStart";
            this.NbStart.Size = new System.Drawing.Size(91, 20);
            this.NbStart.TabIndex = 0;
            this.NbStart.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // NbEnd
            // 
            this.NbEnd.Location = new System.Drawing.Point(128, 45);
            this.NbEnd.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.NbEnd.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NbEnd.Name = "NbEnd";
            this.NbEnd.Size = new System.Drawing.Size(91, 20);
            this.NbEnd.TabIndex = 1;
            this.NbEnd.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // NbSingle
            // 
            this.NbSingle.Location = new System.Drawing.Point(14, 48);
            this.NbSingle.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.NbSingle.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NbSingle.Name = "NbSingle";
            this.NbSingle.Size = new System.Drawing.Size(91, 20);
            this.NbSingle.TabIndex = 2;
            this.NbSingle.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Von:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(125, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(24, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Bis:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.BtnConfirmMulti);
            this.groupBox1.Controls.Add(this.NbEnd);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.NbStart);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(401, 100);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Bereich";
            // 
            // BtnConfirmMulti
            // 
            this.BtnConfirmMulti.Location = new System.Drawing.Point(289, 45);
            this.BtnConfirmMulti.Name = "BtnConfirmMulti";
            this.BtnConfirmMulti.Size = new System.Drawing.Size(92, 23);
            this.BtnConfirmMulti.TabIndex = 5;
            this.BtnConfirmMulti.Text = "Ausführen";
            this.BtnConfirmMulti.UseVisualStyleBackColor = true;
            this.BtnConfirmMulti.Click += new System.EventHandler(this.BtnConfirmMulti_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.BtnConfirmSingle);
            this.groupBox2.Controls.Add(this.NbSingle);
            this.groupBox2.Location = new System.Drawing.Point(12, 117);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(401, 100);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Einzeln";
            // 
            // BtnConfirmSingle
            // 
            this.BtnConfirmSingle.Location = new System.Drawing.Point(289, 45);
            this.BtnConfirmSingle.Name = "BtnConfirmSingle";
            this.BtnConfirmSingle.Size = new System.Drawing.Size(92, 23);
            this.BtnConfirmSingle.TabIndex = 6;
            this.BtnConfirmSingle.Text = "Ausführen";
            this.BtnConfirmSingle.UseVisualStyleBackColor = true;
            this.BtnConfirmSingle.Click += new System.EventHandler(this.BtnConfirmSingle_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.CBEquals);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.CmBHeaders);
            this.groupBox3.Controls.Add(this.TxtValue);
            this.groupBox3.Controls.Add(this.BtnConfirmMatchText);
            this.groupBox3.Location = new System.Drawing.Point(13, 223);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(400, 100);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Entspricht Text";
            // 
            // BtnConfirmMatchText
            // 
            this.BtnConfirmMatchText.Location = new System.Drawing.Point(288, 45);
            this.BtnConfirmMatchText.Name = "BtnConfirmMatchText";
            this.BtnConfirmMatchText.Size = new System.Drawing.Size(92, 23);
            this.BtnConfirmMatchText.TabIndex = 6;
            this.BtnConfirmMatchText.Text = "Ausführen";
            this.BtnConfirmMatchText.UseVisualStyleBackColor = true;
            this.BtnConfirmMatchText.Click += new System.EventHandler(this.BtnConfirmMatchText_Click);
            // 
            // TxtValue
            // 
            this.TxtValue.Location = new System.Drawing.Point(13, 47);
            this.TxtValue.Name = "TxtValue";
            this.TxtValue.Size = new System.Drawing.Size(100, 20);
            this.TxtValue.TabIndex = 7;
            // 
            // CmBHeaders
            // 
            this.CmBHeaders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmBHeaders.FormattingEnabled = true;
            this.CmBHeaders.Location = new System.Drawing.Point(127, 47);
            this.CmBHeaders.Name = "CmBHeaders";
            this.CmBHeaders.Size = new System.Drawing.Size(131, 21);
            this.CmBHeaders.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 31);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Text:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(124, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "In Spalte:";
            // 
            // CBEquals
            // 
            this.CBEquals.AutoSize = true;
            this.CBEquals.Checked = true;
            this.CBEquals.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBEquals.Location = new System.Drawing.Point(13, 77);
            this.CBEquals.Name = "CBEquals";
            this.CBEquals.Size = new System.Drawing.Size(148, 17);
            this.CBEquals.TabIndex = 11;
            this.CBEquals.Text = "Genaue Übereinstimmung";
            this.CBEquals.UseVisualStyleBackColor = true;
            // 
            // DeleteRows
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(425, 336);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "DeleteRows";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Zeilen löschen";
            ((System.ComponentModel.ISupportInitialize)(this.NbStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NbEnd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NbSingle)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NumericUpDown NbStart;
        private System.Windows.Forms.NumericUpDown NbEnd;
        private System.Windows.Forms.NumericUpDown NbSingle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button BtnConfirmMulti;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button BtnConfirmSingle;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox CmBHeaders;
        private System.Windows.Forms.TextBox TxtValue;
        private System.Windows.Forms.Button BtnConfirmMatchText;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox CBEquals;
    }
}