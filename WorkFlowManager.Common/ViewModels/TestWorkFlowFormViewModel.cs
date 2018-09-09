using FluentValidation.Attributes;
using System.ComponentModel.DataAnnotations;
using WorkFlowManager.Common.Validation;

namespace WorkFlowManager.Common.ViewModels
{
    [Validator(typeof(TestWorkFlowFormViewModelValidator))]
    public class TestWorkFlowFormViewModel : WorkFlowFormViewModel
    {
        [Display(Name = "Your Age")]
        public int Age { get; set; }

    }
}
