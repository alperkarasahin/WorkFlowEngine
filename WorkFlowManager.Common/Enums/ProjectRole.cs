using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WorkFlowManager.Common.Enums
{
    public enum ProjectRole
    {
        [Description("SYSTEM")]
        [Display(Name = "S-Admin")]
        Admin = 10,


        [Description("SYSTEM")]
        [Display(Name = "S-Monitor")]
        Monitor = 70,

        [Description("UNIT")]
        [Display(Name = "U-Project Manager")]
        ProjectManager = 80,

        [Description("UNIT")]
        [Display(Name = "U-Project Procurement Officer")]
        ProjectProcurmentOfficer = 100,

        [Description("UNIT")]
        [Display(Name = "U-Project Finance Officer")]
        ProjectFinanceOfficer = 110,

        [Description("APPLICATION")]
        [Display(Name = "Sistem")]
        Sistem = 1000
    }
}
