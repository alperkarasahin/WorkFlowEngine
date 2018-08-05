using System.ComponentModel.DataAnnotations;

namespace WorkFlowManager.Common.Enums
{
    public enum ProcessStatus
    {
        [Display(Name = "Draft")]
        Draft = 5,
        [Display(Name = "Completed")]
        Completed = 10,
        [Display(Name = "Cancelled")]
        Cancelled = 15,
    }
}
