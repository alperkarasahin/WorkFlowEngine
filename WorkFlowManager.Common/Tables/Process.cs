using System;
using System.Collections.Generic;
using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Extensions;

namespace WorkFlowManager.Common.Tables
{
    public class Process : BaseTable
    {
        public ProjectRole AssignedRole { get; set; }
        public int? FormViewId { get; set; }
        public FormView FormView { get; set; }
        public List<ProcessMonitoringRole> MonitoringRoleList { get; set; }
        public string ProcessUniqueCode { get; set; }
        public int TaskId { get; set; }
        public Task Task { get; set; }
        public int? NextProcessId { get; set; }
        public Process NextProcess { get; set; }
        public string NextText { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SpecialFormAnalysis { get; set; }
        public bool IsDescriptionMandatory { get; set; }
        public string MessageForMonitor { get; set; }
        public string NextLabel => (NextText == null ? "Save/Next" : NextText);
        public bool IsStandardForm => (FormViewId == null);
        public string NameWithRole => Name + " (" + AssignedRole.GetDisplayValue() + ")";
        public string FormDescription => (Description == null ? Name : Description);
        public string NotificationMessage => (MessageForMonitor == null ? Name : MessageForMonitor);
        public override string ToString()
        {
            return Name.ToString();
        }
        public Process(Task task, string name, ProjectRole assignedRole, string description = null, FormView formView = null)
        {
            Task = task;
            Name = name;
            AssignedRole = assignedRole;
            Description = description;
            FormView = formView;
            ProcessUniqueCode = Guid.NewGuid().ToString();
            DocumentList = new HashSet<Document>();
            task.AddProcess(this);
        }
        public Process()
        {

        }
    }

}
