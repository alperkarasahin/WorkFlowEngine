using System.ComponentModel.DataAnnotations;

namespace WorkFlowManager.Common.Enums
{
    public enum FileType
    {

        [Display(Name = "Analysis file")]
        AnalysisFile = 5,

        [Display(Name = "Process File")]
        ProcessFile = 10,


        [Display(Name = "Process Template File")]
        ProcessTemplateFile = 15,
    }
}
