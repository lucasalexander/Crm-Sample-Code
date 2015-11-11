// --------------------------------------------------------------------------------------------------------------------
// MainForm.cs
//
// Copyright 2015 Lucas Alexander
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using AlexanderDevelopment.ConfigDataMover.Lib;

namespace AlexanderDevelopment.ConfigDataMover
{
    public partial class MainForm : Form
    {
        int _stepCounter;
        Importer _importer;

        public MainForm()
        {
            InitializeComponent();
            stepListBox.DisplayMember = "StepName";
            _stepCounter = 0;
        }

        /// <summary>
        /// adds a new step
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addStepButton_Click(object sender, EventArgs e)
        {
            _stepCounter++;
            string stepname = "step " + (_stepCounter).ToString();
            stepListBox.Items.Add(new JobStep { StepName = stepname, StepFetch = string.Empty });
        }

        /// <summary>
        /// loads the step details for editing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stepListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (stepListBox.SelectedIndex != -1)
            {
                JobStep step = (JobStep)stepListBox.SelectedItem;
                stepNameTextBox.Text = step.StepName;
                stepFetchTextBox.Text = step.StepFetch;
                updateOnlyCheckBox.Checked = step.UpdateOnly;
            }
            else
            {
                stepNameTextBox.Text = string.Empty;
                stepFetchTextBox.Text = string.Empty;
                updateOnlyCheckBox.Checked = false;
            }
        }

        /// <summary>
        /// updates the step details
        /// </summary>
        void UpdateStep()
        {
            if (stepListBox.SelectedIndex != -1)
            {
                JobStep step = (JobStep)stepListBox.SelectedItem;
                step.StepName = stepNameTextBox.Text;
                step.StepFetch = stepFetchTextBox.Text;
                step.UpdateOnly = updateOnlyCheckBox.Checked;
                stepListBox.Items[stepListBox.SelectedIndex] = stepListBox.SelectedItem;
            }
        }

        /// <summary>
        /// moves a step forward in the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void moveUpButton_Click(object sender, EventArgs e)
        {
            stepListBox.BeginUpdate();
            int numberOfSelectedItems = stepListBox.SelectedItems.Count;
            for (int i = 0; i < numberOfSelectedItems; i++)
            {
                // only if it's not the first item
                if (stepListBox.SelectedIndices[i] > 0)
                {
                    // the index of the item above the item that we wanna move up
                    int indexToInsertIn = stepListBox.SelectedIndices[i] - 1;
                    // insert UP the item that we want to move up
                    stepListBox.Items.Insert(indexToInsertIn, stepListBox.SelectedItems[i]);
                    // removing it from its old place
                    stepListBox.Items.RemoveAt(indexToInsertIn + 2);
                    // highlighting it in its new place
                    stepListBox.SelectedItem = stepListBox.Items[indexToInsertIn];
                }
            }
            stepListBox.EndUpdate();
        }

        /// <summary>
        /// moves a step backward in the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void moveDownButton_Click(object sender, EventArgs e)
        {
            stepListBox.BeginUpdate();
            int numberOfSelectedItems = stepListBox.SelectedItems.Count;
            // when going down, instead of moving through the selected items from top to bottom
            // we'll go from bottom to top, it's easier to handle this way.
            for (int i = numberOfSelectedItems - 1; i >= 0; i--)
            {
                // only if it's not the last item
                if (stepListBox.SelectedIndices[i] < stepListBox.Items.Count - 1)
                {
                    // the index of the item that is currently below the selected item
                    int indexToInsertIn = stepListBox.SelectedIndices[i] + 2;
                    // insert DOWN the item that we want to move down
                    stepListBox.Items.Insert(indexToInsertIn, stepListBox.SelectedItems[i]);
                    // removing it from its old place
                    stepListBox.Items.RemoveAt(indexToInsertIn - 2);
                    // highlighting it in its new place
                    stepListBox.SelectedItem = stepListBox.Items[indexToInsertIn - 1];
                }
            }
            stepListBox.EndUpdate();
        }

        /// <summary>
        /// removes a step
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeStepButton_Click(object sender, EventArgs e)
        {
            if (stepListBox.SelectedIndex != -1)
            {
                DialogResult result = MessageBox.Show("Are you sure you want to remove this step? There is no undo.", "Confirm step removal", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    stepListBox.Items.RemoveAt(stepListBox.SelectedIndex);
                }
            }
        }

