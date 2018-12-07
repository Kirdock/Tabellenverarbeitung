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
            ((System.ComponentModel.ISupportInitialize)(this.dgDuplicate)).BeginInit();
            this.SuspendLayout();
            // 
            // dgDuplicate
            // 
            this.dgDuplicate.AllowUserToAddRows = false;
            this.dgDuplicate.AllowUserToDeleteRows = false;
            this.dgDuplicate.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgDuplicate.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgDuplicate.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgDuplicate.Location = new System.Drawing.Point(12, 12);
            this.dgDuplicate.Name = "dgDuplicate";
            this.dgDuplicate.RowHeadersVisible = false;
            this.dgDuplicate.Size = new System.Drawing.Size(404, 307);
            this.dgDuplicate.TabIndex = 0;
            this.dgDuplicate.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dgDuplicate_MouseDown);
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(435, 12);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(91, 23);
            this.btnConfirm.TabIndex = 1;
            this.btnConfirm.Text = "Übernehmen";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // SelectDuplicateColumns
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
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
    }
}