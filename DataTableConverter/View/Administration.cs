using CheckComboBoxTest;
using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using DataTableConverter.Classes.WorkProcs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private ViewHelper viewHelper;
        private Dictionary<Type, Action<WorkProc>> assignControls;


        internal Administration(object[] headers, ContextMenuStrip ctxRow)
        {
            InitializeComponent();
            viewHelper = new ViewHelper(ctxRow, lbUsedProcedures_SelectedIndexChanged);
            addContextMenu();
            
            setOrderList();
            assignGroupBoxToEnum();
            cmbProcedureType.SelectedIndex = 0;
            
            loadProcedures();
            loadTolerances();
            loadCases();
            loadWorkflows();
            setHeaders(headers);

            ltbProcedures_SelectedIndexChanged(null, null);
            generateProceduresForWorkflow();
            loadProceduresWorkflow(false);
            
            lbWorkflows_SelectedIndexChanged(null, null);
            
            lbTolerances_SelectedIndexChanged(null, null);

            
            lbCases_SelectedIndexChanged(null, null);
            
        }

        private void addContextMenu()
        {
            viewHelper.addContextMenuToDataGridView(dgTolerance, true);
            viewHelper.addContextMenuToDataGridView(dgCaseColumns, true);
            viewHelper.addContextMenuToDataGridView(dgvColumns, true);
            viewHelper.addContextMenuToDataGridView(dgvReplaces, true);
            viewHelper.addContextMenuToDataGridView(dgOrderColumns, false);
        }

        private void setOrderList()
        {
            OrderList = new List<KeyValuePair<string, bool>>();
            OrderList.Add(new KeyValuePair<string, bool>("Aufsteigend", false));
            OrderList.Add(new KeyValuePair<string, bool>("Absteigend", true));
        }

        private void setHeaders(object[] headers)
        {
            cbHeaders.Items.AddRange(headers);
            clbHeaderProcedure.Items.AddRange(headers);
            clbHeaderOrder.Items.AddRange(headers);
        }

        private void assignGroupBoxToEnum()
        {
            gbState = new Dictionary<GroupBox, Type>();
            gbState.Add(gbDefDuplicate, typeof(ProcDuplicate));
            gbState.Add(gbMerge, typeof(ProcMerge));
            //gbState.Add(gbTrim, ProcedureState.Trim);
            gbState.Add(gbProcedure, typeof(ProcMerge));
            gbState.Add(gbOrder, typeof(ProcOrder));

            assignControls = new Dictionary<Type, Action<WorkProc>> {
                { typeof(ProcUser), setMergeControls },
                { typeof(ProcDuplicate), setDuplicateControls },
                { typeof(ProcMerge), setMergeControls },
                { typeof(ProcOrder), setOrderControls },
                { typeof(ProcTrim), setTrimControls }
            };
        }

        private void Administration_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ExportHelper.saveWorkflows(Workflows) || ExportHelper.saveProcedures(Procedures) || ExportHelper.saveTolerances(Tolerances) || ExportHelper.saveCases(Cases))
            {
                DialogResult result = MessageHandler.MessagesYesNo(MessageBoxIcon.Warning, "Es ist ein Fehler beim Speichern aufgetreten!\nMöchten Sie das Fenster trotzdem schließen?");
                e.Cancel = result == DialogResult.No;
            }
        }

        

        #region Procedures

        private void loadProcedures()
        {
            Procedures = ImportHelper.loadProcedures();
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
            int id = maxIdProcedure() + 1;
            Proc proc = new Proc(name + number, table, id);
            while (bindingProcedure.Contains(proc))
            {
                number++;
                proc.Name = name + number;
            }
            bindingProcedure.Add(proc);
            ltbProcedures_SelectedIndexChanged(null, null);
            sortProcedureList(id);
            loadProceduresWorkflow();
        }

        private int maxIdProcedure()
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
                if (isProcedureUsed(id, out string usedWorkflows, typeof(ProcUser)))
                {
                    result = MessageHandler.MessagesYesNo(MessageBoxIcon.Warning, "Diese Funktion wird von anderen Arbeitsabläufen verwendet!\nArbeitsabläufe die diese Funktion verwenden:\n" + usedWorkflows + "\nTrotzdem löschen?");
                }
                if (result == null || result == DialogResult.Yes)
                {
                    bindingProcedure.RemoveAt(ltbProcedures.SelectedIndex);
                    resetForm();
                    ltbProcedures_SelectedIndexChanged(null, null);
                    if (result == DialogResult.Yes)
                    {
                        deleteProcedureOfWorkflows(id, typeof(ProcUser));
                    }
                }
                loadProceduresWorkflow();
            }
        }

        private void deleteProcedureOfWorkflows(int id, Type type)
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
                setWorkflowOrdinal(w);
            }
        }

        private void setWorkflowOrdinal(Work work)
        {
            for(int ordinal = 0; ordinal < work.Procedures.Count; ordinal++)
            {
                work.Procedures[ordinal].Ordinal = ordinal;
            }
        }

        private bool isProcedureUsed(int id, out string usedWorkflows, Type type)
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
                resetForm();
                groupBox1.Enabled = false;
            }
            else
            {
                groupBox1.Enabled = true;
                selectedProc = (Proc)ltbProcedures.SelectedItem;
                txtName.Text = selectedProc.Name;
                dgvReplaces.DataSource = selectedProc.Replace;
            }
        }

        private void resetForm()
        {
            txtName.Text = string.Empty;
            dgvReplaces.DataSource = null;
        }


        private void sortProcedureList(int selectedId)
        {
            Procedures.Sort();
            bindingProcedure.ResetBindings();
            ltbProcedures.SelectedIndex = getProcedureIndexThroughId(selectedId);
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            if (lbProcedures.SelectedIndex != -1)
            {
                selectedProc.Name = txtName.Text;
                sortProcedureList(selectedProc.Id);
            }
        }

        private int getProcedureIndexThroughId(int selectedId)
        {
            return Procedures.FindIndex(proc => proc.Id == selectedId);
        }
        #endregion

        #region Workflows
        private void loadWorkflows()
        {
            Workflows = ImportHelper.loadWorkflows();
            bindingWorkflow = new BindingList<Work>(Workflows);
            lbWorkflows.DataSource = bindingWorkflow;
            lbWorkflows.DisplayMember = "Name";
            lbWorkflows.ValueMember = "Id";
        }

        private void loadProceduresWorkflow(bool status = true)
        {
            lbProcedures.DataSource = null;
            lbProcedures.DataSource = getSelectedProcedureType();
            lbProcedures.DisplayMember = "Name";
            lbProcedures.ValueMember = "Id";
            if (status)
            {
                lbWorkflows_SelectedIndexChanged(null, null);
            }
        }

        private void generateProceduresForWorkflow()
        {
            SystemProc = new List<Proc>();
            SystemProc.Add(new Proc("Trim", null, 1));
            SystemProc.Add(new Proc("Spalten zusammenfügen", null, 2));
            SystemProc.Add(new Proc("Sortieren", null, 3));

            generateDuplicateProc();
        }

        private void generateDuplicateProc()
        {
            DuplicateProc = new List<Proc>();
            foreach (Case cas in Cases)
            {
                DuplicateProc.Add(new Proc(cas.Name, null, cas.Id));
            }
        }

        private List<Proc> getSelectedProcedureType()
        {
            return cmbProcedureType.SelectedIndex == 1 ? SystemProc : cmbProcedureType.SelectedIndex == 2 ? DuplicateProc : Procedures;
        }

        private void btnNewWorkflow_Click(object sender, EventArgs e)
        {
            string name = "Neuer Arbeitsablauf";
            int number = 1;
            int id = maxIdWorkflow() + 1;
            Work work = new Work(name + number, new List<WorkProc>(), id);
            while (bindingWorkflow.Contains(work))
            {
                number++;
                work.Name = name + number;
            }
            bindingWorkflow.Add(work);
            sortWorkflowList(id);
            lbWorkflows_SelectedIndexChanged(null, null);
        }

        private int maxIdWorkflow()
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


        private void sortWorkflowList(int selectedId)
        {
            Workflows.Sort();
            bindingWorkflow.ResetBindings();
            lbWorkflows.SelectedIndex = getWorkflowIndexThroughId(selectedId);
        }

        private int getWorkflowIndexThroughId(int selectedId)
        {
            return Workflows.FindIndex(work => work.Id == selectedId);
        }


        private int getWorkProcedureIndexThroughId(int selectedId)
        {
            return getSelectedWorkflow().Procedures.FindIndex(proc => proc.ProcedureId == selectedId);
        }

        private void lbWorkflows_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gbProcedure.Enabled = gbWorkflow.Enabled = (lbWorkflows.SelectedIndex != -1))
            {
                Work workflow = getSelectedWorkflow();
                setWorkflowProcedures(workflow.Procedures);
                
                txtWorkflow.Text = workflow.Name;
                lbUsedProcedures_SelectedIndexChanged(null, null);
            }
            else
            {
                txtWorkflow.Text = string.Empty;
            }
        }

        private void setWorkflowProcedures(List<WorkProc> proc, BindingList<WorkProc> newList = null)
        {
            //bindingWorkProc = new BindingList<Proc>(getProceduresWithId(proc));
            bindingWorkProc = newList ?? new BindingList<WorkProc>(proc);
            lbUsedProcedures.DataSource = null;
            lbUsedProcedures.DataSource = bindingWorkProc;
            lbUsedProcedures.DisplayMember = "Name";
            lbUsedProcedures.ValueMember = "ProcedureId";
        }

        private void sortWorkProcList(int ordinal)
        {
            getSelectedWorkflow().Procedures.Sort();
            setWorkflowProcedures(getSelectedWorkflow().Procedures);
            lbUsedProcedures.SelectedIndex = getWorkProcedureIndexThroughOrdinal(ordinal);
        }

        private int getWorkProcedureIndexThroughOrdinal(int ordinal)
        {
            return getSelectedWorkflow().Procedures.FindIndex(proc => proc.Ordinal == ordinal);
        }

        private List<Proc> getProceduresWithId(List<WorkProc> proc)
        {
            List<Proc> procs = new List<Proc>();
            foreach(WorkProc wp in proc)
            {
                procs.Add(getMergeProcedureIndexThroughId(wp.ProcedureId, wp.GetType()));
            }
            return procs;
        }

        private Proc getMergeProcedureIndexThroughId(int selectedId, Type type)
        {
            List <Proc> procs = type == typeof(ProcUser)? Procedures : type == typeof(ProcDuplicate) ? DuplicateProc : SystemProc;
            return procs[procs.FindIndex(proc => proc.Id == selectedId)];
        }

        private Work getSelectedWorkflow()
        {
            return ((Work)lbWorkflows.SelectedItem);
        }

        private void setMergeControls( WorkProc selectedProc)
        {
            setCmbHeader(selectedProc.Formula);
            lblOriginalNameText.Text = ProcMerge.ClassName;
        }

        private void setTrimControls(WorkProc selectedProc)
        {
            lblOriginalNameText.Text = ProcTrim.ClassName;
        }

        private void setUserControls(WorkProc selectedProc)
        {
            lblOriginalNameText.Text = getProcedureName(selectedProc.ProcedureId);
            cbNewColumn.Checked = selectedProc.NewColumn != string.Empty;
            dgvColumns.DataSource = selectedProc.Columns;
            setHeaderProcedure(selectedProc.Columns.Rows.Cast<DataRow>().Select(row => row[0].ToString()).ToArray());
        }

        private void setDuplicateControls(WorkProc selectedProc)
        {
            Case myCase = Cases[getCaseIndexThroughId(selectedProc.ProcedureId)];
            lblOriginalNameText.Text = myCase.Name;
            DataTable temp = myCase.Columns.Copy();
            DataTable table = new DataTable { TableName = "WorkDuplicates" };
            object[] firstColumn = temp.Rows.Cast<DataRow>().Select(dc => dc.ItemArray[0]).ToArray();

            table.Columns.Add("Bezeichnung");
            table.Columns.Add("Zuweisung");

            if (selectedProc.DuplicateColumns.Length < firstColumn.Length)
            {
                List<string> list = new List<string>();
                if (selectedProc.DuplicateColumns.Length > 0)
                {
                    list.AddRange(selectedProc.DuplicateColumns);
                }

                list.AddRange(firstColumn.Skip(selectedProc.DuplicateColumns.Length).Select(ob => ob.ToString()));


                selectedProc.DuplicateColumns = list.ToArray();
            }
            else if (selectedProc.DuplicateColumns.Length > firstColumn.Length)
            {
                selectedProc.DuplicateColumns = selectedProc.DuplicateColumns.Take(firstColumn.Length).ToArray();
            }
            for (int i = 0; i < firstColumn.Length; i++)
            {
                table.Rows.Add(new object[] { firstColumn[i], selectedProc.DuplicateColumns[i] });
            }
            dgColumnDefDuplicate.DataSource = table;
            dgColumnDefDuplicate.Columns[0].ReadOnly = true;
        }

        private void setOrderControls(WorkProc selectedProc)
        {
            setHeaderOrder(selectedProc.Columns.Rows.Cast<DataRow>().Select(row => row[0].ToString()).ToArray());
            lblOriginalNameText.Text = ProcOrder.ClassName;

            if (selectedProc.Columns.Columns.Count <= 1)
            {
                DataTable table = new DataTable { TableName = "Order" };
                table.Columns.Add("Spalte");
                table.Columns.Add("Sortierung");
                table.Columns[1].DataType = typeof(bool);
                selectedProc.Columns = table;
            }
            dgOrderColumns.DataSource = null;
            dgOrderColumns.DataSource = selectedProc.Columns;

            dgOrderColumns.Columns[1].Visible = false;

            DataGridViewComboBoxColumn cmb = new DataGridViewComboBoxColumn();


            cmb.DataSource = OrderList;
            cmb.HeaderText = "Sortierung";
            cmb.DisplayMember = "key";
            cmb.ValueMember = "value";
            cmb.DataPropertyName = "Sortierung";


            dgOrderColumns.Columns.Add(cmb);
        }

        private void setProcValues(WorkProc selectedProc)
        {
            txtWorkProcName.Text = selectedProc.Name;
            txtFormula.Text = selectedProc.Formula;
            setNewColumnText(selectedProc.NewColumn);
            setGroupBoxVisibility(selectedProc.GetType());
        }


        private void lbUsedProcedures_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gbProcedure.Enabled = (lbUsedProcedures.SelectedIndex != -1))
            {
                WorkProc selectedProc = getSelectedWorkProcedure();
                setProcValues(selectedProc);
                assignControls[selectedProc.GetType()](selectedProc);
            }
        }

        private void setGroupBoxVisibility(Type type)
        {
            foreach (GroupBox box in gbState.Keys)
            {
                box.Visible = gbState[box] == type;
            }
        }

        private void setNewColumnText(string text)
        {
            txtNewColumn.Text = txtNewColumnMerge.Text = text;
        }

        private string getProcedureName(int id)
        {
            return Procedures[getProcedureIndexThroughId(id)].Name;
        }

        private void setCmbHeader(string formula)
        {
            if (formula != null)
            {
                cbHeaders.ItemCheck -= cbHeaders_ItemCheck;
                Regex re = new Regex(@"\[(.*?)\]");
                MatchCollection matches = re.Matches(formula);
                string[] headers = new string[matches.Count];
                for (int i = 0; i < matches.Count; i++)
                {
                    string header = matches[i].Value.Substring(1, matches[i].Value.Length - 2);
                    int index;
                    if ((index = cbHeaders.Items.IndexOf(header)) != -1)
                    {
                        cbHeaders.SetItemChecked(index, true);
                    }
                }
                cbHeaders.ItemCheck += cbHeaders_ItemCheck;
            }
        }

        private void setHeaderProcedure(string[] headers)
        {
            clbHeaderProcedure.ItemCheck -= clbHeaderProcedure_ItemCheck;
            setChecked(clbHeaderProcedure, headers);
            clbHeaderProcedure.ItemCheck += clbHeaderProcedure_ItemCheck;
        }

        private void setChecked(CheckedComboBox box, string[] headers)
        {
            for (int i = 0; i < box.Items.Count; i++)
            {
                box.SetItemChecked(i, headers.Contains(box.Items[i].ToString()));
            }
        }

        private void setHeaderOrder(string[] headers)
        {
            clbHeaderOrder.ItemCheck -= clbHeaderOrder_ItemCheck;
            setChecked(clbHeaderOrder, headers);
            clbHeaderOrder.ItemCheck += clbHeaderOrder_ItemCheck;
        }

        private WorkProc getSelectedWorkProcedure()
        {
            return getSelectedWorkflow().Procedures[lbUsedProcedures.SelectedIndex];
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
                Work workflow = getSelectedWorkflow();
                WorkProc newProc;
                switch ((int)lbProcedures.SelectedValue)
                {
                    //System-Proc
                    case 1:
                        switch (cmbProcedureType.SelectedIndex)
                        {
                            case 1:
                                newProc = new ProcTrim(workflow.Procedures.Count, (int)lbProcedures.SelectedValue, cmbProcedureType.SelectedIndex, ((Proc)lbProcedures.SelectedItem).Name);
                                break;

                            case 2:
                                newProc = new ProcMerge(workflow.Procedures.Count, (int)lbProcedures.SelectedValue, cmbProcedureType.SelectedIndex, ((Proc)lbProcedures.SelectedItem).Name);
                                break;

                            default:
                                newProc = new ProcOrder(workflow.Procedures.Count, (int)lbProcedures.SelectedValue, cmbProcedureType.SelectedIndex, ((Proc)lbProcedures.SelectedItem).Name);
                                break;
                        }
                        break;

                    //Duplicate
                    case 2:
                        newProc = new ProcDuplicate(workflow.Procedures.Count, (int)lbProcedures.SelectedValue, cmbProcedureType.SelectedIndex, ((Proc)lbProcedures.SelectedItem).Name);
                        break;

                    //User-Proc
                    default:
                        newProc = new ProcUser(workflow.Procedures.Count, (int)lbProcedures.SelectedValue, cmbProcedureType.SelectedIndex, ((Proc)lbProcedures.SelectedItem).Name);
                        break;
                }
                workflow.Procedures.Add(new WorkProc(workflow.Procedures.Count, (int)lbProcedures.SelectedValue, cmbProcedureType.SelectedIndex, ((Proc)lbProcedures.SelectedItem).Name));

                setWorkflowProcedures(workflow.Procedures);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (lbUsedProcedures.SelectedIndex != -1)
            {
                Work workflow = getSelectedWorkflow();
                workflow.Procedures.RemoveAt(lbUsedProcedures.SelectedIndex);
                for(int i = lbUsedProcedures.SelectedIndex; i < workflow.Procedures.Count; i++)
                {
                    workflow.Procedures[i].Ordinal--;
                }
                setWorkflowProcedures(workflow.Procedures);
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            if(lbUsedProcedures.SelectedIndex != -1)
            {
                Work workflow = getSelectedWorkflow();
                WorkProc wp = workflow.Procedures[lbUsedProcedures.SelectedIndex];
                if(wp.Ordinal > 0)
                {
                    workflow.Procedures[lbUsedProcedures.SelectedIndex - 1].Ordinal++;
                    wp.Ordinal--;
                    sortWorkProcList(wp.Ordinal);
                }
            }            
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            if (lbUsedProcedures.SelectedIndex != -1)
            {
                Work workflow = getSelectedWorkflow();
                WorkProc wp = workflow.Procedures[lbUsedProcedures.SelectedIndex];
                if (wp.Ordinal < workflow.Procedures.Count -1)
                {
                    workflow.Procedures[lbUsedProcedures.SelectedIndex + 1].Ordinal--;
                    wp.Ordinal++;
                    sortWorkProcList(wp.Ordinal);
                }
            }
        }

        private void cbNewColumn_CheckedChanged(object sender, EventArgs e)
        {
            lblNewColumn.Visible = txtNewColumn.Visible = cbNewColumn.Checked;
            if (!txtNewColumn.Visible)
            {
                txtNewColumn.Text = string.Empty;
                txtNewColumn_TextChanged(txtNewColumn, e);
            }
        }

        private void txtNewColumn_TextChanged(object sender, EventArgs e)
        {
            getSelectedWorkflow().Procedures[lbUsedProcedures.SelectedIndex].NewColumn = ((TextBox)sender).Text;
        }

        private void txtFormula_TextChanged(object sender, EventArgs e)
        {
            getSelectedWorkflow().Procedures[lbUsedProcedures.SelectedIndex].Formula = txtFormula.Text;
        }

        private void addColumnToFormula(string column)
        {
            if (!txtFormula.Text.Contains(column))
            {
                string separator = txtFormula.Text.Length > 0 ? " " : string.Empty;
                txtFormula.Text = $"{txtFormula.Text}{separator}[{column}]";
            }
        }

        private void removeColumnOfFormula(string column)
        {
            txtFormula.Text = txtFormula.Text.Replace($" [{column}]", "");
        }

        private void cbHeaders_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                addColumnToFormula(cbHeaders.Items[e.Index].ToString());
            }
            else
            {
                removeColumnOfFormula(cbHeaders.Items[e.Index].ToString());
            }
            getSelectedWorkflow().Procedures[lbUsedProcedures.SelectedIndex].Formula = txtFormula.Text;
        }

        private void txtWorkflow_TextChanged(object sender, EventArgs e)
        {
            if (lbWorkflows.SelectedIndex != -1)
            {
                getSelectedWorkflow().Name = txtWorkflow.Text;
            }
        }

        private void cmbProcedureType_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadProceduresWorkflow();
        }

        #endregion


        #region Tolerances


        private void loadTolerances()
        {
            Tolerances = ImportHelper.loadTolerances();
            bindingTolerance = new BindingList<Tolerance>(Tolerances);
            lbTolerances.DataSource = bindingTolerance;
            lbTolerances.DisplayMember = "Name";
            lbTolerances.ValueMember = "Name";
        }


        private void sortToleranceList(int selectedId)
        {
            Tolerances.Sort();
            bindingTolerance.ResetBindings();
            lbTolerances.SelectedIndex = getToleranceIndexThroughId(selectedId);
        }

        private int getToleranceIndexThroughId(int selectedId)
        {
            return Tolerances.FindIndex(tol => tol.Id == selectedId);
        }

        private void btnNewTolerance_Click(object sender, EventArgs e)
        {
            string name = "Neue Toleranz";
            int number = 1;
            DataTable table = new DataTable { TableName = "Tolerance" };
            table.Columns.Add("Bezeichnung", typeof(string));

            int id = maxIdTolerance() + 1;
            Tolerance proc = new Tolerance(name + number, table, id);
            while (bindingTolerance.Contains(proc))
            {
                number++;
                proc.Name = name + number;
            }
            bindingTolerance.Add(proc);
            sortToleranceList(id);
            lbTolerances_SelectedIndexChanged(null, null);
        }

        private int maxIdTolerance()
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
                dgTolerance.DataSource = selectedTolerance.Columns;
            }
        }

        private void txtToleranceName_TextChanged(object sender, EventArgs e)
        {
            if(lbTolerances.SelectedIndex != -1)
            {
                Tolerance selectedTolerance = (Tolerance)lbTolerances.SelectedItem;
                selectedTolerance.Name = txtToleranceName.Text;
                sortToleranceList(selectedTolerance.Id);
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

            int id = maxIdCase() + 1;
            Case proc = new Case(name + number, string.Empty, table, id);
            while (bindingCase.Contains(proc))
            {
                number++;
                proc.Name = name + number;
            }
            bindingCase.Add(proc);
            sortCaseList(id);
            lbCases_SelectedIndexChanged(null, null);
            generateDuplicateProc();
            loadProceduresWorkflow();
        }

        private void loadCases()
        {
            Cases = ImportHelper.loadCases();
            bindingCase = new BindingList<Case>(Cases);
            lbCases.DataSource = bindingCase;
            lbCases.DisplayMember = "Name";
            lbCases.ValueMember = "Name";
        }


        private void sortCaseList(int selectedId)
        {
            Cases.Sort();
            bindingCase.ResetBindings();
            lbCases.SelectedIndex = getCaseIndexThroughId(selectedId);
        }

        private int getCaseIndexThroughId(int selectedId)
        {
            return Cases.FindIndex(tol => tol.Id == selectedId);
        }

        private int maxIdCase()
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
                dgCaseColumns.DataSource = null;
            }
            else
            {
                Case selectedCase = (Case)lbCases.SelectedItem;
                txtCaseName.Text = selectedCase.Name;
                txtShortcut.Text = selectedCase.Shortcut;
                dgCaseColumns.DataSource = selectedCase.Columns;
            }
        }

        private void txtCaseName_TextChanged(object sender, EventArgs e)
        {
            if (lbCases.SelectedIndex != -1)
            {
                Case selectedCase = (Case)lbCases.SelectedItem;
                selectedCase.Name = txtCaseName.Text;
                sortCaseList(selectedCase.Id);

                generateDuplicateProc();
                loadProceduresWorkflow();
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
                if (isProcedureUsed(id, out string usedWorkflows, typeof(ProcDuplicate)))
                {
                    result = MessageHandler.MessagesYesNo(MessageBoxIcon.Warning, "Dieser Fall wird von anderen Arbeitsabläufen verwendet!\nArbeitsabläufe die diese Funktion verwenden:\n" + usedWorkflows + "\nTrotzdem löschen?");
                }
                if (result == null || result == DialogResult.Yes)
                {
                    bindingCase.RemoveAt(lbCases.SelectedIndex);
                    lbCases_SelectedIndexChanged(null, null);
                    if (result == DialogResult.Yes)
                    {
                        deleteProcedureOfWorkflows(id, typeof(ProcDuplicate));
                    }
                }
                generateDuplicateProc();
                loadProceduresWorkflow();
            }
        }

        private void dgCaseColumns_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            ViewHelper.handleDataGridViewNumber(sender, e);
        }




        #endregion

        private void txtWorkProcName_TextChanged(object sender, EventArgs e)
        {
            lbUsedProcedures.SelectedIndexChanged -= lbUsedProcedures_SelectedIndexChanged;
            int index = lbUsedProcedures.SelectedIndex;

            getSelectedWorkflow().Procedures[lbUsedProcedures.SelectedIndex].Name = txtWorkProcName.Text;
            bindingWorkProc[lbUsedProcedures.SelectedIndex].Name = txtWorkProcName.Text;

            setWorkflowProcedures(null, bindingWorkProc);
            lbUsedProcedures.SelectedIndex = index;

            lbUsedProcedures.SelectedIndexChanged += lbUsedProcedures_SelectedIndexChanged;
        }

        private void dgCaseColumns_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            dgCaseColumns.BindingContext[dgCaseColumns.DataSource].EndCurrentEdit();
            lbUsedProcedures_SelectedIndexChanged(null, null);
        }

        private void zwischenablageEinfügenToolStripMenuItem_Click(object sender, EventArgs e, int selectedRow)
        {
            ViewHelper.insertClipboardToDataGridView((DataGridView)sender, selectedRow);
        }

        private void dgColumnDefDuplicate_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            WorkProc selecteWorkProc = getSelectedWorkProcedure();
            selecteWorkProc.DuplicateColumns = getDataSource(dgColumnDefDuplicate).Rows.Cast<DataRow>().Select(row => row.ItemArray[1].ToString()).ToArray();
        }

        private DataTable getDataSource(DataGridView dgv)
        {
            dgv.BindingContext[dgv.DataSource].EndCurrentEdit();
            return ((DataTable)dgv.DataSource).Copy();
        }

        private void clbHeaderProcedure_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                dgvColumns.Rows.Add(clbHeaderProcedure.Items[e.Index]);
            }
            else
            {
                bool found = false;
                for(int i = dgvColumns.Rows.Count-1; i >= 0 && !found; i--)
                {
                    if(dgvColumns.Rows[i].Cells[0] == clbHeaderProcedure.Items[e.Index])
                    {
                        dgvColumns.Rows.RemoveAt(i);
                        found = true;
                    }
                }
            }
        }

        private void clbHeaderOrder_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            WorkProc selectedProc = getSelectedWorkProcedure();
            DataTable table = getDataSource(dgOrderColumns);
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
            assignDataSource(table, dgOrderColumns);
        }

        private void assignDataSource(DataTable table, DataGridView sender)
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
    }
}
