using System.ComponentModel.DataAnnotations;

namespace WorkFlowManager.Common.Enums
{
    public enum FormComplexity
    {
        [Display(Name = "Easy")]
        Easy = 5,

        [Display(Name = "Middle")]
        Middle = 10,

        [Display(Name = "Complex")]
        Complex = 15
    }
}
