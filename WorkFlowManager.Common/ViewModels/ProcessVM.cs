using System;
using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Extensions;

namespace WorkFlowManager.Common.ViewModels
{
    [Serializable]
    public class ProcessVM
    {
        public int Id { get; set; }
        public ProjectRole AssignedRole { get; set; }
        public int? FormViewId { get; set; }
        public string ProcessUniqueCode { get; set; }
        public int TaskId { get; set; }
        public int? NextProcessId { get; set; }
        public string NextText { get; set; }
        public string NextLabel => (NextText == null ? "Save/Next" : NextText);
        public string Name { get; set; }
        public string NameWithRole => Name + " (" + AssignedRole.GetDisplayValue() + ")";
        public string Description { get; set; }
        public string SpecialFormAnalysis { get; set; }
        public bool IsDescriptionMandatory { get; set; }
        public bool IsStandardForm => (FormViewId == null);
        public string FormDescription => (Description == null ? Name : Description);
        public string MessageForMonitor { get; set; }
        public string NotificationMessage => (MessageForMonitor == null ? string.Format("'{0}' Completed.", Name) : MessageForMonitor);
        public bool IsCondition { get; set; }
        public int? ConditionId { get; set; }
    }
}
