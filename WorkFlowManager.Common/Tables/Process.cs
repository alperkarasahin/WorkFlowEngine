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
        public string NextLabel => (NextText == null ? "Save/Next" : NextText);
        public string Name { get; set; }
        public string NameWithRole => Name + " (" + AssignedRole.GetDisplayValue() + ")";
        public string Description { get; set; }
        public string SpecialFormAnalysis { get; set; }
        public bool IsDescriptionMandatory { get; set; }
        public bool IsFileUploadMandatory { get; set; }
        public bool IsStandardForm => (FormViewId == null);
        public string FormDescription => (Description == null ? Name : Description);
        public string MessageForMonitor { get; set; }
        public string NotificationMessage => (MessageForMonitor == null ? string.Format("'{0}' Completed.", Name) : MessageForMonitor);
        public override string ToString()
        {
            return Name.ToString();
        }
        public Process()
        {
            DocumentList = new HashSet<Document>();
        }
    }

}
