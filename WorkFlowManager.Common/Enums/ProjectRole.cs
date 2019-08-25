using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WorkFlowManager.Common.Enums
{
    public enum ProjectRole
    {
        [Display(Name = "Admin")]
        Admin = 10,

        [Display(Name = "Monitor")]
        Monitor = 70,

        [Display(Name = "Officer")]
        Officer = 80,

        [Display(Name = "Unit Purchasing Officer")]
        UnitPurchasingOfficer = 90,

        [Display(Name = "Spending Officer")]
        SpendingOfficer = 100,

        [Display(Name = "Purchasing Officer")]
        PurchasingOfficer = 110,

        [Display(Name = "System")]
        System = 1000
    }
}
