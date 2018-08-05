using System.ComponentModel.DataAnnotations;

namespace WorkFlowManager.Common.Enums
{
    public enum Gender
    {
        [Display(Name = "Male")]
        Male = 1,
        [Display(Name = "Female")]
        Female = 2
    }
}
