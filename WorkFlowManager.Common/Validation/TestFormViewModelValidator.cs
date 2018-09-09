using FluentValidation;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Common.Validation
{
    public class TestWorkFlowFormViewModelValidator : AbstractValidator<TestWorkFlowFormViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        public TestWorkFlowFormViewModelValidator()
        {

        }

        public TestWorkFlowFormViewModelValidator(IUnitOfWork unitOfWork)
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
