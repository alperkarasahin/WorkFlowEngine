using FluentValidation.Attributes;
using System.ComponentModel.DataAnnotations;
using WorkFlowManager.Common.Validation;

namespace WorkFlowManager.Common.ViewModels
{
    [Validator(typeof(TestFormViewModelValidator))]
    public class TestFormViewModel : WorkFlowFormViewModel
    {
        public int OwnerId { get; set; }

        [Display(Name = "Your Age")]
        public int Age { get; set; }

    }
}
