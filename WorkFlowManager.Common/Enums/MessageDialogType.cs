using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlowManager.Common.Enums
{
    public enum MessageDialogType
    {
        [Display(Name = "Admin LTE")]
        AdminLTE = 10,
        [Display(Name = "BootStrap")]
        BootStrap = 20
    }
}
