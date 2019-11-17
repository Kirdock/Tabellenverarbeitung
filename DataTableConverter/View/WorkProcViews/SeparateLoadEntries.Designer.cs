namespace DataTableConverter.View.WorkProcViews
{
    partial class SeparateLoadEntries
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
            this.CmBColumns = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.BtnUncheckAll = new System.Windows.Forms.Button();
            this.BtnCheckAll = new System.Windows.Forms.Button();
            this.BtnConfirm = new System.Windows.Forms.Button();
            this.CLBValues = new DataTableConverter.View.CustomControls.CountListbox();
            this.SuspendLayout();
            // 
            // CmBColumns
            // 
            this.CmBColumns.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmBColumns.FormattingEnabled = true;
            this.CmBColumns.Location = new System.Drawing.Point(12, 25);
            this.CmBColumns.Name = "CmBColumns";
            this.CmBColumns.Size = new System.Drawing.Size(257, 21);
            this.CmBColumns.TabIndex = 1;
            this.CmBColumns.SelectedIndexChanged += new System.EventHandler(this.CmBColumns_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Spalte:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Werte:";
            // 
            // BtnUncheckAll
            // 
            this.BtnUncheckAll.Location = new System.Drawing.Point(161, 73);
            this.BtnUncheckAll.Name = "BtnUncheckAll";
            this.BtnUncheckAll.Size = new System.Drawing.Size(108, 23);
            this.BtnUncheckAll.TabIndex = 23;
            this.BtnUncheckAll.Text = "Alles abwählen";
            this.BtnUncheckAll.UseVisualStyleBackColor = true;
            this.BtnUncheckAll.Click += new System.EventHandler(this.BtnUncheckAll_Click);
            // 
            // BtnCheckAll
            // 
            this.BtnCheckAll.Location = new System.Drawing.Point(12, 73);
            this.BtnCheckAll.Name = "BtnCheckAll";
            this.BtnCheckAll.Size = new System.Drawing.Size(108, 23);
            this.BtnCheckAll.TabIndex = 22;
            this.BtnCheckAll.Text = "Alles auswählen";
            this.BtnCheckAll.UseVisualStyleBackColor = true;
            this.BtnCheckAll.Click += new System.EventHandler(this.BtnCheckAll_Click);
            // 
            // BtnConfirm
            // 
            this.BtnConfirm.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BtnConfirm.Location = new System.Drawing.Point(91, 342);
            this.BtnConfirm.Name = "BtnConfirm";
            this.BtnConfirm.Size = new System.Drawing.Size(85, 23);
            this.BtnConfirm.TabIndex = 24;
            this.BtnConfirm.Text = "Bestätigen";
            this.BtnConfirm.UseVisualStyleBackColor = true;
            // 
            // CLBValues
            // 
            this.CLBValues.CheckOnClick = true;
            this.CLBValues.FormattingEnabled = true;
            this.CLBValues.HorizontalScrollbar = true;
            this.CLBValues.Location = new System.Drawing.Point(12, 112);
            this.CLBValues.Name = "CLBValues";
            this.CLBValues.Size = new System.Drawing.Size(257, 214);
            this.CLBValues.TabIndex = 25;
            this.CLBValues.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.CLBValues_ItemCheck);
            // 
            // SeparateLoadEntries
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(286, 377);
            this.Controls.Add(this.CLBValues);
            this.Controls.Add(this.BtnConfirm);
            this.Controls.Add(this.BtnUncheckAll);
            this.Controls.Add(this.BtnCheckAll);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CmBColumns);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "SeparateLoadEntries";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Trennen";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox CmBColumns;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BtnUncheckAll;
        private System.Windows.Forms.Button BtnCheckAll;
        private System.Windows.Forms.Button BtnConfirm;
        private CustomControls.CountListbox CLBValues;
    }
}