        /// <summary>
        /// saves the job from a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveJobButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "XML file|*.xml";
            saveFileDialog1.Title = "Save job data";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                XmlDocument doc = new XmlDocument();
                XmlElement elRoot = (XmlElement)doc.AppendChild(doc.CreateElement("ConfigDataJob"));
                XmlElement elJobConfig = (XmlElement)elRoot.AppendChild(doc.CreateElement("JobConfig"));
                elJobConfig.SetAttribute("mapBuGuid", mapBuCheckBox.Checked.ToString());
                elJobConfig.SetAttribute("mapCurrencyGuid", mapCurrencyCheckBox.Checked.ToString());

                XmlElement elSteps = (XmlElement)elRoot.AppendChild(doc.CreateElement("JobSteps"));
                foreach (var item in stepListBox.Items)
                {
                    JobStep step = (JobStep)item;
                    XmlElement elStep = doc.CreateElement("Step");
                    elStep.AppendChild(doc.CreateElement("Name")).InnerText = step.StepName;
                    elStep.SetAttribute("updateOnly", updateOnlyCheckBox.Checked.ToString());
                    elStep.AppendChild(doc.CreateElement("Fetch")).InnerText = step.StepFetch;
                    elSteps.AppendChild(elStep);
                }

                XmlElement elMappings = (XmlElement)elRoot.AppendChild(doc.CreateElement("GuidMappings"));
                foreach (var item in guidMappingGridView.Rows)
                {
                    DataGridViewRow dr = (DataGridViewRow)item;
                    if (dr.Cells["sourceGuid"].Value != null && dr.Cells["targetGuid"].Value != null)
                    {
                        string sourceId = dr.Cells["sourceGuid"].Value.ToString();
                        string targetId = dr.Cells["targetGuid"].Value.ToString();
                        XmlElement elMapping = doc.CreateElement("GuidMapping");
                        elMapping.SetAttribute("source", sourceId);
                        elMapping.SetAttribute("target", targetId);
                        elMappings.AppendChild(elMapping);
                    }
                }

                if(saveConnectionsCheckBox.Checked)
                {
                    XmlElement elConnection = (XmlElement)elRoot.AppendChild(doc.CreateElement("ConnectionDetails"));
                    elConnection.SetAttribute("source", sourceTextBox.Text);
                    elConnection.SetAttribute("target", targetTextBox.Text);
                    elConnection.SetAttribute("save", "True");
                }

