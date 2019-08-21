using CheckComboBoxTest;
using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using DataTableConverter.Classes.WorkProcs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class Administration : Form
    {
        private BindingList<Proc> bindingProcedure;
        private BindingList<WorkProc> bindingWorkProc;
        private BindingList<Work> bindingWorkflow;
        private BindingList<Tolerance> bindingTolerance;
        private BindingList<Case> bindingCase;
        private Dictionary<GroupBox, Type> gbState;
        private List<KeyValuePair<string, bool>> OrderList;
        internal List<Proc> Procedures { get; set; }
        internal List<Proc> SystemProc { get; set; }
        internal List<Proc> DuplicateProc { get; set; }
        internal List<Work> Workflows { get; set; }
        internal List<Tolerance> Tolerances { get; set; }
        internal List<Case> Cases { get; set; }
        private Proc selectedProc;
        private ViewHelper ViewHelper;
        private Dictionary<Type, Action<WorkProc>> assignControls;
        private MemoryStream ProceduresBefore, WorkflowsBefore, TolerancesBefore, CasesBefore;

        internal Administration(object[] headers, ContextMenuStrip ctxRow, List<Proc> procedures, List<Work> workflows, List<Case> cases, List<Tolerance> tolerances)
        {
            InitializeComponent();
            SetSize();
            AssignGroupBoxToEnum();
            SetHeaders(headers);
            SetOrderList();
            SetListBoxStyle();
            
            CmBRound.SelectedIndex = 0;
            CmBRound.SelectedIndexChanged += CmBRound_SelectedIndexChanged;
            cmbProcedureType.SelectedIndex = 0;

            SetDataBefore(procedures, cases, workflows, tolerances);

            LoadProcedures(procedures);
            LoadTolerances(tolerances);
            LoadCases(cases);
            LoadWorkflows(workflows);

            ViewHelper = new ViewHelper(ctxRow, lbUsedProcedures_SelectedIndexChanged, Workflows);
            
            GenerateProceduresForWorkflow();
            LoadProceduresWorkflow(false);
            SetGroupBoxVisibility(null);
            AddContextMenuAndDataGridViewStyle();
            RestoreSplitterDistance();
            SetColors();
        }

        private void SetListBoxStyle()
        {
            ListBox[] listBoxes = new ListBox[]
            {
                lbCases,
                lbProcedures,
                lbTolerances,
                lbUsedProcedures,
                lbWorkflows,
                ltbProcedures
            };
            foreach(ListBox listBox in listBoxes)
            {
                ViewHelper.SetListBoxStyle(listBox);
            }
        }

        private void SetDataBefore(List<Proc> procedures, List<Case> cases, List<Work> workflows, List<Tolerance> tolerances)
        {
            SetProceduresBefore(procedures);
            SetCasesBefore(cases);
            SetWorkflowsBefore(workflows);
            SetTolerancesBefore(tolerances);
        }

        private void SetProceduresBefore(List<Proc> procedures)
        {
            ProceduresBefore = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ProceduresBefore, procedures);
        }

        private void SetCasesBefore(List<Case> cases)
        {
            CasesBefore = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(CasesBefore, cases);
        }

        private void SetTolerancesBefore(List<Tolerance> tolerances)
        {
            TolerancesBefore = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(TolerancesBefore, tolerances);
        }

        private void SetWorkflowsBefore(List<Work> workflows)
        {
            WorkflowsBefore = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(WorkflowsBefore, workflows);
        }

        private void SetSize()
        {
            Size = Properties.Settings.Default.AdministrationWindowSize;
        }

        private void Administration_Load(object sender, EventArgs e)
        {
            //ltbProcedures_SelectedIndexChanged(null, null);
            //lbWorkflows_SelectedIndexChanged(null, null);
            //lbTolerances_SelectedIndexChanged(null, null);
            //lbCases_SelectedIndexChanged(null, null);
            lbUsedProcedures_SelectedIndexChanged(null, null);
        }

        private void SetColors()
        {
            Label[] labels = new Label[]
            {
                lblNumberColumnName,
                lblSubstringNewColumn,
                lblWorkProcName,
                lblWorkName,
                lblPadNewColumn,
                lblNewColumn,
                lblNewColumnRound,
                lblProcName,
                lblCaseName,
                lblTotalShortcut,
                lblPartialShortcut,
                lblToleranceName,
                lblNewColumnMerge,
                lblUsedColumnsPadding,
                lblHeaders,
                lblUsedColumnsReplaceWhole,
                lblUsedColumnsReplaceWhole,
                lblUsedColumnsUpLowCase,
                lblUsedColumnsRound,
                lblCompareFirstColumn,
                lblCompareSecondColumn,
                lblCompareNewColumn,
                lblCountColumn
            };
            foreach (Label label in labels)
            {
                label.ForeColor = Properties.Settings.Default.RequiredField;
            }
        }

        private void RestoreSplitterDistance()
        {
            splitCases.SplitterDistance = Properties.Settings.Default.splitCases;
            splitTolerances.SplitterDistance = Properties.Settings.Default.splitTolerances;
            splitDuplicates.SplitterDistance = Properties.Settings.Default.splitDuplicates;
            splitProcedures.SplitterDistance = Properties.Settings.Default.splitProcedures;
            splitWorkflow.SplitterDistance = Properties.Settings.Default.splitWorkflow;
            splitWorkflowProcProperties.SplitterDistance = Properties.Settings.Default.splitWorkflowProcProperties;
            splitWorkflowProperties.SplitterDistance = Properties.Settings.Default.splitWorkflowProperties;
        }

        private void AddContextMenuAndDataGridViewStyle()
        {
            DataGridView[] dataGridViewsClipboard = new DataGridView[]
            {
                dgTolerance,
                dgCaseColumns,
                dgvColumns,
                dgvReplaces,
                dgvRound,
                dgUpLow,
                dgvMerge,
                dgvPadColumns,
                dgvPadConditions,
                dgvSubstringColumns,
                dgvReplaceWhole,
                dgvPVMExport
            };

            DataGridView[] dataGridViews = new DataGridView[]
            {
                dgOrderColumns
            };

            foreach (DataGridView dataGridView in dataGridViewsClipboard)
            {
                ViewHelper.AddContextMenuToDataGridView(dataGridView, true);
                ViewHelper.SetDataGridViewStyle(dataGridView);
            }
            
            foreach(DataGridView dataGridView in dataGridViews)
            {
                ViewHelper.AddContextMenuToDataGridView(dataGridView, false);
                ViewHelper.SetDataGridViewStyle(dataGridView);
            }
        }

        private void SetOrderList()
        {
            OrderList = new List<KeyValuePair<string, bool>>
            {
                new KeyValuePair<string, bool>("Aufsteigend", false),
                new KeyValuePair<string, bool>("Absteigend", true)
            };
        }

        private void SetHeaders(object[] headers)
        {
            CheckedComboBox[] checkedComboBoxes = new CheckedComboBox[]
            {
                clbHeaderOrder,
                clbHeaderProcedure,
                clbHeadersRound,
                clbUpLowHeader,
                cbSubstringHeaders,
                cbHeadersReplaceWhole,
                cbHeadersPVMExport
            };
            foreach(CheckedComboBox checkedComboBox in checkedComboBoxes)
            {
                checkedComboBox.Items.AddRange(headers);
            }
        }

        private void AssignGroupBoxToEnum()
        {
            gbState = new Dictionary<GroupBox, Type>
            {
                { gbDefDuplicate, typeof(ProcDuplicate) },
                { gbMerge, typeof(ProcMerge) },
                { gbProcedure, typeof(ProcUser) },
                { gbOrder, typeof(ProcOrder) },
                { gbUpLowCase, typeof(ProcUpLowCase) },
                { gbRound, typeof(ProcRound) },
                { gbPadding, typeof(ProcPadding) },
                { gbNumber, typeof(ProcNumber) },
                { gbSubstring, typeof(ProcSubstring) },
                { gbReplaceWhole, typeof(ProcReplaceWhole) },
                { gbAddTableColumns, typeof(ProcAddTableColumns) },
                { gbCompare, typeof(ProcCompare) },
                { gbPVMExport, typeof(ProcPVMExport) },
                { gbCount, typeof(ProcCount) }
            };

            assignControls = new Dictionary<Type, Action<WorkProc>> {
                { typeof(ProcUser), SetUserControls},
                { typeof(ProcDuplicate), SetDuplicateControls },
                { typeof(ProcMerge), SetMergeControls },
                { typeof(ProcOrder), SetOrderControls },
                { typeof(ProcUpLowCase), SetUpLowCaseControls },
                { typeof(ProcRound), SetRoundControls },
                { typeof(ProcTrim), SetTrimControls },
                { typeof(ProcPadding), SetPaddingControls },
                { typeof(ProcNumber), SetNumberControls },
                { typeof(ProcSubstring), SetSubstringControls },
                { typeof(ProcReplaceWhole), SetReplaceWholeControls },
                { typeof(ProcAddTableColumns), SetAddTableColumnsControls },
                { typeof(ProcCompare), SetCompareControls },
                { typeof(ProcPVMExport), SetPVMExportControls },
                { typeof(ProcCount), SetCountControls }
            };
        }

        private void GenerateProceduresForWorkflow()
        {
            SystemProc = new List<Proc>
            {
                new Proc(ProcTrim.ClassName, null, 1),
                new Proc(ProcMerge.ClassName, null, 2),
                new Proc(ProcOrder.ClassName, null, 3),
                new Proc(ProcUpLowCase.ClassName, null, 4),
                new Proc(ProcRound.ClassName, null, 5),
                new Proc(ProcPadding.ClassName, null, 6),
                new Proc(ProcNumber.ClassName, null, 7),
                new Proc(ProcSubstring.ClassName, null, 8),
                new Proc(ProcReplaceWhole.ClassName, null, 9),
                new Proc(ProcAddTableColumns.ClassName, null, 10),
                new Proc(ProcCompare.ClassName, null, 11),
                new Proc(ProcPVMExport.ClassName, null, 12),
                new Proc(ProcCount.ClassName, null, 13)
            };
            SystemProc.Sort();
            GenerateDuplicateProc();
        }

        private void Administration_FormClosing(object sender, FormClosingEventArgs e)
        {
            ViewHelper.Clear();
            SaveSplitterDistance();
            SaveSize();
            Properties.Settings.Default.Save();

            if ((ExportHelper.SaveWorkflows(Workflows) || ExportHelper.SaveProcedures(Procedures) || ExportHelper.SaveTolerances(Tolerances) || ExportHelper.SaveCases(Cases)))
            {
                DialogResult result = MessageHandler.MessagesYesNo(MessageBoxIcon.Warning, "Es ist ein Fehler beim Speichern aufgetreten!\nMöchten Sie das Fenster trotzdem schließen?");
                e.Cancel = result == DialogResult.No;
            }
        }

        private void SaveSize()
        {
            Properties.Settings.Default.AdministrationWindowSize = Size;
        }

        private void SaveSplitterDistance()
        {
            Properties.Settings.Default.splitCases = splitCases.SplitterDistance;
            Properties.Settings.Default.splitTolerances = splitTolerances.SplitterDistance;
            Properties.Settings.Default.splitDuplicates = splitDuplicates.SplitterDistance;
            Properties.Settings.Default.splitProcedures = splitProcedures.SplitterDistance;
            Properties.Settings.Default.splitWorkflow = splitWorkflow.SplitterDistance;
            Properties.Settings.Default.splitWorkflowProcProperties = splitWorkflowProcProperties.SplitterDistance;
            Properties.Settings.Default.splitWorkflowProperties = splitWorkflowProperties.SplitterDistance;
        }

        

        #region Procedures

        private void LoadProcedures(List<Proc> procedures)
        {
            Procedures = procedures;
            bindingProcedure = new BindingList<Proc>(Procedures);
            ltbProcedures.DataSource = bindingProcedure;
            ltbProcedures.DisplayMember = "Name";
            ltbProcedures.ValueMember = "Name";
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            string name = "Neue Funktion";
            int number = 1;
            DataTable table = new DataTable { TableName = "Setting" };
            table.Columns.Add("Ersetze", typeof(string));
            table.Columns.Add("Durch", typeof(string));
            int id = MaxIdProcedure() + 1;
            Proc proc = new Proc(name + number, table, id);
            while (bindingProcedure.Contains(proc))
            {
                number++;
                proc.Name = name + number;
            }
            bindingProcedure.Add(proc);
            ltbProcedures_SelectedIndexChanged(null, null);
            SortProcedureList(id);
            LoadProceduresWorkflow();
        }

        private int MaxIdProcedure()
        {
            int max = Procedures.Count == 0 ? 1 : Procedures[0].Id;
            foreach (Proc proc in Procedures)
            {
                if (proc.Id > max)
                {
                    max = proc.Id;
                }
            }
            return max;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (ltbProcedures.SelectedIndex != -1)
            {
                DialogResult? result = null;
                int id = Procedures[ltbProcedures.SelectedIndex].Id;
                if (IsProcedureUsed(id, out string usedWorkflows, typeof(ProcUser)))
                {
                    result = MessageHandler.MessagesYesNo(MessageBoxIcon.Warning, "Diese Funktion wird von anderen Arbeitsabläufen verwendet!\nArbeitsabläufe die diese Funktion verwenden:\n" + usedWorkflows + "\nTrotzdem löschen?");
                }
                if (result == null || result == DialogResult.Yes)
                {
                    bindingProcedure.RemoveAt(ltbProcedures.SelectedIndex);
                    ResetForm();
                    ltbProcedures_SelectedIndexChanged(null, null);
                    if (result == DialogResult.Yes)
                    {
                        DeleteProcedureOfWorkflows(id, typeof(ProcUser));
                    }
                }
                LoadProceduresWorkflow();
            }
        }

        private void DeleteProcedureOfWorkflows(int id, Type type)
        {
            foreach (Work w in Workflows)
            {
                for (int i = 0; i < w.Procedures.Count; i++)
                {
                    if (w.Procedures[i].GetType() == type && w.Procedures[i].ProcedureId == id)
                    {
                        w.Procedures.RemoveAt(i);
                        i--;
                    }
                }
                SetWorkflowOrdinal(w);
            }
        }

        private void SetWorkflowOrdinal(Work work)
        {
            for(int ordinal = 0; ordinal < work.Procedures.Count; ordinal++)
            {
                work.Procedures[ordinal].Ordinal = ordinal;
            }
        }

        private bool IsProcedureUsed(int id, out string usedWorkflows, Type type)
        {
            bool status = false;
            StringBuilder builder = new StringBuilder();
            foreach (Work w in Workflows)
            {
                foreach (WorkProc wp in w.Procedures)
                {
                    if (wp.GetType() == type && wp.ProcedureId == id)
                    {
                        status = true;
                        builder.Append(w.Name + ", ");
                    }
                }
            }
            if (status)
            {
                builder.Remove(builder.Length - 2, 2);
            }
            usedWorkflows = builder.ToString();
            return status;
        }

        private void ltbProcedures_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ltbProcedures.SelectedIndex == -1)
            {
                ResetForm();
                gbSearchAndReplace.Enabled = false;
            }
            else
            {
                selectedProc = (Proc)ltbProcedures.SelectedItem;
                txtName.Text = selectedProc.Name;
                SetDataSource(dgvReplaces, selectedProc.Replace);
                cbCheckTotal.Checked = selectedProc.CheckTotal;
                SetProcedureLock(selectedProc);
            }
        }

        private void ResetForm()
        {
            txtName.Text = string.Empty;
            dgvReplaces.DataSource = null;
            cbCheckTotal.Checked = false;
        }


        private void SortProcedureList(int selectedId)
        {
            Procedures.Sort();
            bindingProcedure.ResetBindings();
            ltbProcedures.SelectedIndex = GetProcedureIndexThroughId(selectedId);
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            if (lbProcedures.SelectedIndex != -1)
            {
                selectedProc.Name = txtName.Text;
                SortProcedureList(selectedProc.Id);
                LoadProceduresWorkflow(false);
            }
        }

        private int GetProcedureIndexThroughId(int selectedId)
        {
            return Procedures.FindIndex(proc => proc.Id == selectedId);
        }
        #endregion

        #region Workflows
        private void LoadWorkflows(List<Work> workflows)
        {
            Workflows = workflows;
            bindingWorkflow = new BindingList<Work>(Workflows);
            lbWorkflows.DataSource = bindingWorkflow;
            lbWorkflows.DisplayMember = "Name";
            lbWorkflows.ValueMember = "Id";
        }

        private void LoadProceduresWorkflow(bool status = true)
        {
            lbProcedures.DataSource = null;
            lbProcedures.DataSource = GetSelectedProcedureType();
            lbProcedures.DisplayMember = "Name";
            lbProcedures.ValueMember = "Id";
            if (status)
            {
                lbWorkflows_SelectedIndexChanged(null, null);
            }
        }

        private void GenerateDuplicateProc()
        {
            DuplicateProc = new List<Proc>();
            foreach (Case cas in Cases)
            {
                DuplicateProc.Add(new Proc(cas.Name, null, cas.Id));
            }
        }

        private List<Proc> GetSelectedProcedureType()
        {
            return cmbProcedureType.SelectedIndex == 1 ? SystemProc : cmbProcedureType.SelectedIndex == 2 ? DuplicateProc : Procedures;
        }

        private void btnNewWorkflow_Click(object sender, EventArgs e)
        {
            string name = "Neuer Arbeitsablauf";
            int number = 1;
            int id = MaxIdWorkflow() + 1;
            Work work = new Work(name + number, new List<WorkProc>(), id);
            while (bindingWorkflow.Contains(work))
            {
                number++;
                work.Name = name + number;
            }
            bindingWorkflow.Add(work);
            SortWorkflowList(id);
            lbWorkflows_SelectedIndexChanged(null, null);
        }

        private int MaxIdWorkflow()
        {
            int max = Workflows.Count == 0 ? 1 : Workflows[0].Id;
            foreach (Work work in Workflows)
            {
                if (work.Id > max)
                {
                    max = work.Id;
                }
            }
            return max;
        }


        private void SortWorkflowList(int selectedId)
        {
            Workflows.Sort();
            bindingWorkflow.ResetBindings();
            lbWorkflows.SelectedIndex = GetWorkflowIndexThroughId(selectedId);
        }

        private int GetWorkflowIndexThroughId(int selectedId)
        {
            return Workflows.FindIndex(work => work.Id == selectedId);
        }


        private int GetWorkProcedureIndexThroughId(int selectedId)
        {
            return GetSelectedWorkflow().Procedures.FindIndex(proc => proc.ProcedureId == selectedId);
        }

        private void lbWorkflows_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gbProcedure.Enabled = gbWorkflow.Enabled = (lbWorkflows.SelectedIndex != -1))
            {
                Work workflow = GetSelectedWorkflow();
                SetWorkflowProcedures(workflow.Procedures);
                
                txtWorkflow.Text = workflow.Name;
                lbUsedProcedures_SelectedIndexChanged(null, null);
                SetWorkflowLock(workflow);
            }
            else
            {
                txtWorkflow.Text = string.Empty;
            }
        }

        private void SetWorkflowLock(Work workflow = null)
        {
            bool enabled = splitWorkflowProcProperties.Enabled = btnDeleteWorkflow.Enabled = !(workflow ?? GetSelectedWorkflow()).Locked;
            foreach(GroupBox groupBox in gbState.Keys)
            {
                groupBox.Enabled = enabled;
            }
            gbMain.Enabled = enabled;
        }

        private void SetProcedureLock(Proc procedure = null)
        {
            gbSearchAndReplace.Enabled = btnDeleteProcedure.Enabled = !(procedure ?? selectedProc).Locked;
        }

        private void SetToleranceLock(Tolerance procedure = null)
        {
            splitTolerances.Panel2.Enabled = btnDeleteTolerance.Enabled = !(procedure ?? Tolerances[lbTolerances.SelectedIndex]).Locked;
        }

        private void SetCaseLock(Case procedure = null)
        {
            splitCases.Panel2.Enabled = gbCaseShortcuts.Enabled = gbCaseColumns.Enabled = btnDeleteCase.Enabled = !(procedure ?? Cases[lbCases.SelectedIndex]).Locked;
        }

        private void SetWorkflowProcedures(List<WorkProc> proc, BindingList<WorkProc> newList = null)
        {
            //bindingWorkProc = new BindingList<Proc>(getProceduresWithId(proc));
            bindingWorkProc = newList ?? new BindingList<WorkProc>(proc);
            lbUsedProcedures.DataSource = null;
            lbUsedProcedures.DataSource = bindingWorkProc;
            if (bindingWorkProc.Count > 0)
            {
                lbUsedProcedures.DisplayMember = "Name";
                lbUsedProcedures.ValueMember = "ProcedureId";
            }
        }

        private void SortWorkProcList(int ordinal)
        {
            GetSelectedWorkflow().Procedures.Sort();
            SetWorkflowProcedures(GetSelectedWorkflow().Procedures);
            lbUsedProcedures.SelectedIndex = GetWorkProcedureIndexThroughOrdinal(ordinal);
        }

        private int GetWorkProcedureIndexThroughOrdinal(int ordinal)
        {
            return GetSelectedWorkflow().Procedures.FindIndex(proc => proc.Ordinal == ordinal);
        }

        private List<Proc> GetProceduresWithId(List<WorkProc> proc)
        {
            List<Proc> procs = new List<Proc>();
            foreach(WorkProc wp in proc)
            {
                procs.Add(GetMergeProcedureIndexThroughId(wp.ProcedureId, wp.GetType()));
            }
            return procs;
        }

        private void SetDataSource(DataGridView view, DataTable table)
        {
            view.DataSource = null;
            view.DataSource = table;
        }

        private Proc GetMergeProcedureIndexThroughId(int selectedId, Type type)
        {
            List <Proc> procs = type == typeof(ProcUser)? Procedures : type == typeof(ProcDuplicate) ? DuplicateProc : SystemProc;
            return procs[procs.FindIndex(proc => proc.Id == selectedId)];
        }

        private Work GetSelectedWorkflow()
        {
            return ((Work)lbWorkflows.SelectedItem);
        }

        private void SetMergeControls( WorkProc proc)
        {
            ProcMerge selectedProc = proc as ProcMerge;
            txtNewColumnMerge.Text = selectedProc.NewColumn;
            txtFormula.Text = selectedProc?.Format?.ToString() ?? string.Empty;
            lblOriginalNameText.Text = ProcMerge.ClassName;

            SetMergeDataGridView(selectedProc.Conditions);
        }

        private void SetMergeDataGridView(DataTable table)
        {
            dgvMerge.DataSource = null;
            dgvMerge.Rows.Clear();
            dgvMerge.Columns.Clear();
            dgvMerge.DataSource = table.DefaultView;

            DataGridViewButtonColumn boxCol = new DataGridViewButtonColumn
            {
                Text = "Format",
                UseColumnTextForButtonValue = true
            };
            if (dgvMerge.Columns.Count > 3)
            {
                dgvMerge.Columns[(int)ProcMerge.ConditionColumn.Format].ReadOnly = true;
            }
            dgvMerge.Columns.Add(boxCol);
        }

        private void dgvMerge_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //Button clicked
            if(e.ColumnIndex > -1 && e.RowIndex > -1 && dgvMerge[e.ColumnIndex, e.RowIndex] is DataGridViewButtonCell)
            {
                ViewHelper.EndDataGridViewEdit(dgvMerge);
                DataTable table = (dgvMerge.DataSource as DataView).Table;
                
                var row = (dgvMerge.Rows[e.RowIndex].DataBoundItem as DataRowView).Row;
                
                if (!(row[(int)ProcMerge.ConditionColumn.Format] is MergeFormat))
                {
                    row[(int)ProcMerge.ConditionColumn.Format] = new MergeFormat();
                }
                
                MergeFormatView view = new MergeFormatView(row[(int)ProcMerge.ConditionColumn.Format] as MergeFormat);
                view.ShowDialog();
                dgvMerge.Refresh();
            }
        }

        private void dgvMerge_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex > -1 && e.RowIndex > -1 && dgvMerge[e.ColumnIndex,e.RowIndex] is DataGridViewButtonCell && string.IsNullOrWhiteSpace(e.Value?.ToString()))
            {
                e.Value = "Format";
            }
        }

        private void SetPaddingControls(WorkProc selectedProc)
        {
            ProcPadding proc = selectedProc as ProcPadding;
            lblOriginalNameText.Text = ProcPadding.ClassName;
            nbPadCount.Value = proc.Counter;
            txtNewColumnPad.Text = proc.NewColumn;
            
            bool status = proc.OperationSide == ProcPadding.Side.Left;
            RbLeft.Checked = status;
            RbRight.Checked = !status;
            TxtCharacter.Text = proc.Character?.ToString() ?? string.Empty;
            cbPadNewColumn.Checked = !string.IsNullOrWhiteSpace(proc.NewColumn);
            cbPadOldColumn.Checked = proc.CopyOldColumn;
            SetHeaderPadding(proc.GetAffectedHeaders());
            SetDataSource(dgvPadColumns, proc.Columns);
            SetDataSource(dgvPadConditions, proc.Conditions);
        }

        private void SetSubstringControls(WorkProc selectedProc)
        {
            ProcSubstring proc = selectedProc as ProcSubstring;
            SetHeaderSubstring(selectedProc.GetHeaders());
            lblOriginalNameText.Text = ProcSubstring.ClassName;
            cbSubstringNewColumn.Checked = !string.IsNullOrWhiteSpace(proc.NewColumn);
            txtSubstringNewColumn.Text = proc.NewColumn;
            nbSubstringStart.Value = proc.Start;
            nbSubstringEnd.Value = proc.End;
            txtSubstringText.Text = proc.ReplaceText;
            txtSubstringText.Visible = cbSubstringText.Checked = proc.ReplaceChecked;
            cbSubstringOldColumn.Checked = proc.CopyOldColumn;
            SetDataSource(dgvSubstringColumns, proc.Columns);
        }

        private void SetReplaceWholeControls(WorkProc selectedProc)
        {
            ProcReplaceWhole proc = selectedProc as ProcReplaceWhole;
            SetHeaderReplaceWhole(selectedProc.GetHeaders());
            lblOriginalNameText.Text = ProcReplaceWhole.ClassName;
            SetDataSource(dgvReplaceWhole, proc.Columns);

        }

        private void SetAddTableColumnsControls(WorkProc selectedProc)
        {
            ProcAddTableColumns proc = selectedProc as ProcAddTableColumns;
            lblOriginalNameText.Text = ProcAddTableColumns.ClassName;
            txtIdentifierSource.Text = proc.IdentifySource;
            txtIdentifierAppend.Text = proc.IdentifyAppend;
            txtNewColumn.Text = proc.NewColumn;
            cbNewColumnAddTableColumn.Checked = !string.IsNullOrWhiteSpace(proc.NewColumn);
        }

        private void SetCompareControls(WorkProc selectedProc)
        {
            ProcCompare proc = selectedProc as ProcCompare;
            lblOriginalNameText.Text = ProcCompare.ClassName;
            txtCompareSourceColumn.Text = proc.SourceColumn;
            txtCompareSecondColumn.Text = proc.CompareColumn;
            txtNewColumn.Text = proc.NewColumn;
            cbCompareNewColumn.Checked = !string.IsNullOrWhiteSpace(proc.NewColumn);
            cbCompareOldColumn.Checked = proc.CopyOldColumn;
        }

        private void SetNumberControls(WorkProc selectedProc)
        {
            ProcNumber proc = selectedProc as ProcNumber;
            lblOriginalNameText.Text = ProcNumber.ClassName;
            txtNumberNewColumn.Text = proc.NewColumn;
            nbNumberStart.Value = proc.Start;
            nbNumberEnd.Value = proc.End;
            cbNumberRepeat.Checked = proc.Repeat;
        }

        private void SetTrimControls(WorkProc selectedProc)
        {
            lblOriginalNameText.Text = ProcTrim.ClassName;
        }

        private void SetUserControls(WorkProc selectedProc)
        {
            lblOriginalNameText.Text = GetProcedureName(selectedProc.ProcedureId);
            cbNewColumn.Checked = !string.IsNullOrWhiteSpace(selectedProc.NewColumn);
            SetDataSource(dgvColumns, selectedProc.Columns);

            SetHeaderProcedure(selectedProc.Columns.Rows.Cast<DataRow>().Select(row => row[0].ToString()).ToArray());
        }

        private void SetRoundControls(WorkProc selectedProc)
        {
            lblOriginalNameText.Text = ProcRound.ClassName;
            cbNewColumnRound.Checked = !string.IsNullOrWhiteSpace(selectedProc.NewColumn);
            SetDataSource(dgvRound, selectedProc.Columns);
            SetHeaderRound(selectedProc.Columns.Rows.Cast<DataRow>().Select(row => row[0].ToString()).ToArray());
        }

        private void SetDuplicateControls(WorkProc selectedProc)
        {
            Case myCase = Cases[getCaseIndexThroughId(selectedProc.ProcedureId)];
            lblOriginalNameText.Text = myCase.Name;
            DataTable temp = myCase.Columns.Copy();
            DataTable table = new DataTable { TableName = "WorkDuplicates" };
            object[] firstColumn = temp.Rows.Cast<DataRow>().Select(dc => dc.ItemArray[0]).ToArray();

            table.Columns.Add("Bezeichnung", typeof(string));
            table.Columns.Add("Zuweisung", typeof(string));

            if (selectedProc.DuplicateColumns.Length < firstColumn.Length)
            {
                selectedProc.DuplicateColumns = new string[firstColumn.Length];
            }

            for (int i = 0; i < firstColumn.Length; i++)
            {
                table.Rows.Add(new object[] { firstColumn[i], selectedProc.DuplicateColumns[i] });
            }
            SetDataSource(dgColumnDefDuplicate, table);
            dgColumnDefDuplicate.Columns[0].ReadOnly = true;
        }

        private void SetUpLowCaseControls(WorkProc proc)
        {
            ProcUpLowCase selectedProc = ((ProcUpLowCase)proc);
            lblOriginalNameText.Text = ProcUpLowCase.ClassName;
            cbUpLow.Checked = selectedProc.AllColumns;
            cmbUpLow.SelectedIndex = selectedProc.Option;
            SetDataSource(dgUpLow, selectedProc.Columns);
            SetUpLowEnabled(!selectedProc.AllColumns);
            SetHeaderUpLowCase(selectedProc.Columns.Rows.Cast<DataRow>().Select(row => row[0].ToString()).ToArray());
        }

        private void SetOrderControls(WorkProc selectedProc)
        {
            SetHeaderOrder(selectedProc.Columns.Rows.Cast<DataRow>().Select(row => row[0].ToString()).ToArray());
            lblOriginalNameText.Text = ProcOrder.ClassName;

            if (selectedProc.Columns.Columns.Count <= 1)
            {
                DataTable table = new DataTable { TableName = "Order" };
                table.Columns.Add("Spalte", typeof(string));
                table.Columns.Add("Sortierung", typeof(string));
                table.Columns[1].DataType = typeof(bool);
                selectedProc.Columns = table;
            }
            SetDataSource(dgOrderColumns, selectedProc.Columns);

            dgOrderColumns.Columns[1].Visible = false;

            DataGridViewComboBoxColumn cmb = new DataGridViewComboBoxColumn
            {
                DataSource = OrderList,
                HeaderText = "Sortierung",
                DisplayMember = "key",
                ValueMember = "value",
                DataPropertyName = "Sortierung"
            };


            dgOrderColumns.Columns.Add(cmb);
        }

        private void SetPVMExportControls(WorkProc selectedProc)
        {
            SetHeaderOrder(selectedProc.Columns.Rows.Cast<DataRow>().Select(row => row[0].ToString()).ToArray());
            lblOriginalNameText.Text = ProcPVMExport.ClassName;

            SetDataSource(dgvPVMExport, selectedProc.Columns);
        }

        private void SetCountControls(WorkProc selectedProc)
        {
            ProcCount proc = selectedProc as ProcCount;
            lblOriginalNameText.Text = ProcCount.ClassName;
            TxtCountColumn.Text = proc.Column;
            cbShowFromTo.Checked = proc.ShowFromTo;
            cbCount.Checked = proc.CountChecked;
            nbCount.Value = proc.Count == 0 ? 1 : proc.Count;
            nbCount.Visible = proc.CountChecked;
        }

        private void SetProcValues(WorkProc selectedProc)
        {
            txtWorkProcName.Text = selectedProc.Name;
            SetNewColumnText(selectedProc.NewColumn);
            SetGroupBoxVisibility(selectedProc.GetType());
        }


        private void lbUsedProcedures_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gbMain.Enabled = (lbUsedProcedures.SelectedIndex != -1))
            {
                WorkProc selectedProc = GetSelectedWorkProcedure();
                SetProcValues(selectedProc);
                assignControls?[selectedProc.GetType()](selectedProc);
            }
            else
            {
                SetGroupBoxVisibility(typeof(ProcTrim));
                txtWorkProcName.TextChanged -= txtWorkProcName_TextChanged;
                txtWorkProcName.Text = lblOriginalNameText.Text = string.Empty;
                txtWorkProcName.TextChanged += txtWorkProcName_TextChanged;
            }
        }

        private void SetGroupBoxVisibility(Type type)
        {
            if (gbState != null)
            {
                foreach (GroupBox box in gbState.Keys)
                {
                    box.Visible = gbState[box] == type;
                }
                gbMain.Visible = type != null;
            }
        }

        private void SetNewColumnText(string text)
        {
            txtNewColumn.Text = text;
        }

        private string GetProcedureName(int id)
        {
            return Procedures[GetProcedureIndexThroughId(id)].Name;
        }

        private void ResetHeader(CheckedComboBox box, ItemCheckEventHandler handler)
        {
            box.ItemCheck -= handler;
            for(int i = 0; i < box.Items.Count; i++)
            {
                box.SetItemChecked(i, false);
            }
            box.ItemCheck += handler;
        }

        private void SetHeaderProcedure(string[] headers)
        {
            SetChecked(clbHeaderProcedure, headers, clbHeaderProcedure_ItemCheck);
        }

        private void SetHeaderPadding(string[] headers)
        {
            SetChecked(cbHeadersPad, headers, clbHeaderPad_ItemCheck);
        }

        private void SetHeaderSubstring(string[] headers)
        {
            SetChecked(cbSubstringHeaders, headers, cbSubstringHeaders_ItemCheck);
        }

        private void SetHeaderReplaceWhole(string[] headers)
        {
            SetChecked(cbHeadersReplaceWhole, headers, cbHeadersReplaceWhole_ItemCheck);
        }

        private void SetHeaderRound(string[] headers)
        {
            SetChecked(clbHeadersRound, headers, clbHeadersRound_ItemCheck);
        }

        private void SetHeaderUpLowCase(string[] headers)
        {
            SetChecked(clbUpLowHeader, headers, clbUpLowHeader_ItemCheck);
        }

        private void SetChecked(CheckedComboBox box, string[] headers, ItemCheckEventHandler handler)
        {
            box.ItemCheck -= handler;
            for (int i = 0; i < box.Items.Count; i++)
            {
                box.SetItemChecked(i, headers.Contains(box.Items[i].ToString()));
            }
            box.ItemCheck += handler;
        }

        private void SetHeaderOrder(string[] headers)
        {
            SetChecked(clbHeaderOrder, headers, clbHeaderOrder_ItemCheck);
        }

        private WorkProc GetSelectedWorkProcedure()
        {
            return GetSelectedWorkflow().Procedures[lbUsedProcedures.SelectedIndex];
        }

        private void btnDeleteWorkflow_Click(object sender, EventArgs e)
        {
            bindingWorkflow.RemoveAt(lbWorkflows.SelectedIndex);
            lbWorkflows_SelectedIndexChanged(null, null);
        }

        private void btnAddProcedureToWorkflow_Click(object sender, EventArgs e)
        {
            if (lbProcedures.SelectedIndex != -1)
            {
                Work workflow = GetSelectedWorkflow();
                workflow.Procedures.Add(WorkflowHelper.CreateWorkProc(cmbProcedureType.SelectedIndex, (int)lbProcedures.SelectedValue, workflow.Procedures.Count, ((Proc)lbProcedures.SelectedItem).Name));

                SetWorkflowProcedures(workflow.Procedures);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (lbUsedProcedures.SelectedIndex != -1)
            {
                Work workflow = GetSelectedWorkflow();
                workflow.Procedures.RemoveAt(lbUsedProcedures.SelectedIndex);
                for(int i = lbUsedProcedures.SelectedIndex; i < workflow.Procedures.Count; i++)
                {
                    workflow.Procedures[i].Ordinal--;
                }
                SetWorkflowProcedures(workflow.Procedures);
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            if(lbUsedProcedures.SelectedIndex != -1)
            {
                Work workflow = GetSelectedWorkflow();
                WorkProc wp = workflow.Procedures[lbUsedProcedures.SelectedIndex];
                if(wp.Ordinal > 0)
                {
                    workflow.Procedures[lbUsedProcedures.SelectedIndex - 1].Ordinal++;
                    wp.Ordinal--;
                    SortWorkProcList(wp.Ordinal);
                }
            }            
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            if (lbUsedProcedures.SelectedIndex != -1)
            {
                Work workflow = GetSelectedWorkflow();
                WorkProc wp = workflow.Procedures[lbUsedProcedures.SelectedIndex];
                if (wp.Ordinal < workflow.Procedures.Count -1)
                {
                    workflow.Procedures[lbUsedProcedures.SelectedIndex + 1].Ordinal--;
                    wp.Ordinal++;
                    SortWorkProcList(wp.Ordinal);
                }
            }
        }

        private void cbNewColumn_CheckedChanged(object sender, EventArgs e)
        {
            NewColumnChanged(cbNewColumn, cbOldColumn, lblNewColumn, txtNewColumn);
        }

        private void txtNewColumn_TextChanged(object sender, EventArgs e)
        {
            GetSelectedWorkProcedure().NewColumn = (sender as TextBox)?.Text;
        }

        private void addColumnToFormula(string column)
        {
            if (!txtFormula.Text.Contains(column))
            {
                string separator = txtFormula.Text.Length > 0 ? " " : string.Empty;
                txtFormula.Text = $"{txtFormula.Text}{separator}[{column}]";
            }
        }

        private void RemoveColumnOfFormula(string column)
        {
            txtFormula.Text = txtFormula.Text.Replace($" [{column}]", "");
        }

        private void txtWorkflow_TextChanged(object sender, EventArgs e)
        {
            if (lbWorkflows.SelectedIndex != -1)
            {
                GetSelectedWorkflow().Name = txtWorkflow.Text;
                bindingWorkflow.ResetBindings();
                SortWorkflowList(GetSelectedWorkflow().Id);
            }
        }

        private void cmbProcedureType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadProceduresWorkflow();
        }

        #endregion


        #region Tolerances


        private void LoadTolerances(List<Tolerance> tolerances)
        {
            Tolerances = tolerances;
            bindingTolerance = new BindingList<Tolerance>(Tolerances);
            lbTolerances.DataSource = bindingTolerance;
            lbTolerances.DisplayMember = "Name";
            lbTolerances.ValueMember = "Name";
        }


        private void SortToleranceList(int selectedId)
        {
            Tolerances.Sort();
            bindingTolerance.ResetBindings();
            lbTolerances.SelectedIndex = GetToleranceIndexThroughId(selectedId);
        }

        private int GetToleranceIndexThroughId(int selectedId)
        {
            return Tolerances.FindIndex(tol => tol.Id == selectedId);
        }

        private void btnNewTolerance_Click(object sender, EventArgs e)
        {
            string name = "Neue Toleranz";
            int number = 1;
            DataTable table = new DataTable { TableName = "Tolerance" };
            table.Columns.Add("Bezeichnung", typeof(string));

            int id = MaxIdTolerance() + 1;
            Tolerance proc = new Tolerance(name + number, table, id);
            while (bindingTolerance.Contains(proc))
            {
                number++;
                proc.Name = name + number;
            }
            bindingTolerance.Add(proc);
            SortToleranceList(id);
            lbTolerances_SelectedIndexChanged(null, null);
        }

        private int MaxIdTolerance()
        {
            int max = Tolerances.Count == 0 ? 1 : Tolerances[0].Id;
            foreach (Tolerance proc in Tolerances)
            {
                if (proc.Id > max)
                {
                    max = proc.Id;
                }
            }
            return max;
        }

        private void lbTolerances_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbTolerances.SelectedIndex == -1)
            {
                txtToleranceName.Text = string.Empty;
                dgTolerance.DataSource = null;
            }
            else
            {
                Tolerance selectedTolerance = (Tolerance)lbTolerances.SelectedItem;
                txtToleranceName.Text = selectedTolerance.Name;
                SetDataSource(dgTolerance, selectedTolerance.Columns);
                SetToleranceLock();
            }
        }

        private void txtToleranceName_TextChanged(object sender, EventArgs e)
        {
            if(lbTolerances.SelectedIndex != -1)
            {
                Tolerance selectedTolerance = (Tolerance)lbTolerances.SelectedItem;
                selectedTolerance.Name = txtToleranceName.Text;
                SortToleranceList(selectedTolerance.Id);
            }
        }

        private void btnDeleteTolerance_Click(object sender, EventArgs e)
        {
            if (lbTolerances.SelectedIndex > -1)
            {
                bindingTolerance.RemoveAt(lbTolerances.SelectedIndex);
                lbTolerances_SelectedIndexChanged(null, null);
            }
        }

        #endregion

        #region Cases

        private void btnNewCase_Click(object sender, EventArgs e)
        {
            string name = "Neuer Fall";
            int number = 1;
            DataTable table = new DataTable { TableName = "Case" };
            table.Columns.Add("Bezeichnung", typeof(string));
            table.Columns.Add("Von", typeof(int));
            table.Columns.Add("Bis", typeof(int));

            int id = MaxIdCase() + 1;
            Case proc = new Case(name + number, string.Empty, table, id);
            while (bindingCase.Contains(proc))
            {
                number++;
                proc.Name = name + number;
            }
            bindingCase.Add(proc);
            SortCaseList(id);
            lbCases_SelectedIndexChanged(null, null);
            GenerateDuplicateProc();
            LoadProceduresWorkflow();
        }

        private void LoadCases(List<Case> cases)
        {
            Cases = cases;
            bindingCase = new BindingList<Case>(Cases);
            lbCases.DataSource = bindingCase;
            lbCases.DisplayMember = "Name";
            lbCases.ValueMember = "Name";
        }


        private void SortCaseList(int selectedId)
        {
            Cases.Sort();
            bindingCase.ResetBindings();
            lbCases.SelectedIndex = getCaseIndexThroughId(selectedId);
        }

        private int getCaseIndexThroughId(int selectedId)
        {
            return Cases.FindIndex(tol => tol.Id == selectedId);
        }

        private int MaxIdCase()
        {
            int max = Cases.Count == 0 ? 1 : Cases[0].Id;
            foreach (Case proc in Cases)
            {
                if (proc.Id > max)
                {
                    max = proc.Id;
                }
            }
            return max;
        }

        private void lbCases_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbCases.SelectedIndex == -1)
            {
                txtCaseName.Text = string.Empty;
                txtShortcut.Text = string.Empty;
                txtShortcutTotal.Text = string.Empty;
                dgCaseColumns.DataSource = null;
            }
            else
            {
                Case selectedCase = (Case)lbCases.SelectedItem;
                txtCaseName.Text = selectedCase.Name;
                txtShortcut.Text = selectedCase.Shortcut;
                txtShortcutTotal.Text = selectedCase.ShortcutTotal;
                SetDataSource(dgCaseColumns, selectedCase.Columns);
                if (ViewHelper != null)
                {
                    ViewHelper.SelectedCase = selectedCase.Id;
                }
                SetCaseLock();
            }
        }

        private void txtCaseName_TextChanged(object sender, EventArgs e)
        {
            if (lbCases.SelectedIndex != -1)
            {
                Case selectedCase = (Case)lbCases.SelectedItem;
                selectedCase.Name = txtCaseName.Text;
                SortCaseList(selectedCase.Id);

                GenerateDuplicateProc();
                LoadProceduresWorkflow();
            }
        }

        private void txtShortcut_TextChanged(object sender, EventArgs e)
        {
            if (lbCases.SelectedIndex != -1)
            {
                ((Case)lbCases.SelectedItem).Shortcut = txtShortcut.Text;
            }
        }

        private void btnDeleteCase_Click(object sender, EventArgs e)
        {
            if (lbCases.SelectedIndex > -1)
            {
                DialogResult? result = null;
                int id = ((Case)lbCases.SelectedItem).Id;
                if (IsProcedureUsed(id, out string usedWorkflows, typeof(ProcDuplicate)))
                {
                    result = MessageHandler.MessagesYesNo(MessageBoxIcon.Warning, "Dieser Fall wird von anderen Arbeitsabläufen verwendet!\nArbeitsabläufe die diese Funktion verwenden:\n" + usedWorkflows + "\nTrotzdem löschen?");
                }
                if (result == null || result == DialogResult.Yes)
                {
                    bindingCase.RemoveAt(lbCases.SelectedIndex);
                    lbCases_SelectedIndexChanged(null, null);
                    if (result == DialogResult.Yes)
                    {
                        DeleteProcedureOfWorkflows(id, typeof(ProcDuplicate));
                    }
                }
                GenerateDuplicateProc();
                LoadProceduresWorkflow();
            }
        }

        private void dgCaseColumns_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            ViewHelper.HandleDataGridViewNumber(sender, e);
        }
        #endregion

        private void txtWorkProcName_TextChanged(object sender, EventArgs e)
        {
            lbUsedProcedures.SelectedIndexChanged -= lbUsedProcedures_SelectedIndexChanged;
            int index = lbUsedProcedures.SelectedIndex;

            GetSelectedWorkflow().Procedures[lbUsedProcedures.SelectedIndex].Name = txtWorkProcName.Text;
            bindingWorkProc[lbUsedProcedures.SelectedIndex].Name = txtWorkProcName.Text;

            SetWorkflowProcedures(null, bindingWorkProc);
            lbUsedProcedures.SelectedIndex = index;

            lbUsedProcedures.SelectedIndexChanged += lbUsedProcedures_SelectedIndexChanged;
        }

        private void dgCaseColumns_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            ViewHelper.EndDataGridViewEdit(dgCaseColumns);
            WorkflowHelper.AdjustDuplicateColumns((Case)lbCases.SelectedItem, Workflows);
            lbUsedProcedures_SelectedIndexChanged(null, null);
        }

        private void zwischenablageEinfügenToolStripMenuItem_Click(object sender, EventArgs e, int selectedRow)
        {
            ViewHelper.InsertClipboardToDataGridView((DataGridView)sender, selectedRow);
        }

        private void dgColumnDefDuplicate_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            WorkProc selecteWorkProc = GetSelectedWorkProcedure();
            selecteWorkProc.DuplicateColumns = GetDataSource(dgColumnDefDuplicate).Rows.Cast<DataRow>().Select(row => row.ItemArray[1].ToString()).ToArray();
        }

        private DataTable GetDataSource(DataGridView dgv)
        {
            ViewHelper.EndDataGridViewEdit(dgv);
            return ((DataTable)dgv.DataSource).Copy();
        }

        private void clbHeaderProcedure_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ViewHelper.AddRemoveHeaderThroughCheckedListBox(dgvColumns, e, (CheckedListBox)sender);
        }

        private void clbHeaderPad_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ViewHelper.AddRemoveHeaderThroughCheckedListBox(dgvPadColumns, e, (CheckedListBox)sender);
        }

        private void clbHeadersRound_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ViewHelper.AddRemoveHeaderThroughCheckedListBox(dgvColumns, e, (CheckedListBox)sender);
        }

        private void clbHeaderOrder_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            WorkProc selectedProc = GetSelectedWorkProcedure();
            DataTable table = GetDataSource(dgOrderColumns);
            string value = clbHeaderOrder.Items[e.Index].ToString();
            if (e.NewValue == CheckState.Checked)
            {
                table.Rows.Add(new object[] { value, false });
            }
            else
            {
                bool found = false;
                for (int i = table.Rows.Count - 1; i >= 0 && !found; i--)
                {
                    if(table.Rows[i][0].ToString() == value)
                    {
                        table.Rows.RemoveAt(i);
                        found = true;
                    }
                }
            }
            selectedProc.Columns = table;
            AssignDataSource(table, dgOrderColumns);
        }

        private void AssignDataSource(DataTable table, DataGridView sender)
        {
            sender.DataSource = table;
        }

        private void dgOrderColumns_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 2 && e.Value?.ToString() == string.Empty)
            {
                e.Value = OrderList[0].Key;
            }
        }
        

        private void dgOrderColumns_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is ComboBox)
            {
                ComboBox ctl = e.Control as ComboBox;
                ctl.Enter -= new EventHandler(ctl_Enter);
                ctl.Enter += new EventHandler(ctl_Enter);
            }
        }

        private void ctl_Enter(object sender, EventArgs e)
        {
            (sender as ComboBox).DroppedDown = true;
        }

        private void cmbUpLow_SelectedIndexChanged(object sender, EventArgs e)
        {
            ((ProcUpLowCase)GetSelectedWorkProcedure()).Option = cmbUpLow.SelectedIndex;
        }

        private void cbUpLow_CheckedChanged(object sender, EventArgs e)
        {
            
            ((ProcUpLowCase)GetSelectedWorkProcedure()).AllColumns = cbUpLow.Checked;
            SetUpLowEnabled(!cbUpLow.Checked);
        }

        private void SetUpLowEnabled(bool status)
        {
            clbUpLowHeader.Enabled = dgUpLow.Enabled = status;
        }

        private void clbUpLowHeader_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ViewHelper.AddRemoveHeaderThroughCheckedListBox(dgUpLow, e, (CheckedListBox)sender);
        }

        private void txtNewColumnRound_TextChanged(object sender, EventArgs e)
        {
            GetSelectedWorkflow().Procedures[lbUsedProcedures.SelectedIndex].NewColumn = ((TextBox)sender).Text;
        }

        private void cbNewColumnRound_CheckedChanged(object sender, EventArgs e)
        {
            NewColumnChanged(cbNewColumnRound, cbOldColumnRound, lblNewColumnRound, txtNewColumnRound);
        }

        private void NewColumnChanged(CheckBox newColumn, CheckBox oldColumn, Label newColumnLabel, TextBox newColumnTextbox)
        {
            newColumnLabel.Visible = newColumnTextbox.Visible = newColumn.Checked;
            if (oldColumn.Visible = !newColumn.Checked)
            {
                newColumnTextbox.Text = string.Empty;
            }
            else
            {
                oldColumn.Checked = false;
            }
        }

        private void numDec_ValueChanged(object sender, EventArgs e)
        {
            ((ProcRound)GetSelectedWorkProcedure()).Decimals = (int)((NumericUpDown)sender).Value;
        }

        private void CmBRound_SelectedIndexChanged(object sender, EventArgs e)
        {
            ((ProcRound)GetSelectedWorkProcedure()).Type = CmBRound.SelectedIndex;
        }

        private void DataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            ViewHelper.EndDataGridViewEdit(sender as DataGridView);
        }

        private void txtNewColumnPad_TextChanged(object sender, EventArgs e)
        {
            GetSelectedWorkProcedure().NewColumn = ((TextBox)sender).Text;
        }

        private void cbPadNewColumn_CheckedChanged(object sender, EventArgs e)
        {
            NewColumnChanged(cbPadNewColumn, cbPadOldColumn, lblPadNewColumn, txtNewColumnPad);
        }

        private void RbLeft_CheckedChanged(object sender, EventArgs e)
        {
            SetPadOperationSide(((RadioButton)sender).Checked ? ProcPadding.Side.Left : ProcPadding.Side.Right);
        }

        private void SetPadOperationSide(ProcPadding.Side side)
        {
            ((ProcPadding)GetSelectedWorkProcedure()).OperationSide = side;
        }

        private void TxtCharacter_TextChanged(object sender, EventArgs e)
        {
            ((ProcPadding)GetSelectedWorkProcedure()).Character = ((TextBox)sender).Text.Length > 0 ? ((TextBox)sender).Text[0] : (char?)null;
        }

        private void nbPadCount_ValueChanged(object sender, EventArgs e)
        {
            ((ProcPadding)GetSelectedWorkProcedure()).Counter = (int)((NumericUpDown)sender).Value;
        }

        private void cbCheckTotal_CheckedChanged(object sender, EventArgs e)
        {
            if (lbProcedures.SelectedIndex != -1)
            {
                selectedProc.CheckTotal = cbCheckTotal.Checked;
            }
        }

        private void txtShortcutTotal_TextChanged(object sender, EventArgs e)
        {
            if (lbCases.SelectedIndex != -1)
            {
                ((Case)lbCases.SelectedItem).ShortcutTotal = txtShortcutTotal.Text;
            }
        }

        private void lbWorkflows_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < Workflows.Count)
            {
                ViewHelper.DrawLock((ListBox)sender, e, Workflows[e.Index].Locked);
            }
        }

        private void lbWorkflows_MouseDown(object sender, MouseEventArgs e)
        {
            ViewHelper.SetLock(e, Workflows, (ListBox)sender, delegate { lbWorkflows_SelectedIndexChanged(null, null); });
        }

        private void ltbProcedures_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < Procedures.Count)
            {
                ViewHelper.DrawLock((ListBox)sender, e, Procedures[e.Index].Locked);
            }
        }

        private void ltbProcedures_MouseDown(object sender, MouseEventArgs e)
        {
            ViewHelper.SetLock(e, Procedures, (ListBox)sender, delegate { ltbProcedures_SelectedIndexChanged(null, null); });
        }

        private void lbTolerances_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < Tolerances.Count)
            {
                ViewHelper.DrawLock((ListBox)sender, e, Tolerances[e.Index].Locked);
            }
        }

        private void lbTolerances_MouseDown(object sender, MouseEventArgs e)
        {
            ViewHelper.SetLock(e, Tolerances, (ListBox)sender, delegate { lbTolerances_SelectedIndexChanged(null, null); });
        }

        private void lbCases_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < Cases.Count)
            {
                ViewHelper.DrawLock((ListBox)sender, e, Cases[e.Index].Locked);
            }
        }

        private void lbCases_MouseDown(object sender, MouseEventArgs e)
        {
            ViewHelper.SetLock(e, Cases, (ListBox)sender, delegate { lbCases_SelectedIndexChanged(null, null); });
        }

        private void cbPadOldColumn_CheckedChanged(object sender, EventArgs e)
        {
            ((ProcPadding)GetSelectedWorkProcedure()).CopyOldColumn = cbPadOldColumn.Checked;
        }

        private void cbOldColumn_CheckedChanged(object sender, EventArgs e)
        {
            ((ProcUser)GetSelectedWorkProcedure()).CopyOldColumn = cbOldColumn.Checked;
        }

        private void cbOldColumnRound_CheckedChanged(object sender, EventArgs e)
        {
            ((ProcRound)GetSelectedWorkProcedure()).CopyOldColumn = cbOldColumnRound.Checked;
        }

        private void txtNumberNewColumn_TextChanged(object sender, EventArgs e)
        {
            GetSelectedWorkProcedure().NewColumn = txtNumberNewColumn.Text;
        }

        private void nbNumberStart_ValueChanged(object sender, EventArgs e)
        {
            ((ProcNumber)GetSelectedWorkProcedure()).Start = (int)nbNumberStart.Value;
        }

        private void nbNumberEnd_ValueChanged(object sender, EventArgs e)
        {
            ((ProcNumber)GetSelectedWorkProcedure()).End = (int)nbNumberEnd.Value;
        }

        private void cbNumberRepeat_CheckedChanged(object sender, EventArgs e)
        {
            ((ProcNumber)GetSelectedWorkProcedure()).Repeat = cbNumberRepeat.Checked;
        }

        private void cbSubstringHeaders_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ViewHelper.AddRemoveHeaderThroughCheckedListBox(dgvSubstringColumns, e, (CheckedListBox)sender);
        }

        private void ndSubstringStart_ValueChanged(object sender, EventArgs e)
        {
            if (nbSubstringEnd.Value < nbSubstringStart.Value && nbSubstringEnd.Value != 0)
            {
                nbSubstringStart.Value = nbSubstringEnd.Value;
            }
            (GetSelectedWorkProcedure() as ProcSubstring).Start = (int)nbSubstringStart.Value;
        }

        private void txtSubstringNewColumn_TextChanged(object sender, EventArgs e)
        {
            GetSelectedWorkProcedure().NewColumn = txtSubstringNewColumn.Text;
        }

        private void nbSubstringEnd_ValueChanged(object sender, EventArgs e)
        {
            if(nbSubstringEnd.Value < nbSubstringStart.Value && nbSubstringEnd.Value != 0)
            {
                nbSubstringEnd.Value = Convert.ToInt32(nbSubstringEnd.Text) > nbSubstringEnd.Value ? 0 : nbSubstringStart.Value;
            }
            (GetSelectedWorkProcedure() as ProcSubstring).End = (int)nbSubstringEnd.Value;
        }

        private void cbSubstringOldColumn_CheckedChanged(object sender, EventArgs e)
        {
            GetSelectedWorkProcedure().CopyOldColumn = cbSubstringOldColumn.Checked;
        }

        private void cbSubstringNewColumn_CheckedChanged(object sender, EventArgs e)
        {
            NewColumnChanged(cbSubstringNewColumn, cbSubstringOldColumn, lblSubstringNewColumn, txtSubstringNewColumn);
        }

        private void GroupBox_EnabledChanged(object s, EventArgs e)
        {
            ViewHelper.SetControlColor(s as GroupBox);
        }

        private void SplitContainer_EnabledChanged(object sender, EventArgs e)
        {
            ViewHelper.SetControlColor(splitWorkflowProperties.Panel1);
            ViewHelper.SetControlColor(splitWorkflowProperties.Panel2);
        }

        private void SplitterPanel_EnabledChanged(object sender, EventArgs e)
        {
            ViewHelper.SetControlColor(sender as SplitterPanel);
        }

        private void cbHeadersReplaceWhole_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ViewHelper.AddRemoveHeaderThroughCheckedListBox(dgvReplaceWhole, e, sender as CheckedListBox);
        }

        private void txtIdentifierSource_TextChanged(object sender, EventArgs e)
        {
            (GetSelectedWorkProcedure() as ProcAddTableColumns).IdentifySource = txtIdentifierSource.Text;
        }

        private void txtIdentifierAppend_TextChanged(object sender, EventArgs e)
        {
            (GetSelectedWorkProcedure() as ProcAddTableColumns).IdentifyAppend = txtIdentifierAppend.Text;
        }

        private void txtNewColumnAddTableColumn_TextChanged(object sender, EventArgs e)
        {
            (GetSelectedWorkProcedure() as ProcAddTableColumns).NewColumn = txtNewColumnAddTableColumn.Text;
        }

        private void cbNewColumnAddTableColumn_CheckedChanged(object sender, EventArgs e)
        {
            txtNewColumnAddTableColumn.Visible = cbNewColumnAddTableColumn.Checked;
            if (!cbNewColumnAddTableColumn.Checked)
            {
                (GetSelectedWorkProcedure() as ProcAddTableColumns).NewColumn = string.Empty;
            }
        }

        private void cbCompareOldColumn_CheckedChanged(object sender, EventArgs e)
        {
            GetSelectedWorkProcedure().CopyOldColumn = cbCompareOldColumn.Checked;
        }

        private void cbCompareNewColumn_CheckedChanged(object sender, EventArgs e)
        {
            NewColumnChanged(cbCompareNewColumn, cbCompareOldColumn, lblCompareNewColumn, txtNewColumnCompare);
        }

        private void txtNewColumnCompare_TextChanged(object sender, EventArgs e)
        {
            GetSelectedWorkProcedure().NewColumn = ((TextBox)sender).Text;
        }

        private void txtCompareSourceColumn_TextChanged(object sender, EventArgs e)
        {
            (GetSelectedWorkProcedure() as ProcCompare).SourceColumn = (sender as TextBox).Text;
        }

        private void txtCompareSecondColumn_TextChanged(object sender, EventArgs e)
        {
            (GetSelectedWorkProcedure() as ProcCompare).CompareColumn = (sender as TextBox).Text;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new SettingForm(SettingForm.Tabs.Help).Show();
        }

        private void cbMergeOldColumn_CheckedChanged(object sender, EventArgs e)
        {
            (GetSelectedWorkProcedure() as ProcMerge).CopyOldColumn = (sender as CheckBox).Checked;
        }

        private void cbSubstringText_CheckedChanged(object s, EventArgs e)
        {
            var sender = s as CheckBox;
            txtSubstringText.Visible = (GetSelectedWorkProcedure() as ProcSubstring).ReplaceChecked = sender.Checked;
            if (!sender.Checked)
            {
                txtSubstringText.Text = string.Empty;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (ExportHelper.SaveWorkflows(Workflows) || ExportHelper.SaveProcedures(Procedures) || ExportHelper.SaveTolerances(Tolerances) || ExportHelper.SaveCases(Cases))
            {
                MessageHandler.MessagesYesNo(MessageBoxIcon.Warning, "Es ist ein Fehler beim Speichern aufgetreten!");
            }
            else
            {
                SetDataBefore(Procedures, Cases, Workflows, Tolerances);
                MessageHandler.MessagesOK(MessageBoxIcon.Information, "Gespeichert!");
            }
        }

        private void BtnDiscard_Click(object sender, EventArgs e)
        {
            if (MessageHandler.MessagesYesNoCancel(MessageBoxIcon.Warning, "Wollen sie die getätigten Änderungen wirklich verwerfen?") == DialogResult.Yes)
            {
                DiscardProcedures();
                DiscardTolerances();
                DiscardWorkflows();
                DiscardCases();
                MessageHandler.MessagesOK(MessageBoxIcon.Information, "Änderungen wurden verworfen");
            }
        }

        private void DiscardProcedures()
        {
            IFormatter formatter = new BinaryFormatter();
            ProceduresBefore.Seek(0, SeekOrigin.Begin);
            LoadProcedures(formatter.Deserialize(ProceduresBefore) as List<Proc>);
        }

        private void DiscardWorkflows()
        {
            IFormatter formatter = new BinaryFormatter();
            WorkflowsBefore.Seek(0, SeekOrigin.Begin);
            LoadWorkflows(formatter.Deserialize(WorkflowsBefore) as List<Work>);
        }

        private void DiscardCases()
        {
            IFormatter formatter = new BinaryFormatter();
            CasesBefore.Seek(0, SeekOrigin.Begin);
            LoadCases(formatter.Deserialize(CasesBefore) as List<Case>);
        }

        private void DiscardTolerances()
        {
            IFormatter formatter = new BinaryFormatter();
            TolerancesBefore.Seek(0, SeekOrigin.Begin);
            LoadTolerances(formatter.Deserialize(TolerancesBefore) as List<Tolerance>);
        }

        private void cbHeadersPVMExport_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ViewHelper.AddRemoveHeaderThroughCheckedListBox(dgvPVMExport, e, (CheckedListBox)sender);
        }

        private void cbShowFromTo_CheckedChanged(object sender, EventArgs e)
        {
            (GetSelectedWorkProcedure() as ProcCount).ShowFromTo = cbShowFromTo.Checked;
        }

        private void cbCount_CheckedChanged(object sender, EventArgs e)
        {
            (GetSelectedWorkProcedure() as ProcCount).CountChecked = nbCount.Visible = cbCount.Checked;
        }

        private void nbCount_ValueChanged(object sender, EventArgs e)
        {
            (GetSelectedWorkProcedure() as ProcCount).Count = (int)nbCount.Value;
        }

        private void TxtCountColumn_TextChanged(object sender, EventArgs e)
        {
            (GetSelectedWorkProcedure() as ProcCount).Column = TxtCountColumn.Text;
        }

        private void txtSubstringText_TextChanged(object sender, EventArgs e)
        {
            (GetSelectedWorkProcedure() as ProcSubstring).ReplaceText = (sender as TextBox).Text;
        }

        private void BtnMergeDefaultFormat_Click(object sender, EventArgs e)
        {
            MergeFormat format = (GetSelectedWorkProcedure() as ProcMerge).Format;
            MergeFormatView view = new MergeFormatView(format);
            if(view.ShowDialog() == DialogResult.OK)
            {
                txtFormula.Text = format.ToString();
            }

        }

        private void dgvMerge_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            if (dgvMerge.DataSource != null)
            {
                dgvMerge.CommitEdit(DataGridViewDataErrorContexts.CurrentCellChange);
                ViewHelper.EndDataGridViewEdit(dgvMerge);
                DataTable table = (dgvMerge.DataSource as DataView).Table;

                table.Rows[e.Row.Index - 1][(int)ProcMerge.ConditionColumn.Format] = new MergeFormat();
                if (dgvMerge.EditingControl is TextBox)
                {
                    TextBox box = ((TextBox)dgvMerge.EditingControl);
                    box.SelectionStart = box.Text.Length;
                    box.SelectionLength = 0;
                }
            }
        }
    }
}
