using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using XrmToolBox.Extensibility;
using McTools.Xrm.Connection;
using System.ServiceModel;

namespace AlexanderDevelopment.AccessTeamTemplateMover
{
    /// <summary>
    /// This is an XrmToolBox version of my console application for moving CRM access team templates between orgs - 
    ///   http://alexanderdevelopment.net/post/2014/12/13/console-application-for-moving-dynamics-crm-access-team-templates/.
    ///
    /// It copies logic from the DamSim SolutionTransferTool plugin - 
    ///   https://github.com/MscrmTools/XrmToolBox/tree/master/Plugins/DamSim.SolutionTransferTool for using multiple connections.
    /// </summary>
    public partial class AccessTeamTemplateMover : PluginControlBase
    {
        private IOrganizationService _service;
        private IOrganizationService _targetService;
        private Panel _infoPanel;
        int _counter = 0;

        //string constants for user-facing messages 
        //usually I would localize with resx files, but not sure how to easily get it working here ¯\_(ツ)_/¯ - lpa - 2015-10-20
        private const string _successMessage = "Import finished. {0} records processed.";
        private const string _errorMessage = "An error occured: {0}";
        private const string _successBoxCaption = "Success";
        private const string _errorBoxCaption = "Error";
        private const string _confirmBoxCaption = "Confirm";
        private const string _retrievingMetadataStatus = "Retrieving teamtemplate entity metadata . . . ";
        private const string _exportingStatus = "Exporting access team templates . . .";
        private const string _importingStatus = "Importing access team templates . . .";
        private const string _enablingAccessTeamStatus = "Enabling access teams for entities in target organization . . .";
        private const string _unselectedError = "You must select source and target organizations!";
        private const string _initializing = "Initializing . . .";
        private const string _sourceUnselected = "No source organization selected";
        private const string _targetUnselected = "No target organization selected";
        private const string _sourceSelectButtonText = "Select source";
        private const string _targetSelectButtonText = "Select target";
        private const string _executeButtonText = "Copy templates";
        private const string _closeButtonText = "Close tool";
        private const string _rowErrorMessage = "Error: {0}, Template: {1}";
        private const string _confirmNoAutoEnableMessage = "If this option is unchecked, access team templates will not be copied for any entities without access teams enabled in the target organization.";
        private const string _enableTargetAccessTeamsText = "Enable access teams for target organization entities?";



        public AccessTeamTemplateMover()
        {
            InitializeComponent();
        }

        /// <summary>
        /// override the UpdateConnection method so we can use two connections here
        /// </summary>
        /// <param name="newService"></param>
        /// <param name="detail"></param>
        /// <param name="actionName"></param>
        /// <param name="parameter"></param>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName = "", object parameter = null)
        {
            if (actionName == "TargetOrganization")
            {
                _targetService = newService;
                SetConnectionLabel(_targetService, "Target");
                ((OrganizationServiceProxy)((OrganizationService)_targetService).InnerService).Timeout = new TimeSpan(0, 2, 0, 0);
            }
            else
            {
                _service = newService;
                SetConnectionLabel(_service, "Source");
                ((OrganizationServiceProxy)((OrganizationService)_service).InnerService).Timeout = new TimeSpan(0, 2, 0, 0);
            }
        }

        /// <summary>
        /// method to set the source/target service and enable the execute button when both are set
        /// </summary>
        /// <param name="serviceToLabel"></param>
        /// <param name="serviceType"></param>
        private void SetConnectionLabel(IOrganizationService serviceToLabel, string serviceType)
        {
            var serviceProxy = (OrganizationServiceProxy)((OrganizationService)serviceToLabel).InnerService;
            var uri = serviceProxy.EndpointSwitch.PrimaryEndpoint;
            var hostName = uri.Host;
            string orgName;
            if (hostName.ToLower().Contains("dynamics.com"))
            {
                orgName = hostName.Split('.')[0];
                hostName = hostName.Remove(0, orgName.Length + 1);
            }
            else
            {
                orgName = uri.AbsolutePath.Substring(1);
                var index = orgName.IndexOf("/", 0, StringComparison.Ordinal);
                orgName = orgName.Substring(0, index);
            }

            var connectionName = string.Format("{0} ({1})", hostName, orgName);
            switch (serviceType)
            {
                case "Source":
                    sourceOrgLabel.Text = connectionName;
                    sourceOrgLabel.ForeColor = Color.Green;
                    break;

                case "Target":
                    targetOrgLabel.Text = connectionName;
                    targetOrgLabel.ForeColor = Color.Green;
                    break;
            }
            if(_service != null && _targetService != null)
            {
                this.executeButton.Enabled = true;
                this.executeButton.BackColor = Color.Green;
            }
            else
            {
                this.executeButton.Enabled = false;
                this.executeButton.BackColor = SystemColors.Control;
            }
        }

