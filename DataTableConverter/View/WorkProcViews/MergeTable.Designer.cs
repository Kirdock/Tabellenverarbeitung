namespace DataTableConverter.View
{
    partial class MergeTable
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
            this.cmbIdentifierOriginal = new System.Windows.Forms.ComboBox();
            this.cmbIdentifierMerge = new System.Windows.Forms.ComboBox();
            this.clbColumns = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnTakeOver = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnRemoveAll = new System.Windows.Forms.Button();
            this.lblImportTable = new System.Windows.Forms.Label();
            this.lblSourceTable = new System.Windows.Forms.Label();
            this.lblImportTableText = new System.Windows.Forms.Label();
            this.lblRowCountSource = new System.Windows.Forms.Label();
            this.lblRowCountImport = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmbIdentifierOriginal
            // 
            this.cmbIdentifierOriginal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbIdentifierOriginal.FormattingEnabled = true;
            this.cmbIdentifierOriginal.Location = new System.Drawing.Point(230, 31);
            this.cmbIdentifierOriginal.Name = "cmbIdentifierOriginal";
            this.cmbIdentifierOriginal.Size = new System.Drawing.Size(121, 21);
            this.cmbIdentifierOriginal.TabIndex = 0;
            // 
            // cmbIdentifierMerge
            // 
            this.cmbIdentifierMerge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbIdentifierMerge.FormattingEnabled = true;
            this.cmbIdentifierMerge.Location = new System.Drawing.Point(230, 74);
            this.cmbIdentifierMerge.Name = "cmbIdentifierMerge";
            this.cmbIdentifierMerge.Size = new System.Drawing.Size(121, 21);
            this.cmbIdentifierMerge.TabIndex = 1;
            // 
            // clbColumns
            // 
            this.clbColumns.CheckOnClick = true;
            this.clbColumns.FormattingEnabled = true;
            this.clbColumns.HorizontalScrollbar = true;
            this.clbColumns.Location = new System.Drawing.Point(377, 61);
            this.clbColumns.Name = "clbColumns";
            this.clbColumns.Size = new System.Drawing.Size(294, 289);
            this.clbColumns.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(218, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Identifizierung der bereits geladenen Tabelle:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(215, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Identifizierung der hinzuzufügenden Tabelle:";
            // 
            // btnTakeOver
            // 
            this.btnTakeOver.Location = new System.Drawing.Point(230, 214);
            this.btnTakeOver.Name = "btnTakeOver";
            this.btnTakeOver.Size = new System.Drawing.Size(121, 23);
            this.btnTakeOver.TabIndex = 5;
            this.btnTakeOver.Text = "Übernehmen";
            this.btnTakeOver.UseVisualStyleBackColor = true;
            this.btnTakeOver.Click += new System.EventHandler(this.btnTakeOver_Click);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Location = new System.Drawing.Point(377, 29);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(117, 23);
            this.btnSelectAll.TabIndex = 6;
            this.btnSelectAll.Text = "Alle auswählen";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.BtnSelectAll_Click);
            // 
            // btnRemoveAll
            // 
            this.btnRemoveAll.Location = new System.Drawing.Point(554, 29);
            this.btnRemoveAll.Name = "btnRemoveAll";
            this.btnRemoveAll.Size = new System.Drawing.Size(117, 23);
            this.btnRemoveAll.TabIndex = 7;
            this.btnRemoveAll.Text = "Alle abwählen";
            this.btnRemoveAll.UseVisualStyleBackColor = true;
            this.btnRemoveAll.Click += new System.EventHandler(this.BtnRemoveAll_Click);
            // 
            // lblImportTable
            // 
            this.lblImportTable.AutoSize = true;
            this.lblImportTable.Location = new System.Drawing.Point(374, 9);
            this.lblImportTable.Name = "lblImportTable";
            this.lblImportTable.Size = new System.Drawing.Size(127, 13);
            this.lblImportTable.TabIndex = 11;
            this.lblImportTable.Text = "Zu importierende Tabelle:";
            // 
            // lblSourceTable
            // 
            this.lblSourceTable.AutoSize = true;
            this.lblSourceTable.Location = new System.Drawing.Point(12, 256);
            this.lblSourceTable.Name = "lblSourceTable";
            this.lblSourceTable.Size = new System.Drawing.Size(204, 13);
            this.lblSourceTable.TabIndex = 12;
            this.lblSourceTable.Text = "Anzahl der Zeilen, der geladenen Tabelle:";
            // 
            // lblImportTableText
            // 
            this.lblImportTableText.AutoSize = true;
            this.lblImportTableText.Location = new System.Drawing.Point(12, 285);
            this.lblImportTableText.Name = "lblImportTableText";
            this.lblImportTableText.Size = new System.Drawing.Size(237, 13);
            this.lblImportTableText.TabIndex = 13;
            this.lblImportTableText.Text = "Anzahl der Zeilen, der zu importierenden Tabelle:";
            // 
            // lblRowCountSource
            // 
            this.lblRowCountSource.AutoSize = true;
            this.lblRowCountSource.Location = new System.Drawing.Point(256, 256);
            this.lblRowCountSource.Name = "lblRowCountSource";
            this.lblRowCountSource.Size = new System.Drawing.Size(48, 13);
            this.lblRowCountSource.TabIndex = 14;
            this.lblRowCountSource.Text = "Anzahl 1";
            // 
            // lblRowCountImport
            // 
            this.lblRowCountImport.AutoSize = true;
            this.lblRowCountImport.Location = new System.Drawing.Point(256, 285);
            this.lblRowCountImport.Name = "lblRowCountImport";
            this.lblRowCountImport.Size = new System.Drawing.Size(48, 13);
            this.lblRowCountImport.TabIndex = 15;
            this.lblRowCountImport.Text = "Anzahl 2";
            // 
            // MergeTable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(690, 365);
            this.Controls.Add(this.lblRowCountImport);
            this.Controls.Add(this.lblRowCountSource);
            this.Controls.Add(this.lblImportTableText);
            this.Controls.Add(this.lblSourceTable);
            this.Controls.Add(this.lblImportTable);
            this.Controls.Add(this.btnRemoveAll);
            this.Controls.Add(this.btnSelectAll);
            this.Controls.Add(this.btnTakeOver);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.clbColumns);
            this.Controls.Add(this.cmbIdentifierMerge);
            this.Controls.Add(this.cmbIdentifierOriginal);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MergeTable";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PVM";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbIdentifierOriginal;
        private System.Windows.Forms.ComboBox cmbIdentifierMerge;
        private System.Windows.Forms.CheckedListBox clbColumns;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnTakeOver;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnRemoveAll;
        private System.Windows.Forms.Label lblImportTable;
        private System.Windows.Forms.Label lblSourceTable;
        private System.Windows.Forms.Label lblImportTableText;
        private System.Windows.Forms.Label lblRowCountSource;
        private System.Windows.Forms.Label lblRowCountImport;
    }
}