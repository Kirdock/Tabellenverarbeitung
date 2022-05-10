namespace DataTableConverter.View
{
    partial class SelectDuplicateColumns
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
            this.dgDuplicate = new System.Windows.Forms.DataGridView();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.labelKeepColumn = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgDuplicate)).BeginInit();
            this.SuspendLayout();
            // 
            // dgDuplicate
            // 
            this.dgDuplicate.AllowUserToAddRows = false;
            this.dgDuplicate.AllowUserToDeleteRows = false;
            this.dgDuplicate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgDuplicate.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgDuplicate.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgDuplicate.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgDuplicate.Location = new System.Drawing.Point(12, 37);
            this.dgDuplicate.Name = "dgDuplicate";
            this.dgDuplicate.RowHeadersVisible = false;
            this.dgDuplicate.Size = new System.Drawing.Size(408, 401);
            this.dgDuplicate.TabIndex = 0;
            // 
            // btnConfirm
            // 
            this.btnConfirm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConfirm.Location = new System.Drawing.Point(435, 37);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(91, 23);
            this.btnConfirm.TabIndex = 1;
            this.btnConfirm.Text = "Übernehmen";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // labelKeepColumn
            // 
            this.labelKeepColumn.Location = new System.Drawing.Point(12, 2);
            this.labelKeepColumn.Name = "labelKeepColumn";
            this.labelKeepColumn.Size = new System.Drawing.Size(408, 32);
            this.labelKeepColumn.TabIndex = 2;
            this.labelKeepColumn.Text = "Vorsicht: Wenn \"<Beibehalten>\" ausgewählt wird, kann das dazu führen, dass die Au" +
    "sführung fehlschlägt, da die Spalte nicht gefunden werden kann";
            this.labelKeepColumn.Visible = false;
            // 
            // SelectDuplicateColumns
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(551, 450);
            this.Controls.Add(this.labelKeepColumn);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.dgDuplicate);
            this.Name = "SelectDuplicateColumns";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Es wurden folgende Spalten nicht gefunden";
            ((System.ComponentModel.ISupportInitialize)(this.dgDuplicate)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgDuplicate;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Label labelKeepColumn;
    }
}