        /// <summary>
        /// open the window to select a source connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectSourceButton_Click(object sender, EventArgs e)
        {
            var args = new RequestConnectionEventArgs { ActionName = "SourceOrganization", Control = this };
            RaiseRequestConnectionEvent(args);
        }

        /// <summary>
        /// open the window to select a target connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectTargetButton_Click(object sender, EventArgs e)
        {
            var args = new RequestConnectionEventArgs { ActionName = "TargetOrganization", Control = this };
            RaiseRequestConnectionEvent(args);
        }

        /// <summary>
        /// close the tool tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeButton_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        private void AccessTeamTemplateMover_Load(object sender, EventArgs e)
        {
            //when form loads, set the text labels from the constants
            executeButton.Text = _executeButtonText;
            selectTargetButton.Text = _targetSelectButtonText;
            selectSourceButton.Text = _sourceSelectButtonText;
            sourceOrgLabel.Text = _sourceUnselected;
            targetOrgLabel.Text = _targetUnselected;
            closeButton.Text = _closeButtonText;
            enableTargetAccessTeamsCheckBox.Text = _enableTargetAccessTeamsText;

            //if we have a service already set, show its details on the form
            if (_service!=null)
            {
                SetConnectionLabel(_service, "Source");
            }
        }

        /// <summary>
        /// start the copy operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void executeButton_Click(object sender, EventArgs e)
        {
            //check if source and target are set - this shouldn't be necessary since the button is disabled if they're not both set, but leaving it in anyway
            if (_service != null && _targetService != null)
            {
                //reset the counter to 0
                _counter = 0;

                //display the initializing message
                _infoPanel = InformationPanel.GetInformationPanel(this, _initializing, 340, 120);

                //change the cursor
                Cursor = Cursors.WaitCursor;

                //set up and call the backgroundworker - all querying and updates are done there
                var worker = new BackgroundWorker();
                worker.DoWork += WorkerDoWork;
                worker.ProgressChanged += WorkerProgressChanged;
                worker.RunWorkerCompleted += WorkerRunWorkerCompleted;
                worker.WorkerReportsProgress = true;
                worker.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show(_unselectedError);
            }
        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var bw = (BackgroundWorker)sender;

            bw.ReportProgress(0, _retrievingMetadataStatus);

            //1. retrieve teamtemplate entity metadata
            //set attributes to exclude from the query
            List<string> IgnoredAttributes = new List<string> { "issystem" };

            //execute the teamtemplate entitymetadata request
            RetrieveEntityRequest entityreq = new RetrieveEntityRequest
            {
                LogicalName = "teamtemplate",
                EntityFilters = Microsoft.Xrm.Sdk.Metadata.EntityFilters.Attributes
            };
            RetrieveEntityResponse entityres = (RetrieveEntityResponse)_service.Execute(entityreq);

            //2. build and execute fetchxml to retrieve teamtemplate records
            string fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
            fetchXml += "<entity name='teamtemplate'>";

            foreach (AttributeMetadata amd in entityres.EntityMetadata.Attributes)
            {
                //only include attributes not in the ignore list
                if (!IgnoredAttributes.Contains(amd.LogicalName))
                {
                    fetchXml += "<attribute name='" + amd.LogicalName + "' />";
                }
            }
            fetchXml += "</entity></fetch>";

            bw.ReportProgress(0, _exportingStatus);
            EntityCollection exported = _service.RetrieveMultiple(new FetchExpression(fetchXml));


            //if we have any results
            if (exported.Entities.Count > 0)
            {
                bw.ReportProgress(0, _enablingAccessTeamStatus);

                //3. get list of unique objecttypecodes with access teams from source org
                List<int> objecttypeList = new List<int>();
                foreach (Entity entity in exported.Entities)
                {
                    if (!objecttypeList.Contains((int)entity["objecttypecode"]))
                    {
                        objecttypeList.Add((int)entity["objecttypecode"]);
                    }
                }

                //4. retrieve all entity metadata from target org so we can check whether to enable access teams
                RetrieveAllEntitiesRequest request = new RetrieveAllEntitiesRequest()
                {
                    EntityFilters = EntityFilters.Entity,
                    RetrieveAsIfPublished = false
                };
                RetrieveAllEntitiesResponse response = (RetrieveAllEntitiesResponse)_targetService.Execute(request);

                //5. loop through target entitymetadata results
                //create a list to store entities that are enabled for access teams
                List<int> enabledEntities = new List<int>();
                foreach (EntityMetadata emd in response.EntityMetadata)
                {
                    //if objecttypecode is in list from source org AND AutoCreateAccessTeams == false, set AutoCreateAccessTeams = true
                    //AutoCreateAccessTeams is nullable, so need to set separate enabled flag
                    bool enabled = false;
                    if (emd.AutoCreateAccessTeams.HasValue)
                    {
                        if ((bool)emd.AutoCreateAccessTeams)
                        {
                            //set the flag so we don't try to update it later
                            enabled = true;

                            //add it to the list of entities that are enabled for access teams
                            enabledEntities.Add((int)emd.ObjectTypeCode);
                        }
                    }

                    //6. update target entities to enable access teams if requested
                    if (objecttypeList.Contains((int)emd.ObjectTypeCode) && !enabled && enableTargetAccessTeamsCheckBox.Checked)
                    {
                        //6a. set the entity metadata AutoCreateAccessTeams value
                        emd.AutoCreateAccessTeams = true;
                        UpdateEntityRequest updateReq = new UpdateEntityRequest
                        {
                            Entity = emd,
                        };

                        //update the entity metadata
                        _targetService.Execute(updateReq);

                        //add it to the list of entities that are enabled for access teams
                        enabledEntities.Add((int)emd.ObjectTypeCode);
                    }
                }

                bw.ReportProgress(0, _importingStatus);

                //7. do the access team migration
                foreach (Entity entity in exported.Entities)
                {
                    //only copy team templates for entities that are enabled for access teams
                    if (enabledEntities.Contains((int)entity["objecttypecode"]))
                    { 
                        try
                        {
                            //7a. try to update first
                            try
                            {
                                _targetService.Update(entity);
                            }
                            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>)
                            {
                                //7b. if update fails, then create
                                _targetService.Create(entity);
                            }

                            //8. update our success counter
                            Interlocked.Increment(ref _counter);
                        }
                        catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
                        {
                            //7c. if everything fails, return error
                            throw new Exception(string.Format(_rowErrorMessage, ex.Message, entity["teamtemplatename"]));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// update the info panel the ReportProgress method is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            InformationPanel.ChangeInformationPanelMessage(_infoPanel, e.UserState.ToString());
        }

        private void WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //close the info panel
            _infoPanel.Dispose();
            Controls.Remove(_infoPanel);

            //change cursor back
            Cursor = Cursors.Default;

            //instantiate string to display message to user
            string message;

            //got an error? show it
            if (e.Error != null)
            {
                message = string.Format(_errorMessage, e.Error.Message);
                MessageBox.Show(message, _errorBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //no error? show success
                message = string.Format(_successMessage, _counter);
                MessageBox.Show(message, _successBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void enableTargetAccessTeamsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if(!this.enableTargetAccessTeamsCheckBox.Checked)
            {
                DialogResult result = MessageBox.Show(_confirmNoAutoEnableMessage, _confirmBoxCaption, MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    //do nothing
                }
                else
                {
                    //re-check the box
                    this.enableTargetAccessTeamsCheckBox.Checked = true;
                }
            }
        }
    }
}