                StreamWriter sw = new StreamWriter(saveFileDialog1.FileName);
                sw.Write(doc.OuterXml);
                sw.Close();
            }
        }

        /// <summary>
        /// loads the job from a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadJobButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "XML file|*.xml";
            openFileDialog1.Title = "Open job file";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.StreamReader sr = new
                   System.IO.StreamReader(openFileDialog1.FileName);
                string jobdata = (sr.ReadToEnd());
                sr.Close();

                XmlDocument xml = new XmlDocument();
                try
                {
                    xml.LoadXml(jobdata);
                    stepListBox.Items.Clear();
                    guidMappingGridView.Rows.Clear();
                    saveConnectionsCheckBox.Checked = false;
                    sourceTextBox.Text = string.Empty;
                    targetTextBox.Text = string.Empty;

                    XmlNodeList stepList = xml.GetElementsByTagName("Step");
                    foreach (XmlNode xn in stepList)
                    {
                        JobStep step = new JobStep();
                        step.StepName = xn.SelectSingleNode("Name").InnerText;
                        step.StepFetch = xn.SelectSingleNode("Fetch").InnerText;
                        step.UpdateOnly = Convert.ToBoolean(xn.Attributes["updateOnly"].Value);

                        stepListBox.Items.Add(step);
                    }

                    XmlNodeList configData = xml.GetElementsByTagName("JobConfig");
                    mapBuCheckBox.Checked = Convert.ToBoolean(configData[0].Attributes["mapBuGuid"].Value);
                    mapCurrencyCheckBox.Checked = Convert.ToBoolean(configData[0].Attributes["mapCurrencyGuid"].Value);

                    XmlNodeList mappingList = xml.GetElementsByTagName("GuidMapping");
                    foreach (XmlNode xn in mappingList)
                    {
                        string sourceId = xn.Attributes["source"].Value;
                        string targetId = xn.Attributes["target"].Value;
                        guidMappingGridView.Rows.Add(sourceId, targetId);
                    }

                    XmlNodeList connectionNodes = xml.GetElementsByTagName("ConnectionDetails");
                    if(connectionNodes.Count>0)
                    {
                        sourceTextBox.Text = connectionNodes[0].Attributes["source"].Value;
                        targetTextBox.Text = connectionNodes[0].Attributes["target"].Value;
                        saveConnectionsCheckBox.Checked = Convert.ToBoolean(connectionNodes[0].Attributes["save"].Value);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Could not parse job configuration data in {0}", openFileDialog1.FileName));
                }
            }
        }

        /// <summary>
        /// clears all steps from a job
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearAllButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to remove all steps? There is no undo.", "Confirm clear all steps", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                stepListBox.Items.Clear();
                stepNameTextBox.Text = string.Empty;
                stepFetchTextBox.Text = string.Empty;
                updateOnlyCheckBox.Checked = false;
            }
        }

        /// <summary>
        /// removes a GUID mapping row
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeMappingButton_Click(object sender, EventArgs e)
        {
            if (guidMappingGridView.CurrentCell.RowIndex != -1)
            {
                DialogResult result = MessageBox.Show("Are you sure you want to remove this mapping? There is no undo.", "Confirm mapping removal", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    guidMappingGridView.Rows.RemoveAt(guidMappingGridView.CurrentCell.RowIndex);
                }
            }
        }

        /// <summary>
        /// removes validation error when a cell edit is complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void guidMappingGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            guidMappingGridView.Rows[e.RowIndex].ErrorText = String.Empty;
        }

        /// <summary>
        /// validates GUID mapping cells to make sure they are GUIDs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void guidMappingGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (guidMappingGridView.Rows[e.RowIndex] != null && !guidMappingGridView.Rows[e.RowIndex].IsNewRow && guidMappingGridView.IsCurrentRowDirty)
            {
                Guid newGuid = new Guid();
                bool isGuid = Guid.TryParse(e.FormattedValue.ToString(), out newGuid);
                if (!isGuid)
                {
                    guidMappingGridView.Rows[e.RowIndex].ErrorText = "Both values must be GUIDs. To cancel the edit for this cell, press the ESC key.";
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// clears all GUID mappings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearMappingsButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to remove all mappings? There is no undo.", "Confirm clear all mappings", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                guidMappingGridView.Rows.Clear();
            }
        }

        /// <summary>
        /// forces update of step data when name text box is left
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stepNameTextBox_Leave(object sender, EventArgs e)
        {
            UpdateStep();
        }

        /// <summary>
        /// forces update of step data when fetch text box is left
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stepFetchTextBox_Leave(object sender, EventArgs e)
        {
            UpdateStep();
        }

        /// <summary>
        /// enables copy/paste functionality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void guidMappingGridView_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Modifiers == Keys.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.C:
                            CopyFromMappingDataGridToClipboard();
                            break;

                        case Keys.V:
                            PasteFromClipboardToMappingDataGrid();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Copy/paste operation failed. " + ex.Message, "Copy/Paste", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        /// <summary>
        /// does the copy
        /// </summary>
        private void CopyFromMappingDataGridToClipboard()
        {
            //Copy to clipboard
            DataObject dataObj = guidMappingGridView.GetClipboardContent();
            if (dataObj != null)
                Clipboard.SetDataObject(dataObj);
        }

        /// <summary>
        /// does the paste
        /// </summary>
        private void PasteFromClipboardToMappingDataGrid()
        {
            try
            {
                string s = Clipboard.GetText();
                string[] lines = s.Split('\n');

                int iRow = guidMappingGridView.CurrentCell.RowIndex;
                int iCol = guidMappingGridView.CurrentCell.ColumnIndex;
                DataGridViewCell oCell;
                if (iRow + lines.Length > guidMappingGridView.Rows.Count - 1)
                {
                    bool bFlag = false;
                    foreach (string sEmpty in lines)
                    {
                        if (sEmpty == "")
                        {
                            bFlag = true;
                        }
                    }

                    int iNewRows = iRow + lines.Length - guidMappingGridView.Rows.Count;
                    if (iNewRows > 0)
                    {
                        if (bFlag)
                            guidMappingGridView.Rows.Add(iNewRows);
                        else
                            guidMappingGridView.Rows.Add(iNewRows + 1);
                    }
                    else
                        guidMappingGridView.Rows.Add(iNewRows + 1);
                }
                foreach (string line in lines)
                {
                    if (iRow < guidMappingGridView.RowCount && line.Length > 0)
                    {
                        string[] sCells = line.Split('\t');
                        for (int i = 0; i < sCells.GetLength(0); ++i)
                        {
                            if (iCol + i < this.guidMappingGridView.ColumnCount)
                            {
                                oCell = guidMappingGridView[iCol + i, iRow];
                                oCell.Value = Convert.ChangeType(sCells[i].Replace("\r", ""), oCell.ValueType);
                            }
                            else
                            {
                                break;
                            }
                        }
                        iRow++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("The data you pasted is in the wrong format for the cell");
                return;
            }
        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var bw = (BackgroundWorker)sender;

            //start the importer process
            _importer.Process();
        }

        private void WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //change cursor back
            Cursor = Cursors.Default;

            //unsubscribe from the importer's progress updates
            _importer.OnProgressUpdate -= ImportStatusUpdate;

            int errorCount = _importer.ErrorCount;

            _importer = null;
            //clear the status label
            ImportStatusUpdate("");

            //show a message to the user
            if (errorCount == 0)
            {
                MessageBox.Show("Job finished with no errors.");
            }
            else
            {
                MessageBox.Show("Job finished with errors. See the RecordError.log file for more details.");
            }
        }

        /// <summary>
        /// runs the job
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void runButton_Click(object sender, EventArgs e)
        {
            //do some basic validations
            if (string.IsNullOrEmpty(sourceTextBox.Text))
            {
                MessageBox.Show("no source connection specified");
                return;
            }
            if (string.IsNullOrEmpty(targetTextBox.Text))
            {
                MessageBox.Show("no target connection specified");
                return;
            }
            if (!(stepListBox.Items.Count>0))
            {
                MessageBox.Show("no steps in job");
                return;
            }

            //change the cursor
            Cursor = Cursors.WaitCursor;

            //prepare the list of GUID mappings to pass to the importer object
            List<GuidMapping> mappings = new List<GuidMapping>();

            foreach (var item in guidMappingGridView.Rows)
            {
                DataGridViewRow dr = (DataGridViewRow)item;
                if (dr.Cells["sourceGuid"].Value != null && dr.Cells["targetGuid"].Value != null)
                {
                    Guid sourceGuid = new Guid(dr.Cells["sourceGuid"].Value.ToString());
                    Guid targetGuid = new Guid(dr.Cells["targetGuid"].Value.ToString());
                    mappings.Add(new GuidMapping { sourceId = sourceGuid, targetId = targetGuid });
                }
            }

            //prepare the list of job steps to pass to the importer object
            List<JobStep> steps = new List<JobStep>();
            foreach (var item in stepListBox.Items)
            {
                JobStep step = (JobStep)item;
                steps.Add(step);
            }

            //instantiate the importer object and set its properties
            _importer = new Importer();
            _importer.GuidMappings = mappings;
            _importer.JobSteps = steps;
            _importer.SourceString = sourceTextBox.Text;
            _importer.TargetString = targetTextBox.Text;
            _importer.MapBaseBu = mapBuCheckBox.Checked;
            _importer.MapBaseCurrency = mapCurrencyCheckBox.Checked;

            //subscribe to the importer object progress update event
            _importer.OnProgressUpdate += ImportStatusUpdate;
            
            //set up and call the backgroundworker to do the CRM queries and writing
            var worker = new BackgroundWorker();
            worker.DoWork += WorkerDoWork;
            worker.RunWorkerCompleted += WorkerRunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// updates the status label
        /// </summary>
        /// <param name="status"></param>
        void ImportStatusUpdate(string status)
        {
            base.Invoke((Action)delegate
            {
                statusLabel.Text = status;
            });
        }


        /// <summary>
        /// updates the step when the update-only checkbox is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateStep();
        }

        private void toolStripAboutButton_Click(object sender, EventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.Show();
        }
    }
}
