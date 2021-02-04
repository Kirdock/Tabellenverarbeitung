namespace DataTableConverter.View
{
    partial class TextFormat
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
            this.rbSeparated = new System.Windows.Forms.RadioButton();
            this.rbFixed = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.LblCodePageMessage = new System.Windows.Forms.Label();
            this.cbTakeOver = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbEncoding = new System.Windows.Forms.ComboBox();
            this.gbSeparated = new System.Windows.Forms.GroupBox();
            this.BtnEditSeparators = new System.Windows.Forms.Button();
            this.btnHeaderRename = new System.Windows.Forms.Button();
            this.btnHeaderSave = new System.Windows.Forms.Button();
            this.btnHeaderDelete = new System.Windows.Forms.Button();
            this.btnHeaderLoad = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbHeaderPresets = new System.Windows.Forms.ComboBox();
            this.dgvHeaders = new System.Windows.Forms.DataGridView();
            this.cbContainsHeaders = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtEnd = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtBegin = new System.Windows.Forms.TextBox();
            this.rbBetween = new System.Windows.Forms.RadioButton();
            this.rbSep = new System.Windows.Forms.RadioButton();
            this.rbTab = new System.Windows.Forms.RadioButton();
            this.btnAcceptSeparate = new System.Windows.Forms.Button();
            this.txtSeparator = new System.Windows.Forms.TextBox();
            this.gbFixed = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.BtnRenamePreset = new System.Windows.Forms.Button();
            this.cmbPresets = new System.Windows.Forms.ComboBox();
            this.btnDeletePreset = new System.Windows.Forms.Button();
            this.btnSavePreset = new System.Windows.Forms.Button();
            this.btnLoadPreset = new System.Windows.Forms.Button();
            this.cmbVariant = new System.Windows.Forms.ComboBox();
            this.btnAcceptFixed = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.dgvSetting = new System.Windows.Forms.DataGridView();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dgvPreview = new System.Windows.Forms.DataGridView();
            this.ctxRow = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.zeileLöschenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zwischenablageEinfügenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.LblFileName = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnSyncPreview = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.gbSeparated.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHeaders)).BeginInit();
            this.gbFixed.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSetting)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreview)).BeginInit();
            this.ctxRow.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // rbSeparated
            // 
            this.rbSeparated.AutoSize = true;
            this.rbSeparated.Location = new System.Drawing.Point(279, 46);
            this.rbSeparated.Name = "rbSeparated";
            this.rbSeparated.Size = new System.Drawing.Size(66, 17);
            this.rbSeparated.TabIndex = 0;
            this.rbSeparated.TabStop = true;
            this.rbSeparated.Text = "Getrennt";
            this.rbSeparated.UseVisualStyleBackColor = true;
            // 
            // rbFixed
            // 
            this.rbFixed.AutoSize = true;
            this.rbFixed.Location = new System.Drawing.Point(429, 46);
            this.rbFixed.Name = "rbFixed";
            this.rbFixed.Size = new System.Drawing.Size(81, 17);
            this.rbFixed.TabIndex = 1;
            this.rbFixed.TabStop = true;
            this.rbFixed.Text = "Feste Breite";
            this.rbFixed.UseVisualStyleBackColor = true;
            this.rbFixed.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.LblCodePageMessage);
            this.groupBox1.Controls.Add(this.cbTakeOver);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cmbEncoding);
            this.groupBox1.Controls.Add(this.rbSeparated);
            this.groupBox1.Controls.Add(this.rbFixed);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(800, 105);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Art";
            // 
            // LblCodePageMessage
            // 
            this.LblCodePageMessage.AutoSize = true;
            this.LblCodePageMessage.ForeColor = System.Drawing.Color.Red;
            this.LblCodePageMessage.Location = new System.Drawing.Point(12, 80);
            this.LblCodePageMessage.Name = "LblCodePageMessage";
            this.LblCodePageMessage.Size = new System.Drawing.Size(287, 13);
            this.LblCodePageMessage.TabIndex = 5;
            this.LblCodePageMessage.Text = "Die CodePage entspricht nicht der festgestellten CodePage";
            this.LblCodePageMessage.Visible = false;
            // 
            // cbTakeOver
            // 
            this.cbTakeOver.AutoSize = true;
            this.cbTakeOver.Location = new System.Drawing.Point(523, 47);
            this.cbTakeOver.Name = "cbTakeOver";
            this.cbTakeOver.Size = new System.Drawing.Size(265, 17);
            this.cbTakeOver.TabIndex = 4;
            this.cbTakeOver.Text = "Auf alle Dateien mit derselben Endung anwenden?";
            this.cbTakeOver.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "CodePage:";
            // 
            // cmbEncoding
            // 
            this.cmbEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEncoding.FormattingEnabled = true;
            this.cmbEncoding.Location = new System.Drawing.Point(12, 46);
            this.cmbEncoding.Name = "cmbEncoding";
            this.cmbEncoding.Size = new System.Drawing.Size(253, 21);
            this.cmbEncoding.TabIndex = 2;
            // 
            // gbSeparated
            // 
            this.gbSeparated.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbSeparated.Controls.Add(this.btnSyncPreview);
            this.gbSeparated.Controls.Add(this.BtnEditSeparators);
            this.gbSeparated.Controls.Add(this.btnHeaderRename);
            this.gbSeparated.Controls.Add(this.btnHeaderSave);
            this.gbSeparated.Controls.Add(this.btnHeaderDelete);
            this.gbSeparated.Controls.Add(this.btnHeaderLoad);
            this.gbSeparated.Controls.Add(this.label5);
            this.gbSeparated.Controls.Add(this.cmbHeaderPresets);
            this.gbSeparated.Controls.Add(this.dgvHeaders);
            this.gbSeparated.Controls.Add(this.cbContainsHeaders);
            this.gbSeparated.Controls.Add(this.label4);
            this.gbSeparated.Controls.Add(this.txtEnd);
            this.gbSeparated.Controls.Add(this.label3);
            this.gbSeparated.Controls.Add(this.txtBegin);
            this.gbSeparated.Controls.Add(this.rbBetween);
            this.gbSeparated.Controls.Add(this.rbSep);
            this.gbSeparated.Controls.Add(this.rbTab);
            this.gbSeparated.Controls.Add(this.btnAcceptSeparate);
            this.gbSeparated.Controls.Add(this.txtSeparator);
            this.gbSeparated.Location = new System.Drawing.Point(3, 111);
            this.gbSeparated.Name = "gbSeparated";
            this.gbSeparated.Size = new System.Drawing.Size(797, 281);
            this.gbSeparated.TabIndex = 4;
            this.gbSeparated.TabStop = false;
            this.gbSeparated.Text = "Getrennt";
            this.gbSeparated.Visible = false;
            // 
            // BtnEditSeparators
            // 
            this.BtnEditSeparators.Location = new System.Drawing.Point(305, 58);
            this.BtnEditSeparators.Name = "BtnEditSeparators";
            this.BtnEditSeparators.Size = new System.Drawing.Size(25, 23);
            this.BtnEditSeparators.TabIndex = 18;
            this.BtnEditSeparators.Text = "+";
            this.BtnEditSeparators.UseVisualStyleBackColor = true;
            this.BtnEditSeparators.Click += new System.EventHandler(this.BtnEditSeparators_Click);
            // 
            // btnHeaderRename
            // 
            this.btnHeaderRename.Location = new System.Drawing.Point(690, 133);
            this.btnHeaderRename.Name = "btnHeaderRename";
            this.btnHeaderRename.Size = new System.Drawing.Size(92, 23);
            this.btnHeaderRename.TabIndex = 17;
            this.btnHeaderRename.Text = "Umbenennen";
            this.btnHeaderRename.UseVisualStyleBackColor = true;
            this.btnHeaderRename.Click += new System.EventHandler(this.btnHeaderRename_Click);
            // 
            // btnHeaderSave
            // 
            this.btnHeaderSave.Location = new System.Drawing.Point(591, 133);
            this.btnHeaderSave.Name = "btnHeaderSave";
            this.btnHeaderSave.Size = new System.Drawing.Size(94, 23);
            this.btnHeaderSave.TabIndex = 16;
            this.btnHeaderSave.Text = "Speichern";
            this.btnHeaderSave.UseVisualStyleBackColor = true;
            this.btnHeaderSave.Click += new System.EventHandler(this.btnHeaderSave_Click);
            // 
            // btnHeaderDelete
            // 
            this.btnHeaderDelete.Location = new System.Drawing.Point(691, 85);
            this.btnHeaderDelete.Name = "btnHeaderDelete";
            this.btnHeaderDelete.Size = new System.Drawing.Size(91, 23);
            this.btnHeaderDelete.TabIndex = 15;
            this.btnHeaderDelete.Text = "Löschen";
            this.btnHeaderDelete.UseVisualStyleBackColor = true;
            this.btnHeaderDelete.Click += new System.EventHandler(this.btnHeaderDelete_Click);
            // 
            // btnHeaderLoad
            // 
            this.btnHeaderLoad.Location = new System.Drawing.Point(591, 85);
            this.btnHeaderLoad.Name = "btnHeaderLoad";
            this.btnHeaderLoad.Size = new System.Drawing.Size(94, 23);
            this.btnHeaderLoad.TabIndex = 14;
            this.btnHeaderLoad.Text = "Laden";
            this.btnHeaderLoad.UseVisualStyleBackColor = true;
            this.btnHeaderLoad.Click += new System.EventHandler(this.btnHeaderLoad_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(591, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Vorlagen:";
            // 
            // cmbHeaderPresets
            // 
            this.cmbHeaderPresets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHeaderPresets.FormattingEnabled = true;
            this.cmbHeaderPresets.Location = new System.Drawing.Point(591, 48);
            this.cmbHeaderPresets.Name = "cmbHeaderPresets";
            this.cmbHeaderPresets.Size = new System.Drawing.Size(191, 21);
            this.cmbHeaderPresets.TabIndex = 12;
            // 
            // dgvHeaders
            // 
            this.dgvHeaders.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvHeaders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvHeaders.Location = new System.Drawing.Point(345, 31);
            this.dgvHeaders.Name = "dgvHeaders";
            this.dgvHeaders.Size = new System.Drawing.Size(240, 237);
            this.dgvHeaders.TabIndex = 11;
            this.dgvHeaders.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvHeaders_CellEndEdit);
            this.dgvHeaders.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dgvHeaders_RowPostPaint);
            // 
            // cbContainsHeaders
            // 
            this.cbContainsHeaders.AutoSize = true;
            this.cbContainsHeaders.Location = new System.Drawing.Point(84, 148);
            this.cbContainsHeaders.Name = "cbContainsHeaders";
            this.cbContainsHeaders.Size = new System.Drawing.Size(143, 17);
            this.cbContainsHeaders.TabIndex = 10;
            this.cbContainsHeaders.Text = "Überschrift in erster Zeile";
            this.cbContainsHeaders.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(273, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Ende:";
            // 
            // txtEnd
            // 
            this.txtEnd.Location = new System.Drawing.Point(276, 104);
            this.txtEnd.Name = "txtEnd";
            this.txtEnd.Size = new System.Drawing.Size(63, 20);
            this.txtEnd.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(196, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Anfang:";
            // 
            // txtBegin
            // 
            this.txtBegin.Location = new System.Drawing.Point(199, 103);
            this.txtBegin.Name = "txtBegin";
            this.txtBegin.Size = new System.Drawing.Size(63, 20);
            this.txtBegin.TabIndex = 6;
            // 
            // rbBetween
            // 
            this.rbBetween.AutoSize = true;
            this.rbBetween.Location = new System.Drawing.Point(84, 104);
            this.rbBetween.Name = "rbBetween";
            this.rbBetween.Size = new System.Drawing.Size(113, 17);
            this.rbBetween.TabIndex = 5;
            this.rbBetween.TabStop = true;
            this.rbBetween.Text = "Eingeschlossen in:";
            this.rbBetween.UseVisualStyleBackColor = true;
            // 
            // rbSep
            // 
            this.rbSep.AutoSize = true;
            this.rbSep.Checked = true;
            this.rbSep.Location = new System.Drawing.Point(84, 61);
            this.rbSep.Name = "rbSep";
            this.rbSep.Size = new System.Drawing.Size(93, 17);
            this.rbSep.TabIndex = 4;
            this.rbSep.TabStop = true;
            this.rbSep.Text = "Trennzeichen:";
            this.rbSep.UseVisualStyleBackColor = true;
            // 
            // rbTab
            // 
            this.rbTab.AutoSize = true;
            this.rbTab.Location = new System.Drawing.Point(84, 29);
            this.rbTab.Name = "rbTab";
            this.rbTab.Size = new System.Drawing.Size(70, 17);
            this.rbTab.TabIndex = 3;
            this.rbTab.Text = "Tabstopp";
            this.rbTab.UseVisualStyleBackColor = true;
            // 
            // btnAcceptSeparate
            // 
            this.btnAcceptSeparate.Location = new System.Drawing.Point(691, 245);
            this.btnAcceptSeparate.Name = "btnAcceptSeparate";
            this.btnAcceptSeparate.Size = new System.Drawing.Size(94, 23);
            this.btnAcceptSeparate.TabIndex = 2;
            this.btnAcceptSeparate.Text = "Übernehmen";
            this.btnAcceptSeparate.UseVisualStyleBackColor = true;
            this.btnAcceptSeparate.Click += new System.EventHandler(this.btnAcceptSeparate_Click);
            // 
            // txtSeparator
            // 
            this.txtSeparator.Location = new System.Drawing.Point(199, 60);
            this.txtSeparator.Name = "txtSeparator";
            this.txtSeparator.Size = new System.Drawing.Size(100, 20);
            this.txtSeparator.TabIndex = 0;
            // 
            // gbFixed
            // 
            this.gbFixed.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbFixed.Controls.Add(this.groupBox3);
            this.gbFixed.Controls.Add(this.cmbVariant);
            this.gbFixed.Controls.Add(this.btnAcceptFixed);
            this.gbFixed.Controls.Add(this.label1);
            this.gbFixed.Controls.Add(this.dgvSetting);
            this.gbFixed.Location = new System.Drawing.Point(0, 111);
            this.gbFixed.Name = "gbFixed";
            this.gbFixed.Size = new System.Drawing.Size(800, 281);
            this.gbFixed.TabIndex = 3;
            this.gbFixed.TabStop = false;
            this.gbFixed.Text = "Feste Breite";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.BtnRenamePreset);
            this.groupBox3.Controls.Add(this.cmbPresets);
            this.groupBox3.Controls.Add(this.btnDeletePreset);
            this.groupBox3.Controls.Add(this.btnSavePreset);
            this.groupBox3.Controls.Add(this.btnLoadPreset);
            this.groupBox3.Location = new System.Drawing.Point(424, 47);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(361, 148);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Vorlagen";
            // 
            // BtnRenamePreset
            // 
            this.BtnRenamePreset.Location = new System.Drawing.Point(29, 110);
            this.BtnRenamePreset.Name = "BtnRenamePreset";
            this.BtnRenamePreset.Size = new System.Drawing.Size(97, 23);
            this.BtnRenamePreset.TabIndex = 8;
            this.BtnRenamePreset.Text = "Umbenennen";
            this.BtnRenamePreset.UseVisualStyleBackColor = true;
            this.BtnRenamePreset.Click += new System.EventHandler(this.BtnRenamePreset_Click);
            // 
            // cmbPresets
            // 
            this.cmbPresets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPresets.FormattingEnabled = true;
            this.cmbPresets.Location = new System.Drawing.Point(29, 28);
            this.cmbPresets.Name = "cmbPresets";
            this.cmbPresets.Size = new System.Drawing.Size(303, 21);
            this.cmbPresets.TabIndex = 5;
            // 
            // btnDeletePreset
            // 
            this.btnDeletePreset.Location = new System.Drawing.Point(132, 67);
            this.btnDeletePreset.Name = "btnDeletePreset";
            this.btnDeletePreset.Size = new System.Drawing.Size(97, 23);
            this.btnDeletePreset.TabIndex = 7;
            this.btnDeletePreset.Text = "Löschen";
            this.btnDeletePreset.UseVisualStyleBackColor = true;
            this.btnDeletePreset.Click += new System.EventHandler(this.btnDeletePreset_Click);
            // 
            // btnSavePreset
            // 
            this.btnSavePreset.Location = new System.Drawing.Point(235, 67);
            this.btnSavePreset.Name = "btnSavePreset";
            this.btnSavePreset.Size = new System.Drawing.Size(97, 23);
            this.btnSavePreset.TabIndex = 3;
            this.btnSavePreset.Text = "Speichern";
            this.btnSavePreset.UseVisualStyleBackColor = true;
            this.btnSavePreset.Click += new System.EventHandler(this.btnSavePreset_Click);
            // 
            // btnLoadPreset
            // 
            this.btnLoadPreset.Location = new System.Drawing.Point(29, 67);
            this.btnLoadPreset.Name = "btnLoadPreset";
            this.btnLoadPreset.Size = new System.Drawing.Size(97, 23);
            this.btnLoadPreset.TabIndex = 4;
            this.btnLoadPreset.Text = "Laden";
            this.btnLoadPreset.UseVisualStyleBackColor = true;
            this.btnLoadPreset.Click += new System.EventHandler(this.btnLoadPreset_Click);
            // 
            // cmbVariant
            // 
            this.cmbVariant.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVariant.FormattingEnabled = true;
            this.cmbVariant.Items.AddRange(new object[] {
            "Länge",
            "Bereich"});
            this.cmbVariant.Location = new System.Drawing.Point(103, 19);
            this.cmbVariant.Name = "cmbVariant";
            this.cmbVariant.Size = new System.Drawing.Size(121, 21);
            this.cmbVariant.TabIndex = 6;
            this.cmbVariant.SelectedIndexChanged += new System.EventHandler(this.cmbVariant_SelectedIndexChanged);
            // 
            // btnAcceptFixed
            // 
            this.btnAcceptFixed.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAcceptFixed.Location = new System.Drawing.Point(693, 245);
            this.btnAcceptFixed.Name = "btnAcceptFixed";
            this.btnAcceptFixed.Size = new System.Drawing.Size(92, 23);
            this.btnAcceptFixed.TabIndex = 2;
            this.btnAcceptFixed.Text = "Übernehmen";
            this.btnAcceptFixed.UseVisualStyleBackColor = true;
            this.btnAcceptFixed.Click += new System.EventHandler(this.btnAcceptFixed_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Einstellung:";
            // 
            // dgvSetting
            // 
            this.dgvSetting.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSetting.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSetting.Location = new System.Drawing.Point(6, 47);
            this.dgvSetting.Name = "dgvSetting";
            this.dgvSetting.Size = new System.Drawing.Size(402, 221);
            this.dgvSetting.TabIndex = 0;
            this.dgvSetting.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dgvSetting_CellValidating);
            this.dgvSetting.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dgvSetting_EditingControlShowing);
            this.dgvSetting.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dgvSetting_RowPostPaint);
            this.dgvSetting.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dgvSetting_MouseClick);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.dgvPreview);
            this.groupBox2.Location = new System.Drawing.Point(0, 398);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(800, 178);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Vorschau";
            // 
            // dgvPreview
            // 
            this.dgvPreview.AllowUserToAddRows = false;
            this.dgvPreview.AllowUserToDeleteRows = false;
            this.dgvPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvPreview.Location = new System.Drawing.Point(3, 16);
            this.dgvPreview.Name = "dgvPreview";
            this.dgvPreview.ReadOnly = true;
            this.dgvPreview.Size = new System.Drawing.Size(794, 137);
            this.dgvPreview.TabIndex = 0;
            this.dgvPreview.ColumnAdded += new System.Windows.Forms.DataGridViewColumnEventHandler(this.dgvPreview_ColumnAdded);
            this.dgvPreview.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dgvSetting_RowPostPaint);
            // 
            // ctxRow
            // 
            this.ctxRow.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zeileLöschenToolStripMenuItem,
            this.zwischenablageEinfügenToolStripMenuItem});
            this.ctxRow.Name = "ctxRow";
            this.ctxRow.Size = new System.Drawing.Size(210, 48);
            // 
            // zeileLöschenToolStripMenuItem
            // 
            this.zeileLöschenToolStripMenuItem.Name = "zeileLöschenToolStripMenuItem";
            this.zeileLöschenToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.zeileLöschenToolStripMenuItem.Text = "Zeile löschen";
            // 
            // zwischenablageEinfügenToolStripMenuItem
            // 
            this.zwischenablageEinfügenToolStripMenuItem.Name = "zwischenablageEinfügenToolStripMenuItem";
            this.zwischenablageEinfügenToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.zwischenablageEinfügenToolStripMenuItem.Text = "Zwischenablage einfügen";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.LblFileName});
            this.statusStrip1.Location = new System.Drawing.Point(0, 554);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(71, 17);
            this.toolStripStatusLabel1.Text = "Dateiname:";
            // 
            // LblFileName
            // 
            this.LblFileName.Name = "LblFileName";
            this.LblFileName.Size = new System.Drawing.Size(31, 17);
            this.LblFileName.Text = "Pfad";
            // 
            // btnSyncPreview
            // 
            this.btnSyncPreview.Location = new System.Drawing.Point(9, 245);
            this.btnSyncPreview.Name = "btnSyncPreview";
            this.btnSyncPreview.Size = new System.Drawing.Size(136, 23);
            this.btnSyncPreview.TabIndex = 19;
            this.btnSyncPreview.Text = "Vorschau aktualisieren";
            this.btnSyncPreview.UseVisualStyleBackColor = true;
            this.btnSyncPreview.Click += new System.EventHandler(this.btnSyncPreview_Click);
            // 
            // TextFormat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 576);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.gbSeparated);
            this.Controls.Add(this.gbFixed);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.MinimumSize = new System.Drawing.Size(816, 615);
            this.Name = "TextFormat";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Textkonvertierung";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TextFormat_FormClosing);
            this.Load += new System.EventHandler(this.TextFormat_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.gbSeparated.ResumeLayout(false);
            this.gbSeparated.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHeaders)).EndInit();
            this.gbFixed.ResumeLayout(false);
            this.gbFixed.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSetting)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreview)).EndInit();
            this.ctxRow.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton rbSeparated;
        private System.Windows.Forms.RadioButton rbFixed;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox gbFixed;
        private System.Windows.Forms.DataGridView dgvSetting;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox gbSeparated;
        private System.Windows.Forms.Button btnAcceptSeparate;
        private System.Windows.Forms.TextBox txtSeparator;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dgvPreview;
        private System.Windows.Forms.Button btnAcceptFixed;
        private System.Windows.Forms.Button btnSavePreset;
        private System.Windows.Forms.ComboBox cmbPresets;
        private System.Windows.Forms.Button btnLoadPreset;
        private System.Windows.Forms.ComboBox cmbVariant;
        private System.Windows.Forms.Button btnDeletePreset;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton rbSep;
        private System.Windows.Forms.RadioButton rbTab;
        private System.Windows.Forms.ComboBox cmbEncoding;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ContextMenuStrip ctxRow;
        private System.Windows.Forms.ToolStripMenuItem zeileLöschenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zwischenablageEinfügenToolStripMenuItem;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtEnd;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtBegin;
        private System.Windows.Forms.RadioButton rbBetween;
        private System.Windows.Forms.CheckBox cbTakeOver;
        private System.Windows.Forms.Button BtnRenamePreset;
        private System.Windows.Forms.CheckBox cbContainsHeaders;
        private System.Windows.Forms.DataGridView dgvHeaders;
        private System.Windows.Forms.Button btnHeaderLoad;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbHeaderPresets;
        private System.Windows.Forms.Button btnHeaderRename;
        private System.Windows.Forms.Button btnHeaderSave;
        private System.Windows.Forms.Button btnHeaderDelete;
        private System.Windows.Forms.Button BtnEditSeparators;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel LblFileName;
        private System.Windows.Forms.Label LblCodePageMessage;
        private System.Windows.Forms.Button btnSyncPreview;
    }
}