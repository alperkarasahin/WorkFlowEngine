using FluentValidation;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Common.Validation
{
    public class TestFormViewModelValidator : AbstractValidator<TestFormViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        public TestFormViewModelValidator()
        {

        }

        public TestFormViewModelValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            Validate();
        }

        private void Validate()
        {
            RuleFor(x => x.Age)
                .NotEmpty().WithMessage("Age required")
                .LessThan(150).WithMessage("Age must be less than 150");
        }
    }
}
