namespace DataTableConverter.View
{
    partial class Administration
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dgvReplaces = new System.Windows.Forms.DataGridView();
            this.label3 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.ltbProcedures = new System.Windows.Forms.ListBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.gbWorkflow = new System.Windows.Forms.GroupBox();
            this.cmbProcedureType = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnUp = new System.Windows.Forms.Button();
            this.lbProcedures = new System.Windows.Forms.ListBox();
            this.button4 = new System.Windows.Forms.Button();
            this.btnAddProcedureToWorkflow = new System.Windows.Forms.Button();
            this.lbUsedProcedures = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtWorkflow = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.gbProcedure = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtWorkProcName = new System.Windows.Forms.TextBox();
            this.lblNewColumn = new System.Windows.Forms.Label();
            this.lblHeaders = new System.Windows.Forms.Label();
            this.txtFormula = new System.Windows.Forms.TextBox();
            this.lblFormula = new System.Windows.Forms.Label();
            this.txtNewColumn = new System.Windows.Forms.TextBox();
            this.cbNewColumn = new System.Windows.Forms.CheckBox();
            this.dgvColumns = new System.Windows.Forms.DataGridView();
            this.lbWorkflows = new System.Windows.Forms.ListBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.dgCaseColumns = new System.Windows.Forms.DataGridView();
            this.txtShortcut = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtCaseName = new System.Windows.Forms.TextBox();
            this.btnDeleteCase = new System.Windows.Forms.Button();
            this.btnNewCase = new System.Windows.Forms.Button();
            this.lbCases = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnDeleteTolerance = new System.Windows.Forms.Button();
            this.btnNewTolerance = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.txtToleranceName = new System.Windows.Forms.TextBox();
            this.lbTolerances = new System.Windows.Forms.ListBox();
            this.dgTolerance = new System.Windows.Forms.DataGridView();
            this.ctxRow = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.zeileLöschenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zwischenablageEinfügenToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.lblOriginalName = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.cbHeaders = new CheckComboBoxTest.CheckedComboBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReplaces)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.gbWorkflow.SuspendLayout();
            this.gbProcedure.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumns)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgCaseColumns)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgTolerance)).BeginInit();
            this.ctxRow.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1140, 438);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnDelete);
            this.tabPage1.Controls.Add(this.btnNew);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.ltbProcedures);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1132, 412);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Funktionen";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(276, 14);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 11;
            this.btnDelete.Text = "Löschen";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnNew
            // 
            this.btnNew.Location = new System.Drawing.Point(68, 14);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(75, 23);
            this.btnNew.TabIndex = 10;
            this.btnNew.Text = "Neu";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dgvReplaces);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtName);
            this.groupBox1.Location = new System.Drawing.Point(416, 51);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(305, 342);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Bearbeiten";
            // 
            // dgvReplaces
            // 
            this.dgvReplaces.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvReplaces.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvReplaces.Location = new System.Drawing.Point(22, 88);
            this.dgvReplaces.Name = "dgvReplaces";
            this.dgvReplaces.RowHeadersVisible = false;
            this.dgvReplaces.Size = new System.Drawing.Size(262, 226);
            this.dgvReplaces.TabIndex = 9;
            this.dgvReplaces.MouseClick += new System.Windows.Forms.MouseEventHandler(this.DataGridViewContext_MouseClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Bezeichnung";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(22, 53);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(262, 20);
            this.txtName.TabIndex = 1;
            this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
            // 
            // ltbProcedures
            // 
            this.ltbProcedures.FormattingEnabled = true;
            this.ltbProcedures.Location = new System.Drawing.Point(68, 51);
            this.ltbProcedures.Name = "ltbProcedures";
            this.ltbProcedures.Size = new System.Drawing.Size(283, 342);
            this.ltbProcedures.Sorted = true;
            this.ltbProcedures.TabIndex = 8;
            this.ltbProcedures.SelectedIndexChanged += new System.EventHandler(this.ltbProcedures_SelectedIndexChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.gbWorkflow);
            this.tabPage2.Controls.Add(this.button1);
            this.tabPage2.Controls.Add(this.button2);
            this.tabPage2.Controls.Add(this.gbProcedure);
            this.tabPage2.Controls.Add(this.lbWorkflows);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1132, 412);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Arbeitsabläufe";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // gbWorkflow
            // 
            this.gbWorkflow.Controls.Add(this.cmbProcedureType);
            this.gbWorkflow.Controls.Add(this.label4);
            this.gbWorkflow.Controls.Add(this.label2);
            this.gbWorkflow.Controls.Add(this.btnDown);
            this.gbWorkflow.Controls.Add(this.btnUp);
            this.gbWorkflow.Controls.Add(this.lbProcedures);
            this.gbWorkflow.Controls.Add(this.button4);
            this.gbWorkflow.Controls.Add(this.btnAddProcedureToWorkflow);
            this.gbWorkflow.Controls.Add(this.lbUsedProcedures);
            this.gbWorkflow.Controls.Add(this.label1);
            this.gbWorkflow.Controls.Add(this.txtWorkflow);
            this.gbWorkflow.Location = new System.Drawing.Point(242, 50);
            this.gbWorkflow.Name = "gbWorkflow";
            this.gbWorkflow.Size = new System.Drawing.Size(601, 342);
            this.gbWorkflow.TabIndex = 12;
            this.gbWorkflow.TabStop = false;
            this.gbWorkflow.Text = "Arbeitsablauf";
            // 
            // cmbProcedureType
            // 
            this.cmbProcedureType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProcedureType.FormattingEnabled = true;
            this.cmbProcedureType.Items.AddRange(new object[] {
            "Benutzer",
            "System",
            "Duplikat"});
            this.cmbProcedureType.Location = new System.Drawing.Point(21, 73);
            this.cmbProcedureType.Name = "cmbProcedureType";
            this.cmbProcedureType.Size = new System.Drawing.Size(222, 21);
            this.cmbProcedureType.TabIndex = 15;
            this.cmbProcedureType.SelectedIndexChanged += new System.EventHandler(this.cmbProcedureType_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(356, 97);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(130, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Zugewiesene Funktionen:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 97);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Verfügbare Funktionen:";
            // 
            // btnDown
            // 
            this.btnDown.Location = new System.Drawing.Point(506, 60);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(75, 23);
            this.btnDown.TabIndex = 12;
            this.btnDown.Text = "Runter";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnUp
            // 
            this.btnUp.Location = new System.Drawing.Point(359, 62);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(75, 23);
            this.btnUp.TabIndex = 11;
            this.btnUp.Text = "Hoch";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // lbProcedures
            // 
            this.lbProcedures.FormattingEnabled = true;
            this.lbProcedures.Location = new System.Drawing.Point(21, 113);
            this.lbProcedures.Name = "lbProcedures";
            this.lbProcedures.Size = new System.Drawing.Size(222, 212);
            this.lbProcedures.TabIndex = 10;
            this.lbProcedures.DoubleClick += new System.EventHandler(this.btnAddProcedureToWorkflow_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(263, 178);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 9;
            this.button4.Text = "<<";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // btnAddProcedureToWorkflow
            // 
            this.btnAddProcedureToWorkflow.Location = new System.Drawing.Point(263, 133);
            this.btnAddProcedureToWorkflow.Name = "btnAddProcedureToWorkflow";
            this.btnAddProcedureToWorkflow.Size = new System.Drawing.Size(75, 23);
            this.btnAddProcedureToWorkflow.TabIndex = 8;
            this.btnAddProcedureToWorkflow.Text = ">>";
            this.btnAddProcedureToWorkflow.UseVisualStyleBackColor = true;
            this.btnAddProcedureToWorkflow.Click += new System.EventHandler(this.btnAddProcedureToWorkflow_Click);
            // 
            // lbUsedProcedures
            // 
            this.lbUsedProcedures.FormattingEnabled = true;
            this.lbUsedProcedures.Location = new System.Drawing.Point(359, 113);
            this.lbUsedProcedures.Name = "lbUsedProcedures";
            this.lbUsedProcedures.Size = new System.Drawing.Size(222, 212);
            this.lbUsedProcedures.TabIndex = 7;
            this.lbUsedProcedures.SelectedIndexChanged += new System.EventHandler(this.lbUsedProcedures_SelectedIndexChanged);
            this.lbUsedProcedures.DoubleClick += new System.EventHandler(this.button4_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Bezeichnung";
            // 
            // txtWorkflow
            // 
            this.txtWorkflow.Location = new System.Drawing.Point(21, 38);
            this.txtWorkflow.Name = "txtWorkflow";
            this.txtWorkflow.Size = new System.Drawing.Size(222, 20);
            this.txtWorkflow.TabIndex = 6;
            this.txtWorkflow.TextChanged += new System.EventHandler(this.txtWorkflow_TextChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(154, 20);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 11;
            this.button1.Text = "Löschen";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnDeleteWorkflow_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(8, 21);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 10;
            this.button2.Text = "Neu";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.btnNewWorkflow_Click);
            // 
            // gbProcedure
            // 
            this.gbProcedure.Controls.Add(this.label8);
            this.gbProcedure.Controls.Add(this.lblOriginalName);
            this.gbProcedure.Controls.Add(this.label7);
            this.gbProcedure.Controls.Add(this.txtWorkProcName);
            this.gbProcedure.Controls.Add(this.lblNewColumn);
            this.gbProcedure.Controls.Add(this.cbHeaders);
            this.gbProcedure.Controls.Add(this.lblHeaders);
            this.gbProcedure.Controls.Add(this.txtFormula);
            this.gbProcedure.Controls.Add(this.lblFormula);
            this.gbProcedure.Controls.Add(this.txtNewColumn);
            this.gbProcedure.Controls.Add(this.cbNewColumn);
            this.gbProcedure.Controls.Add(this.dgvColumns);
            this.gbProcedure.Location = new System.Drawing.Point(856, 6);
            this.gbProcedure.Name = "gbProcedure";
            this.gbProcedure.Size = new System.Drawing.Size(268, 386);
            this.gbProcedure.TabIndex = 9;
            this.gbProcedure.TabStop = false;
            this.gbProcedure.Text = "Spaltenangabe";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 33);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(100, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "Name der Funktion:";
            // 
            // txtWorkProcName
            // 
            this.txtWorkProcName.Location = new System.Drawing.Point(6, 48);
            this.txtWorkProcName.Name = "txtWorkProcName";
            this.txtWorkProcName.Size = new System.Drawing.Size(256, 20);
            this.txtWorkProcName.TabIndex = 8;
            this.txtWorkProcName.TextChanged += new System.EventHandler(this.txtWorkProcName_TextChanged);
            // 
            // lblNewColumn
            // 
            this.lblNewColumn.AutoSize = true;
            this.lblNewColumn.Location = new System.Drawing.Point(6, 88);
            this.lblNewColumn.Name = "lblNewColumn";
            this.lblNewColumn.Size = new System.Drawing.Size(89, 13);
            this.lblNewColumn.TabIndex = 7;
            this.lblNewColumn.Text = "Name der Spalte:";
            // 
            // lblHeaders
            // 
            this.lblHeaders.AutoSize = true;
            this.lblHeaders.Location = new System.Drawing.Point(3, 128);
            this.lblHeaders.Name = "lblHeaders";
            this.lblHeaders.Size = new System.Drawing.Size(107, 13);
            this.lblHeaders.TabIndex = 5;
            this.lblHeaders.Text = "Auswahl der Spalten:";
            // 
            // txtFormula
            // 
            this.txtFormula.Location = new System.Drawing.Point(6, 206);
            this.txtFormula.Name = "txtFormula";
            this.txtFormula.Size = new System.Drawing.Size(256, 20);
            this.txtFormula.TabIndex = 4;
            this.txtFormula.TextChanged += new System.EventHandler(this.txtFormula_TextChanged);
            // 
            // lblFormula
            // 
            this.lblFormula.AutoSize = true;
            this.lblFormula.Location = new System.Drawing.Point(3, 190);
            this.lblFormula.Name = "lblFormula";
            this.lblFormula.Size = new System.Drawing.Size(42, 13);
            this.lblFormula.TabIndex = 3;
            this.lblFormula.Text = "Format:";
            // 
            // txtNewColumn
            // 
            this.txtNewColumn.Location = new System.Drawing.Point(6, 102);
            this.txtNewColumn.Name = "txtNewColumn";
            this.txtNewColumn.ReadOnly = true;
            this.txtNewColumn.Size = new System.Drawing.Size(256, 20);
            this.txtNewColumn.TabIndex = 2;
            this.txtNewColumn.TextChanged += new System.EventHandler(this.txtNewColumn_TextChanged);
            // 
            // cbNewColumn
            // 
            this.cbNewColumn.AutoSize = true;
            this.cbNewColumn.Location = new System.Drawing.Point(6, 73);
            this.cbNewColumn.Name = "cbNewColumn";
            this.cbNewColumn.Size = new System.Drawing.Size(193, 17);
            this.cbNewColumn.TabIndex = 1;
            this.cbNewColumn.Text = "Ergebnis in neue Spalte schreiben?";
            this.cbNewColumn.UseVisualStyleBackColor = true;
            this.cbNewColumn.CheckedChanged += new System.EventHandler(this.cbNewColumn_CheckedChanged);
            // 
            // dgvColumns
            // 
            this.dgvColumns.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvColumns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvColumns.Location = new System.Drawing.Point(6, 144);
            this.dgvColumns.Name = "dgvColumns";
            this.dgvColumns.RowHeadersVisible = false;
            this.dgvColumns.Size = new System.Drawing.Size(256, 225);
            this.dgvColumns.TabIndex = 0;
            this.dgvColumns.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvColumns_CellEndEdit);
            this.dgvColumns.MouseClick += new System.Windows.Forms.MouseEventHandler(this.DataGridViewContext_MouseClick);
            // 
            // lbWorkflows
            // 
            this.lbWorkflows.FormattingEnabled = true;
            this.lbWorkflows.Location = new System.Drawing.Point(8, 50);
            this.lbWorkflows.Name = "lbWorkflows";
            this.lbWorkflows.Size = new System.Drawing.Size(221, 342);
            this.lbWorkflows.TabIndex = 8;
            this.lbWorkflows.SelectedIndexChanged += new System.EventHandler(this.lbWorkflows_SelectedIndexChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupBox3);
            this.tabPage3.Controls.Add(this.groupBox2);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1132, 412);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Duplikate";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.groupBox4);
            this.groupBox3.Controls.Add(this.txtShortcut);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.txtCaseName);
            this.groupBox3.Controls.Add(this.btnDeleteCase);
            this.groupBox3.Controls.Add(this.btnNewCase);
            this.groupBox3.Controls.Add(this.lbCases);
            this.groupBox3.Location = new System.Drawing.Point(477, 18);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(647, 375);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Fälle";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.dgCaseColumns);
            this.groupBox4.Location = new System.Drawing.Point(222, 81);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(419, 294);
            this.groupBox4.TabIndex = 20;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Spalten";
            // 
            // dgCaseColumns
            // 
            this.dgCaseColumns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgCaseColumns.Location = new System.Drawing.Point(6, 19);
            this.dgCaseColumns.Name = "dgCaseColumns";
            this.dgCaseColumns.Size = new System.Drawing.Size(407, 263);
            this.dgCaseColumns.TabIndex = 0;
            this.dgCaseColumns.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgCaseColumns_CellEndEdit);
            this.dgCaseColumns.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dgCaseColumns_EditingControlShowing);
            this.dgCaseColumns.MouseClick += new System.Windows.Forms.MouseEventHandler(this.DataGridViewContext_MouseClick);
            // 
            // txtShortcut
            // 
            this.txtShortcut.Location = new System.Drawing.Point(439, 47);
            this.txtShortcut.Name = "txtShortcut";
            this.txtShortcut.Size = new System.Drawing.Size(57, 20);
            this.txtShortcut.TabIndex = 16;
            this.txtShortcut.TextChanged += new System.EventHandler(this.txtShortcut_TextChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(436, 24);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(39, 13);
            this.label10.TabIndex = 15;
            this.label10.Text = "Kürzel:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(235, 24);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "Bezeichnung:";
            // 
            // txtCaseName
            // 
            this.txtCaseName.Location = new System.Drawing.Point(238, 47);
            this.txtCaseName.Name = "txtCaseName";
            this.txtCaseName.Size = new System.Drawing.Size(175, 20);
            this.txtCaseName.TabIndex = 6;
            this.txtCaseName.TextChanged += new System.EventHandler(this.txtCaseName_TextChanged);
            // 
            // btnDeleteCase
            // 
            this.btnDeleteCase.Location = new System.Drawing.Point(136, 18);
            this.btnDeleteCase.Name = "btnDeleteCase";
            this.btnDeleteCase.Size = new System.Drawing.Size(75, 23);
            this.btnDeleteCase.TabIndex = 5;
            this.btnDeleteCase.Text = "Löschen";
            this.btnDeleteCase.UseVisualStyleBackColor = true;
            this.btnDeleteCase.Click += new System.EventHandler(this.btnDeleteCase_Click);
            // 
            // btnNewCase
            // 
            this.btnNewCase.Location = new System.Drawing.Point(28, 19);
            this.btnNewCase.Name = "btnNewCase";
            this.btnNewCase.Size = new System.Drawing.Size(75, 23);
            this.btnNewCase.TabIndex = 4;
            this.btnNewCase.Text = "Neu";
            this.btnNewCase.UseVisualStyleBackColor = true;
            this.btnNewCase.Click += new System.EventHandler(this.btnNewCase_Click);
            // 
            // lbCases
            // 
            this.lbCases.FormattingEnabled = true;
            this.lbCases.Location = new System.Drawing.Point(28, 47);
            this.lbCases.Name = "lbCases";
            this.lbCases.Size = new System.Drawing.Size(183, 316);
            this.lbCases.TabIndex = 3;
            this.lbCases.SelectedIndexChanged += new System.EventHandler(this.lbCases_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnDeleteTolerance);
            this.groupBox2.Controls.Add(this.btnNewTolerance);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.txtToleranceName);
            this.groupBox2.Controls.Add(this.lbTolerances);
            this.groupBox2.Controls.Add(this.dgTolerance);
            this.groupBox2.Location = new System.Drawing.Point(8, 18);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(448, 375);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Toleranzen";
            // 
            // btnDeleteTolerance
            // 
            this.btnDeleteTolerance.Location = new System.Drawing.Point(132, 18);
            this.btnDeleteTolerance.Name = "btnDeleteTolerance";
            this.btnDeleteTolerance.Size = new System.Drawing.Size(75, 23);
            this.btnDeleteTolerance.TabIndex = 6;
            this.btnDeleteTolerance.Text = "Löschen";
            this.btnDeleteTolerance.UseVisualStyleBackColor = true;
            this.btnDeleteTolerance.Click += new System.EventHandler(this.btnDeleteTolerance_Click);
            // 
            // btnNewTolerance
            // 
            this.btnNewTolerance.Location = new System.Drawing.Point(18, 19);
            this.btnNewTolerance.Name = "btnNewTolerance";
            this.btnNewTolerance.Size = new System.Drawing.Size(75, 23);
            this.btnNewTolerance.TabIndex = 5;
            this.btnNewTolerance.Text = "Neu";
            this.btnNewTolerance.UseVisualStyleBackColor = true;
            this.btnNewTolerance.Click += new System.EventHandler(this.btnNewTolerance_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(245, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Bezeichnung:";
            // 
            // txtToleranceName
            // 
            this.txtToleranceName.Location = new System.Drawing.Point(248, 47);
            this.txtToleranceName.Name = "txtToleranceName";
            this.txtToleranceName.Size = new System.Drawing.Size(175, 20);
            this.txtToleranceName.TabIndex = 3;
            this.txtToleranceName.TextChanged += new System.EventHandler(this.txtToleranceName_TextChanged);
            // 
            // lbTolerances
            // 
            this.lbTolerances.FormattingEnabled = true;
            this.lbTolerances.Location = new System.Drawing.Point(18, 47);
            this.lbTolerances.Name = "lbTolerances";
            this.lbTolerances.Size = new System.Drawing.Size(189, 316);
            this.lbTolerances.TabIndex = 0;
            this.lbTolerances.SelectedIndexChanged += new System.EventHandler(this.lbTolerances_SelectedIndexChanged);
            // 
            // dgTolerance
            // 
            this.dgTolerance.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgTolerance.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgTolerance.Location = new System.Drawing.Point(248, 81);
            this.dgTolerance.Name = "dgTolerance";
            this.dgTolerance.RowHeadersVisible = false;
            this.dgTolerance.Size = new System.Drawing.Size(175, 282);
            this.dgTolerance.TabIndex = 2;
            this.dgTolerance.MouseClick += new System.Windows.Forms.MouseEventHandler(this.DataGridViewContext_MouseClick);
            // 
            // ctxRow
            // 
            this.ctxRow.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zeileLöschenToolStripMenuItem,
            this.zwischenablageEinfügenToolStripMenuItem1});
            this.ctxRow.Name = "ctxRow";
            this.ctxRow.Size = new System.Drawing.Size(210, 48);
            // 
            // zeileLöschenToolStripMenuItem
            // 
            this.zeileLöschenToolStripMenuItem.Name = "zeileLöschenToolStripMenuItem";
            this.zeileLöschenToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.zeileLöschenToolStripMenuItem.Text = "Zeile löschen";
            // 
            // zwischenablageEinfügenToolStripMenuItem1
            // 
            this.zwischenablageEinfügenToolStripMenuItem1.Name = "zwischenablageEinfügenToolStripMenuItem1";
            this.zwischenablageEinfügenToolStripMenuItem1.Size = new System.Drawing.Size(209, 22);
            this.zwischenablageEinfügenToolStripMenuItem1.Text = "Zwischenablage einfügen";
            // 
            // lblOriginalName
            // 
            this.lblOriginalName.AutoSize = true;
            this.lblOriginalName.Location = new System.Drawing.Point(141, 15);
            this.lblOriginalName.Name = "lblOriginalName";
            this.lblOriginalName.Size = new System.Drawing.Size(0, 13);
            this.lblOriginalName.TabIndex = 10;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 15);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(129, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Originaler Funktionsname:";
            // 
            // cbHeaders
            // 
            this.cbHeaders.CheckOnClick = true;
            this.cbHeaders.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cbHeaders.DropDownHeight = 1;
            this.cbHeaders.FormattingEnabled = true;
            this.cbHeaders.IntegralHeight = false;
            this.cbHeaders.Location = new System.Drawing.Point(6, 144);
            this.cbHeaders.Name = "cbHeaders";
            this.cbHeaders.Size = new System.Drawing.Size(256, 21);
            this.cbHeaders.TabIndex = 6;
            this.cbHeaders.ValueSeparator = ", ";
            this.cbHeaders.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cbHeaders_ItemCheck);
            // 
            // Administration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1140, 438);
            this.Controls.Add(this.tabControl1);
            this.Name = "Administration";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Verwaltung";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Administration_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReplaces)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.gbWorkflow.ResumeLayout(false);
            this.gbWorkflow.PerformLayout();
            this.gbProcedure.ResumeLayout(false);
            this.gbProcedure.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumns)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgCaseColumns)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgTolerance)).EndInit();
            this.ctxRow.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView dgvReplaces;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.ListBox ltbProcedures;
        private System.Windows.Forms.GroupBox gbWorkflow;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtWorkflow;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox gbProcedure;
        private System.Windows.Forms.DataGridView dgvColumns;
        private System.Windows.Forms.ListBox lbWorkflows;
        private System.Windows.Forms.ContextMenuStrip ctxRow;
        private System.Windows.Forms.ToolStripMenuItem zeileLöschenToolStripMenuItem;
        private System.Windows.Forms.ListBox lbProcedures;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button btnAddProcedureToWorkflow;
        private System.Windows.Forms.ListBox lbUsedProcedures;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbNewColumn;
        private System.Windows.Forms.TextBox txtNewColumn;
        private System.Windows.Forms.Label lblFormula;
        private System.Windows.Forms.TextBox txtFormula;
        private System.Windows.Forms.Label lblHeaders;
        private CheckComboBoxTest.CheckedComboBox cbHeaders;
        private System.Windows.Forms.Label lblNewColumn;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txtShortcut;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtCaseName;
        private System.Windows.Forms.Button btnDeleteCase;
        private System.Windows.Forms.Button btnNewCase;
        private System.Windows.Forms.ListBox lbCases;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnDeleteTolerance;
        private System.Windows.Forms.Button btnNewTolerance;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtToleranceName;
        private System.Windows.Forms.ListBox lbTolerances;
        private System.Windows.Forms.DataGridView dgTolerance;
        private System.Windows.Forms.DataGridView dgCaseColumns;
        private System.Windows.Forms.ComboBox cmbProcedureType;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtWorkProcName;
        private System.Windows.Forms.ToolStripMenuItem zwischenablageEinfügenToolStripMenuItem1;
        private System.Windows.Forms.Label lblOriginalName;
        private System.Windows.Forms.Label label8;
    }
}