using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.Validation;

namespace WorkFlowManager.Common.ViewModels
{
    [Validator(typeof(WorkFlowFormViewModelValidator))]
    public class WorkFlowFormViewModel
    {
        public int ProcessId { get; set; }

        [Display(Name = "Form Description")]
        public string Description { get; set; }
        public string ProcessDescription { get; set; }

        public int? ConditionOptionId { get; set; }
        public List<ProcessVM> ListOfOptions { get; set; }

        public int Id { get; set; }

        [Display(Name = "Comment")]
        [DataType(DataType.MultilineText)]
        public string ProcessComment { get; set; }
        public string ProcessName { get; set; }
        public string ProcessFormViewViewName { get; set; }
        public string ProcessNextLabel { get; set; }

        public int OwnerId { get; set; }

        public bool IsCondition { get; set; }

        public ProcessStatus ProcessStatus { get; set; }

        public IEnumerable<WorkFlowTraceVM> ProgressProcessList { get; set; }
        public IEnumerable<UserProcessViewModel> AuthorizedProcessList { get; set; }
        public IEnumerable<UserProcessViewModel> TargetProcessListForCancel { get; set; }

        public bool ProcessFormViewCompleted { get; set; }
        public int ProcessTaskId { get; set; }
        public string ProcessTaskName { get; set; }
        public ProjectRole ProcessAssignedRole { get; set; }
        public bool IptalEdilebilir { get; set; }
        public string IptalText { get; set; }
        public string GorevTanim { get; set; }

        //İşlem geri alınırken kullanılacak
        public int TargetProcessId { get; set; }
        public string ProcessTaskController { get; set; }
        public string ProcessTaskSpecialFormTemplateView { get; set; }

        public List<BusinessProcess> SubProcessList { get; set; }
    }

    [Serializable]
    public class UserProcessViewModel
    {
        public string CreatedBy { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string UpdaterName { get; set; }
        public string CreatorName { get; set; }
        public string LastlyModifierName => UpdaterName ?? CreatorName;
        public DateTime? LastlyModifiedTime => UpdatedTime ?? CreatedTime;
        public int Id { get; set; }
        public string Controller { get; set; }
        public string SpecialFormTemplateView { get; set; }
        public ProcessStatus ProcessStatus { get; set; }
        public ProjectRole AssignedRole { get; set; }
        public string Description { get; set; }
        public List<Document> IslemBelgeListesi { get; set; }
        public string TaskName { get; set; }
        public List<ProcessMonitoringRole> ProcessMonitoringRolList { get; set; }
        public int OwnerId { get; set; }
        public int TaskId { get; set; }
        public bool IsCondition { get; set; }
        public int? ConditionOptionId { get; set; }
        public int ProcessId { get; set; }
        public string ProcessVariableName { get; set; }

        public string ProcessUniqueCode { get; set; }

        public int? NextProcessId { get; set; }
        public string ProcessDescription { get; set; }
        public string ProcessNotificationMessage { get; set; }
        public string ProcessName { get; set; }
    }
}
