﻿namespace DataTableConverter.View
{
    partial class SortForm
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
            this.clBoxHeaders = new System.Windows.Forms.CheckedListBox();
            this.btnUp = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnTop = new System.Windows.Forms.Button();
            this.btnBottom = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.lBoxSelectedHeaders = new System.Windows.Forms.ListBox();
            this.rbWindows = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbReverse = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // clBoxHeaders
            // 
            this.clBoxHeaders.CheckOnClick = true;
            this.clBoxHeaders.FormattingEnabled = true;
            this.clBoxHeaders.HorizontalScrollbar = true;
            this.clBoxHeaders.Location = new System.Drawing.Point(12, 51);
            this.clBoxHeaders.Name = "clBoxHeaders";
            this.clBoxHeaders.Size = new System.Drawing.Size(316, 379);
            this.clBoxHeaders.TabIndex = 0;
            this.clBoxHeaders.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clBoxHeaders_ItemCheck);
            // 
            // btnUp
            // 
            this.btnUp.Location = new System.Drawing.Point(431, 22);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(75, 23);
            this.btnUp.TabIndex = 2;
            this.btnUp.Text = "Hoch";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnDown
            // 
            this.btnDown.Location = new System.Drawing.Point(512, 22);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(75, 23);
            this.btnDown.TabIndex = 3;
            this.btnDown.Text = "Runter";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnTop
            // 
            this.btnTop.Location = new System.Drawing.Point(350, 22);
            this.btnTop.Name = "btnTop";
            this.btnTop.Size = new System.Drawing.Size(75, 23);
            this.btnTop.TabIndex = 4;
            this.btnTop.Text = "Oben";
            this.btnTop.UseVisualStyleBackColor = true;
            this.btnTop.Click += new System.EventHandler(this.btnTop_Click);
            // 
            // btnBottom
            // 
            this.btnBottom.Location = new System.Drawing.Point(593, 22);
            this.btnBottom.Name = "btnBottom";
            this.btnBottom.Size = new System.Drawing.Size(75, 23);
            this.btnBottom.TabIndex = 5;
            this.btnBottom.Text = "Unten";
            this.btnBottom.UseVisualStyleBackColor = true;
            this.btnBottom.Click += new System.EventHandler(this.btnBottom_Click);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(693, 407);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(95, 23);
            this.btnOk.TabIndex = 6;
            this.btnOk.Text = "Übernehmen";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // lBoxSelectedHeaders
            // 
            this.lBoxSelectedHeaders.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lBoxSelectedHeaders.FormattingEnabled = true;
            this.lBoxSelectedHeaders.HorizontalScrollbar = true;
            this.lBoxSelectedHeaders.Location = new System.Drawing.Point(350, 51);
            this.lBoxSelectedHeaders.Name = "lBoxSelectedHeaders";
            this.lBoxSelectedHeaders.Size = new System.Drawing.Size(316, 381);
            this.lBoxSelectedHeaders.TabIndex = 9;
            this.lBoxSelectedHeaders.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lBoxSelectedHeaders_DrawItem);
            this.lBoxSelectedHeaders.DoubleClick += new System.EventHandler(this.lBoxSelectedHeaders_DoubleClick);
            this.lBoxSelectedHeaders.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lBoxSelectedHeaders_MouseDown);
            // 
            // rbWindows
            // 
            this.rbWindows.AutoSize = true;
            this.rbWindows.Location = new System.Drawing.Point(21, 19);
            this.rbWindows.Name = "rbWindows";
            this.rbWindows.Size = new System.Drawing.Size(58, 17);
            this.rbWindows.TabIndex = 10;
            this.rbWindows.TabStop = true;
            this.rbWindows.Text = "Normal";
            this.rbWindows.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbReverse);
            this.groupBox1.Controls.Add(this.rbWindows);
            this.groupBox1.Location = new System.Drawing.Point(672, 51);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(176, 97);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sortierung";
            // 
            // rbReverse
            // 
            this.rbReverse.AutoSize = true;
            this.rbReverse.Location = new System.Drawing.Point(21, 53);
            this.rbReverse.Name = "rbReverse";
            this.rbReverse.Size = new System.Drawing.Size(131, 17);
            this.rbReverse.TabIndex = 11;
            this.rbReverse.TabStop = true;
            this.rbReverse.Text = "Vorderseite/Rückseite";
            this.rbReverse.UseVisualStyleBackColor = true;
            // 
            // SortForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(860, 450);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lBoxSelectedHeaders);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnBottom);
            this.Controls.Add(this.btnTop);
            this.Controls.Add(this.btnDown);
            this.Controls.Add(this.btnUp);
            this.Controls.Add(this.clBoxHeaders);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "SortForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SortForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox clBoxHeaders;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnTop;
        private System.Windows.Forms.Button btnBottom;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.ListBox lBoxSelectedHeaders;
        private System.Windows.Forms.RadioButton rbWindows;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbReverse;
    }
}