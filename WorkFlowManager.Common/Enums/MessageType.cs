using System.ComponentModel.DataAnnotations;

namespace WorkFlowManager.Common.Enums
{
    public enum MessageType
    {
        [Display(Name = "Success!")]
        Success = 10,
        [Display(Name = "Info!")]
        Info = 20,
        [Display(Name = "Warning!")]
        Warning = 30,
        [Display(Name = "Danger!")]
        Danger = 40
    }
}
