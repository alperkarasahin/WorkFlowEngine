using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlowManager.Common.Enums
{
    public enum Gender
    {
        [Display(Name = "Erkek")]
        Male = 1,
        [Display(Name = "Kadın")]
        Female = 2
    }
}
