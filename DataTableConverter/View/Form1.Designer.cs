namespace DataTableConverter
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.öffnenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.öffnenToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.speichernToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.speichernToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.cSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dBASEToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.excelToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.benutzerdefiniertesSpeichernToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.postwurfToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nachWertInSpalteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bearbeitenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rückgängigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wiederholenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabellenZusammenfügenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabelleHinzufügenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.überschriftEinlesenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zählenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zeilenZusammenfügenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verwaltungToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.funktionenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trimToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zeilenZusammenfügenToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.großKleinschreibungToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rundenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zeichenAuffüllenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ersetzenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.arbeitsablaufToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.duplikateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortierenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pgbLoading = new System.Windows.Forms.ProgressBar();
            this.ctxBody = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.spalteHinzufügenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxHeader = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.spalteUmbenennenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spalteLöschenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spalteHinzufügenToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.textErsetzenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxRow = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.zeileLöschenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zeileEinfügenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spalteEinfügenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblFilename = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblRows = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.contextGlobal = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteRowItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertRowItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clipboardItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dgTable = new DataTableConverter.DataGridViewDoubleBuffered();
            this.menuStrip1.SuspendLayout();
            this.ctxBody.SuspendLayout();
            this.ctxHeader.SuspendLayout();
            this.ctxRow.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.contextGlobal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgTable)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.öffnenToolStripMenuItem,
            this.bearbeitenToolStripMenuItem,
            this.verwaltungToolStripMenuItem,
            this.funktionenToolStripMenuItem,
            this.ersetzenToolStripMenuItem,
            this.arbeitsablaufToolStripMenuItem,
            this.duplikateToolStripMenuItem,
            this.sortierenToolStripMenuItem,
            this.updateToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // öffnenToolStripMenuItem
            // 
            this.öffnenToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.öffnenToolStripMenuItem1,
            this.speichernToolStripMenuItem,
            this.speichernToolStripMenuItem1,
            this.benutzerdefiniertesSpeichernToolStripMenuItem});
            this.öffnenToolStripMenuItem.Name = "öffnenToolStripMenuItem";
            this.öffnenToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.öffnenToolStripMenuItem.Text = "Datei";
            // 
            // öffnenToolStripMenuItem1
            // 
            this.öffnenToolStripMenuItem1.Name = "öffnenToolStripMenuItem1";
            this.öffnenToolStripMenuItem1.Size = new System.Drawing.Size(230, 22);
            this.öffnenToolStripMenuItem1.Text = "Öffnen";
            // 
            // speichernToolStripMenuItem
            // 
            this.speichernToolStripMenuItem.Name = "speichernToolStripMenuItem";
            this.speichernToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.speichernToolStripMenuItem.Text = "Speichern";
            this.speichernToolStripMenuItem.Click += new System.EventHandler(this.speichernToolStripMenuItem_Click_1);
            // 
            // speichernToolStripMenuItem1
            // 
            this.speichernToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cSVToolStripMenuItem,
            this.dBASEToolStripMenuItem1,
            this.excelToolStripMenuItem1});
            this.speichernToolStripMenuItem1.Name = "speichernToolStripMenuItem1";
            this.speichernToolStripMenuItem1.Size = new System.Drawing.Size(230, 22);
            this.speichernToolStripMenuItem1.Text = "Speichern als";
            // 
            // cSVToolStripMenuItem
            // 
            this.cSVToolStripMenuItem.Name = "cSVToolStripMenuItem";
            this.cSVToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.cSVToolStripMenuItem.Text = "CSV";
            // 
            // dBASEToolStripMenuItem1
            // 
            this.dBASEToolStripMenuItem1.Name = "dBASEToolStripMenuItem1";
            this.dBASEToolStripMenuItem1.Size = new System.Drawing.Size(109, 22);
            this.dBASEToolStripMenuItem1.Text = "DBASE";
            // 
            // excelToolStripMenuItem1
            // 
            this.excelToolStripMenuItem1.Name = "excelToolStripMenuItem1";
            this.excelToolStripMenuItem1.Size = new System.Drawing.Size(109, 22);
            this.excelToolStripMenuItem1.Text = "Excel";
            // 
            // benutzerdefiniertesSpeichernToolStripMenuItem
            // 
            this.benutzerdefiniertesSpeichernToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.postwurfToolStripMenuItem,
            this.nachWertInSpalteToolStripMenuItem});
            this.benutzerdefiniertesSpeichernToolStripMenuItem.Name = "benutzerdefiniertesSpeichernToolStripMenuItem";
            this.benutzerdefiniertesSpeichernToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.benutzerdefiniertesSpeichernToolStripMenuItem.Text = "Benutzerdefiniertes Speichern";
            // 
            // postwurfToolStripMenuItem
            // 
            this.postwurfToolStripMenuItem.Name = "postwurfToolStripMenuItem";
            this.postwurfToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.postwurfToolStripMenuItem.Text = "Postwurf";
            this.postwurfToolStripMenuItem.Click += new System.EventHandler(this.postwurfToolStripMenuItem_Click);
            // 
            // nachWertInSpalteToolStripMenuItem
            // 
            this.nachWertInSpalteToolStripMenuItem.Name = "nachWertInSpalteToolStripMenuItem";
            this.nachWertInSpalteToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.nachWertInSpalteToolStripMenuItem.Text = "Nach Wert in Spalte";
            this.nachWertInSpalteToolStripMenuItem.Click += new System.EventHandler(this.nachWertInSpalteToolStripMenuItem_Click);
            // 
            // bearbeitenToolStripMenuItem
            // 
            this.bearbeitenToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rückgängigToolStripMenuItem,
            this.wiederholenToolStripMenuItem,
            this.tabellenZusammenfügenToolStripMenuItem,
            this.tabelleHinzufügenToolStripMenuItem,
            this.überschriftEinlesenToolStripMenuItem,
            this.zählenToolStripMenuItem,
            this.zeilenZusammenfügenToolStripMenuItem});
            this.bearbeitenToolStripMenuItem.Name = "bearbeitenToolStripMenuItem";
            this.bearbeitenToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
            this.bearbeitenToolStripMenuItem.Text = "Bearbeiten";
            // 
            // rückgängigToolStripMenuItem
            // 
            this.rückgängigToolStripMenuItem.Name = "rückgängigToolStripMenuItem";
            this.rückgängigToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.rückgängigToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.rückgängigToolStripMenuItem.Text = "Rückgängig";
            this.rückgängigToolStripMenuItem.Click += new System.EventHandler(this.rückgängigToolStripMenuItem_Click);
            // 
            // wiederholenToolStripMenuItem
            // 
            this.wiederholenToolStripMenuItem.Name = "wiederholenToolStripMenuItem";
            this.wiederholenToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.wiederholenToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.wiederholenToolStripMenuItem.Text = "Wiederholen";
            this.wiederholenToolStripMenuItem.Click += new System.EventHandler(this.wiederholenToolStripMenuItem_Click);
            // 
            // tabellenZusammenfügenToolStripMenuItem
            // 
            this.tabellenZusammenfügenToolStripMenuItem.Name = "tabellenZusammenfügenToolStripMenuItem";
            this.tabellenZusammenfügenToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.tabellenZusammenfügenToolStripMenuItem.Text = "Spaten aus Tabelle hinzufügen";
            this.tabellenZusammenfügenToolStripMenuItem.Click += new System.EventHandler(this.tabellenZusammenfügenToolStripMenuItem_Click);
            // 
            // tabelleHinzufügenToolStripMenuItem
            // 
            this.tabelleHinzufügenToolStripMenuItem.Name = "tabelleHinzufügenToolStripMenuItem";
            this.tabelleHinzufügenToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.tabelleHinzufügenToolStripMenuItem.Text = "Zeilen aus Tabelle hinzufügen";
            this.tabelleHinzufügenToolStripMenuItem.Click += new System.EventHandler(this.tabelleHinzufügenToolStripMenuItem_Click);
            // 
            // überschriftEinlesenToolStripMenuItem
            // 
            this.überschriftEinlesenToolStripMenuItem.Name = "überschriftEinlesenToolStripMenuItem";
            this.überschriftEinlesenToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.überschriftEinlesenToolStripMenuItem.Text = "Überschrift einlesen";
            this.überschriftEinlesenToolStripMenuItem.Click += new System.EventHandler(this.überschriftenEinlesenToolStripMenuItem_Click);
            // 
            // zählenToolStripMenuItem
            // 
            this.zählenToolStripMenuItem.Name = "zählenToolStripMenuItem";
            this.zählenToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.zählenToolStripMenuItem.Text = "Zählen";
            this.zählenToolStripMenuItem.Click += new System.EventHandler(this.zählenToolStripMenuItem_Click);
            // 
            // zeilenZusammenfügenToolStripMenuItem
            // 
            this.zeilenZusammenfügenToolStripMenuItem.Name = "zeilenZusammenfügenToolStripMenuItem";
            this.zeilenZusammenfügenToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.zeilenZusammenfügenToolStripMenuItem.Text = "Zeilen zusammenfassen";
            this.zeilenZusammenfügenToolStripMenuItem.Click += new System.EventHandler(this.zeilenZusammenfügenToolStripMenuItem_Click_1);
            // 
            // verwaltungToolStripMenuItem
            // 
            this.verwaltungToolStripMenuItem.Name = "verwaltungToolStripMenuItem";
            this.verwaltungToolStripMenuItem.Size = new System.Drawing.Size(78, 20);
            this.verwaltungToolStripMenuItem.Text = "Verwaltung";
            this.verwaltungToolStripMenuItem.Click += new System.EventHandler(this.verwaltungToolStripMenuItem_Click);
            // 
            // funktionenToolStripMenuItem
            // 
            this.funktionenToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trimToolStripMenuItem,
            this.zeilenZusammenfügenToolStripMenuItem1,
            this.großKleinschreibungToolStripMenuItem,
            this.rundenToolStripMenuItem,
            this.zeichenAuffüllenToolStripMenuItem});
            this.funktionenToolStripMenuItem.Name = "funktionenToolStripMenuItem";
            this.funktionenToolStripMenuItem.Size = new System.Drawing.Size(79, 20);
            this.funktionenToolStripMenuItem.Text = "Funktionen";
            // 
            // trimToolStripMenuItem
            // 
            this.trimToolStripMenuItem.Name = "trimToolStripMenuItem";
            this.trimToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.trimToolStripMenuItem.Text = "Trim";
            // 
            // zeilenZusammenfügenToolStripMenuItem1
            // 
            this.zeilenZusammenfügenToolStripMenuItem1.Name = "zeilenZusammenfügenToolStripMenuItem1";
            this.zeilenZusammenfügenToolStripMenuItem1.Size = new System.Drawing.Size(205, 22);
            this.zeilenZusammenfügenToolStripMenuItem1.Text = "Spalten zusammenfügen";
            this.zeilenZusammenfügenToolStripMenuItem1.Click += new System.EventHandler(this.zeilenZusammenfügenToolStripMenuItem_Click);
            // 
            // großKleinschreibungToolStripMenuItem
            // 
            this.großKleinschreibungToolStripMenuItem.Name = "großKleinschreibungToolStripMenuItem";
            this.großKleinschreibungToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.großKleinschreibungToolStripMenuItem.Text = "Groß-/Kleinschreibung";
            this.großKleinschreibungToolStripMenuItem.Click += new System.EventHandler(this.großKleinschreibungToolStripMenuItem_Click);
            // 
            // rundenToolStripMenuItem
            // 
            this.rundenToolStripMenuItem.Name = "rundenToolStripMenuItem";
            this.rundenToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.rundenToolStripMenuItem.Text = "Runden";
            this.rundenToolStripMenuItem.Click += new System.EventHandler(this.rundenToolStripMenuItem_Click);
            // 
            // zeichenAuffüllenToolStripMenuItem
            // 
            this.zeichenAuffüllenToolStripMenuItem.Name = "zeichenAuffüllenToolStripMenuItem";
            this.zeichenAuffüllenToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.zeichenAuffüllenToolStripMenuItem.Text = "Zeichen auffüllen";
            this.zeichenAuffüllenToolStripMenuItem.Click += new System.EventHandler(this.zeichenAuffüllenToolStripMenuItem_Click);
            // 
            // ersetzenToolStripMenuItem
            // 
            this.ersetzenToolStripMenuItem.Name = "ersetzenToolStripMenuItem";
            this.ersetzenToolStripMenuItem.Size = new System.Drawing.Size(117, 20);
            this.ersetzenToolStripMenuItem.Text = "Suchen && Ersetzen";
            // 
            // arbeitsablaufToolStripMenuItem
            // 
            this.arbeitsablaufToolStripMenuItem.Name = "arbeitsablaufToolStripMenuItem";
            this.arbeitsablaufToolStripMenuItem.Size = new System.Drawing.Size(95, 20);
            this.arbeitsablaufToolStripMenuItem.Text = "Arbeitsabläufe";
            // 
            // duplikateToolStripMenuItem
            // 
            this.duplikateToolStripMenuItem.Name = "duplikateToolStripMenuItem";
            this.duplikateToolStripMenuItem.Size = new System.Drawing.Size(69, 20);
            this.duplikateToolStripMenuItem.Text = "Duplikate";
            // 
            // sortierenToolStripMenuItem
            // 
            this.sortierenToolStripMenuItem.Name = "sortierenToolStripMenuItem";
            this.sortierenToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.sortierenToolStripMenuItem.Text = "Sortieren";
            this.sortierenToolStripMenuItem.Click += new System.EventHandler(this.sortierenToolStripMenuItem_Click);
            // 
            // updateToolStripMenuItem
            // 
            this.updateToolStripMenuItem.Name = "updateToolStripMenuItem";
            this.updateToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.updateToolStripMenuItem.Text = "Update";
            this.updateToolStripMenuItem.Click += new System.EventHandler(this.updateToolStripMenuItem_Click);
            // 
            // pgbLoading
            // 
            this.pgbLoading.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pgbLoading.Location = new System.Drawing.Point(0, 402);
            this.pgbLoading.MarqueeAnimationSpeed = 10;
            this.pgbLoading.Name = "pgbLoading";
            this.pgbLoading.Size = new System.Drawing.Size(800, 23);
            this.pgbLoading.TabIndex = 2;
            // 
            // ctxBody
            // 
            this.ctxBody.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.spalteHinzufügenToolStripMenuItem});
            this.ctxBody.Name = "contextMenuStrip1";
            this.ctxBody.Size = new System.Drawing.Size(170, 26);
            // 
            // spalteHinzufügenToolStripMenuItem
            // 
            this.spalteHinzufügenToolStripMenuItem.Name = "spalteHinzufügenToolStripMenuItem";
            this.spalteHinzufügenToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.spalteHinzufügenToolStripMenuItem.Text = "Spalte hinzufügen";
            this.spalteHinzufügenToolStripMenuItem.Click += new System.EventHandler(this.spalteHinzufügenToolStripMenuItem_Click);
            // 
            // ctxHeader
            // 
            this.ctxHeader.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.spalteUmbenennenToolStripMenuItem,
            this.spalteLöschenToolStripMenuItem,
            this.spalteHinzufügenToolStripMenuItem1,
            this.textErsetzenToolStripMenuItem});
            this.ctxHeader.Name = "ctxHeader";
            this.ctxHeader.Size = new System.Drawing.Size(181, 92);
            // 
            // spalteUmbenennenToolStripMenuItem
            // 
            this.spalteUmbenennenToolStripMenuItem.Name = "spalteUmbenennenToolStripMenuItem";
            this.spalteUmbenennenToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.spalteUmbenennenToolStripMenuItem.Text = "Spalte umbenennen";
            this.spalteUmbenennenToolStripMenuItem.Click += new System.EventHandler(this.spalteUmbenennenToolStripMenuItem_Click);
            // 
            // spalteLöschenToolStripMenuItem
            // 
            this.spalteLöschenToolStripMenuItem.Name = "spalteLöschenToolStripMenuItem";
            this.spalteLöschenToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.spalteLöschenToolStripMenuItem.Text = "Spalte löschen";
            this.spalteLöschenToolStripMenuItem.Click += new System.EventHandler(this.spalteLöschenToolStripMenuItem_Click);
            // 
            // spalteHinzufügenToolStripMenuItem1
            // 
            this.spalteHinzufügenToolStripMenuItem1.Name = "spalteHinzufügenToolStripMenuItem1";
            this.spalteHinzufügenToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.spalteHinzufügenToolStripMenuItem1.Text = "Spalte hinzufügen";
            this.spalteHinzufügenToolStripMenuItem1.Click += new System.EventHandler(this.spalteHinzufügenToolStripMenuItem_Click);
            // 
            // textErsetzenToolStripMenuItem
            // 
            this.textErsetzenToolStripMenuItem.Name = "textErsetzenToolStripMenuItem";
            this.textErsetzenToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.textErsetzenToolStripMenuItem.Text = "Text ersetzen";
            this.textErsetzenToolStripMenuItem.Click += new System.EventHandler(this.textErsetzenToolStripMenuItem_Click);
            // 
            // ctxRow
            // 
            this.ctxRow.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zeileLöschenToolStripMenuItem,
            this.zeileEinfügenToolStripMenuItem,
            this.spalteEinfügenToolStripMenuItem});
            this.ctxRow.Name = "ctxRow";
            this.ctxRow.Size = new System.Drawing.Size(157, 70);
            // 
            // zeileLöschenToolStripMenuItem
            // 
            this.zeileLöschenToolStripMenuItem.Name = "zeileLöschenToolStripMenuItem";
            this.zeileLöschenToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.zeileLöschenToolStripMenuItem.Text = "Zeile löschen";
            this.zeileLöschenToolStripMenuItem.Click += new System.EventHandler(this.zeileLöschenToolStripMenuItem_Click);
            // 
            // zeileEinfügenToolStripMenuItem
            // 
            this.zeileEinfügenToolStripMenuItem.Name = "zeileEinfügenToolStripMenuItem";
            this.zeileEinfügenToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.zeileEinfügenToolStripMenuItem.Text = "Zeile einfügen";
            this.zeileEinfügenToolStripMenuItem.Click += new System.EventHandler(this.zeileEinfügenToolStripMenuItem_Click);
            // 
            // spalteEinfügenToolStripMenuItem
            // 
            this.spalteEinfügenToolStripMenuItem.Name = "spalteEinfügenToolStripMenuItem";
            this.spalteEinfügenToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.spalteEinfügenToolStripMenuItem.Text = "Spalte einfügen";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.lblFilename,
            this.toolStripStatusLabel3,
            this.lblRows,
            this.StatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 426);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 24);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(71, 19);
            this.toolStripStatusLabel1.Text = "Dateiname:";
            // 
            // lblFilename
            // 
            this.lblFilename.Name = "lblFilename";
            this.lblFilename.Size = new System.Drawing.Size(0, 19);
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.toolStripStatusLabel3.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.toolStripStatusLabel3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(48, 19);
            this.toolStripStatusLabel3.Text = "Zeilen:";
            // 
            // lblRows
            // 
            this.lblRows.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRows.Name = "lblRows";
            this.lblRows.Size = new System.Drawing.Size(0, 19);
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(0, 19);
            // 
            // contextGlobal
            // 
            this.contextGlobal.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteRowItem,
            this.insertRowItem,
            this.clipboardItem});
            this.contextGlobal.Name = "ctxRow";
            this.contextGlobal.Size = new System.Drawing.Size(210, 70);
            // 
            // deleteRowItem
            // 
            this.deleteRowItem.Name = "deleteRowItem";
            this.deleteRowItem.Size = new System.Drawing.Size(209, 22);
            this.deleteRowItem.Text = "Zeile löschen";
            // 
            // insertRowItem
            // 
            this.insertRowItem.Name = "insertRowItem";
            this.insertRowItem.Size = new System.Drawing.Size(209, 22);
            this.insertRowItem.Text = "Zeile einfügen";
            // 
            // clipboardItem
            // 
            this.clipboardItem.Name = "clipboardItem";
            this.clipboardItem.Size = new System.Drawing.Size(209, 22);
            this.clipboardItem.Text = "Zwischenablage einfügen";
            // 
            // dgTable
            // 
            this.dgTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            this.dgTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgTable.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgTable.Location = new System.Drawing.Point(0, 27);
            this.dgTable.Name = "dgTable";
            this.dgTable.RowHeadersWidth = 60;
            this.dgTable.Size = new System.Drawing.Size(800, 369);
            this.dgTable.TabIndex = 0;
            this.dgTable.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgTable_CellClick);
            this.dgTable.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgTable_CellPainting);
            this.dgTable.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dgTable_CellValidating);
            this.dgTable.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgTable_CellValueChanged);
            this.dgTable.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dgTable_RowPostPaint);
            this.dgTable.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.dgTable_RowsAdded);
            this.dgTable.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dgTable_MouseClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.pgbLoading);
            this.Controls.Add(this.dgTable);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Konvertierung";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ctxBody.ResumeLayout(false);
            this.ctxHeader.ResumeLayout(false);
            this.ctxRow.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.contextGlobal.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgTable)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion


        private DataGridViewDoubleBuffered dgTable;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem öffnenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem funktionenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem trimToolStripMenuItem;
        private System.Windows.Forms.ProgressBar pgbLoading;
        private System.Windows.Forms.ContextMenuStrip ctxBody;
        private System.Windows.Forms.ToolStripMenuItem spalteHinzufügenToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip ctxHeader;
        private System.Windows.Forms.ToolStripMenuItem spalteUmbenennenToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip ctxRow;
        private System.Windows.Forms.ToolStripMenuItem zeileLöschenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spalteLöschenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bearbeitenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rückgängigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wiederholenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem arbeitsablaufToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblFilename;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel lblRows;
        private System.Windows.Forms.ToolStripMenuItem zeilenZusammenfügenToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem tabellenZusammenfügenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spalteHinzufügenToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem verwaltungToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem öffnenToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem speichernToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem excelToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem dBASEToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem cSVToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem benutzerdefiniertesSpeichernToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem postwurfToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nachWertInSpalteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zählenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem duplikateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zeileEinfügenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tabelleHinzufügenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem überschriftEinlesenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortierenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spalteEinfügenToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextGlobal;
        private System.Windows.Forms.ToolStripMenuItem deleteRowItem;
        private System.Windows.Forms.ToolStripMenuItem insertRowItem;
        private System.Windows.Forms.ToolStripMenuItem clipboardItem;
        private System.Windows.Forms.ToolStripMenuItem großKleinschreibungToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rundenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem speichernToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zeilenZusammenfügenToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.ToolStripMenuItem ersetzenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zeichenAuffüllenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem textErsetzenToolStripMenuItem;
    }
}

