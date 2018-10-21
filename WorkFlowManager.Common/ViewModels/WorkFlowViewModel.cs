using FluentValidation.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.Validation;

namespace WorkFlowManager.Common.ViewModels
{
    public class MonitoringRoleCheckbox
    {
        public ProjectRole ProjectRole { get; set; }
        public bool IsChecked { get; set; }
    }

    [Validator(typeof(ProcessFormValidator))]
    public class ProcessForm
    {
        public int? ConditionId { get; set; }
        public string ConditionName { get; set; }
        public ProcessType ProcessType { get; set; }
        public int Id { get; set; }

        [Display(Name = "Assigned Role")]
        public ProjectRole AssignedRole { get; set; }

        [Display(Name = "Special Form Type")]
        public int? FormViewId { get; set; }

        public SelectList FormViewList { get; set; }

        public IList<MonitoringRoleCheckbox> MonitoringRoleList { get; set; }

        public string ProcessUniqueCode { get; set; }
        public int TaskId { get; set; }
        public Task Task { get; set; }

        [Display(Name = "Next Process")]
        public int? NextProcessId { get; set; }
        public SelectList MainProcessList { get; set; }

        [Display(Name = "Next Button Caption")]
        public string NextText { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }


        [Display(Name = "Analysis Information")]
        [DataType(DataType.MultilineText)]
        public string SpecialFormAnalysis { get; set; }



        [Display(Name = "Message For Monitor")]
        public string MessageForMonitor { get; set; }


        [Display(Name = "Is Description Mandatory")]
        public bool IsDescriptionMandatory { get; set; }
        [Display(Name = "Is FileUpload Mandatory")]
        public bool IsStandardForm { get; set; }

        [Display(Name = "Select Decision Method")]
        public int? DecisionMethodId { get; set; }
        public SelectList DecisionMethodList { get; set; }

        [UIHint("_FileUploadTemplate")]
        public FileUpload TemplateFileList { get; set; }

        [UIHint("_FileUploadTemplate")]
        public FileUpload AnalysisFileList { get; set; }




        public SelectList RepetitionHourList { get; set; }

        [Display(Name = "Repetition Frequence By Hour")]
        public int? RepetitionFrequenceByHour { get; set; }

        [StringLength(200)]
        public string Value { get; set; }

        [Display(Name = "Variable Name")]
        public string VariableName { get; set; }

        [Display(Name = "TaskId,VariableName List JSON")]
        public string TaskVariableList { get; set; }
    }



    public class WorkFlowIndexViewModel
    {
        public string ActiveTaskName { get; set; }
        public int ActiveTaskId { get; set; }
        public IEnumerable<Task> TaskList { get; set; }
    }

    public class WorkFlowViewModel
    {
        public int ActiveTaskId { get; set; }
        public int FirstProcessId { get; set; }
        public IEnumerable<NextProcess> NextProcessList { get; set; }
    }

    public class NextProcess
    {
        public SelectList MainProcessList { get; set; }
        public Process Process { get; set; }
    }

    public class FormViewViewModel
    {
        public int Id { get; set; }
        public int TaskId { get; set; }

        [Required(ErrorMessage = "Form Tanımını Girmelisiniz.")]
        [Display(Name = "Name")]
        public string FormName { get; set; }


        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string FormDescription { get; set; }

        [UIHint("_ComplexityEnum")]
        [Display(Name = "Form Complexity Degree")]
        public FormComplexity FormComplexity { get; set; }

        [Range(1, 20, ErrorMessage = "{0} must be between {1} and  {2}.")]
        [Display(Name = "Number of Page")]
        public int NumberOfPage { get; set; }

        public string ViewName { get; set; }

        [Display(Name = "Completed")]
        public bool Completed { get; set; }
    }

    public class DecisionMethodViewModel
    {
        public int Id { get; set; }
        public int TaskId { get; set; }

        [Required(ErrorMessage = "Method name required.")]
        [Display(Name = "Name")]
        public string MethodName { get; set; }

        [Required(ErrorMessage = "Result of Method will be [Y]es/[N]o. Please describe input and algorithm.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string MethodDescription { get; set; }


        [Display(Name = "Function Name")]
        public string MethodFunction { get; set; }

        [Display(Name = "Completed")]
        public bool Completed { get; set; }

    }

    public class SummaryOfWorkFlowViewModel
    {
        public int NumberOfCondition { get; set; }
        public int NumberOfDecisionPoint { get; set; }

        public int NumberOfProcessWhicHasSpecialForm { get; set; }
        public int NumberOfStandartProcess { get; set; }
        public int NumberOfTotalProcess { get; set; }


        public int NumberOfWillBeDesignedDecisionMethod { get; set; }

        public int NumberOfWillBeDesignedSimpleForm { get; set; }
        public int NumberOfWillBeDesignedSimplePage { get; set; }

        public int NumberOfWillBeDesignedMiddleForm { get; set; }
        public int NumberOfWillBeDesignedMiddlePage { get; set; }

        public int NumberOfWillBeDesignedComplexForm { get; set; }
        public int NumberOfWillBeDesignedComplexPage { get; set; }


        public int NumberOfTotalJobWillBeComplete { get; set; }

    }

    public class WorkFlowView
    {
        public string WorkFlowText { get; set; }
        public bool Flag { get; set; }
    }
}
