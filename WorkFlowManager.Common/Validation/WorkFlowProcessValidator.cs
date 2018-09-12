using FluentValidation;
using FluentValidation.Results;
using System.Linq;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Common.Validation
{
    public class WorkFlowFormViewModelValidator : AbstractValidator<WorkFlowFormViewModel>
    {


        private readonly IUnitOfWork _unitOfWork;


        public WorkFlowFormViewModelValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            Validate();
        }
        public WorkFlowFormViewModelValidator()
        {
            Validate();
        }

        private void Validate()
        {
            RuleFor(x => x.ProcessComment)
                .Length(0, 1000).WithMessage("Character size of comment must less than 1000.");

            if (_unitOfWork != null)
            {
                Custom(model =>
                {

                    bool isKosul = model.IsCondition;
                    Process process = null;

                    if (isKosul && model.ConditionOptionId == null)
                    {
                        return new ValidationFailure("ConditionOptionId", string.Format("You must chose an option"));
                    }
                    var proessList = _unitOfWork.Repository<Process>().GetList(x => x.TaskId == model.ProcessTaskId);

                    if (model.ConditionOptionId != null)
                    {
                        process = proessList.FirstOrDefault(x => x.Id == (int)model.ConditionOptionId);
                    }
                    else
                    {
                        process = proessList.FirstOrDefault(x => x.Id == model.ProcessId);
                    }
                    string descriptionPropertyName = "Description";

                    if (process.IsDescriptionMandatory)
                    {
                        if (model.Description == null)
                        {
                            return new ValidationFailure(descriptionPropertyName, string.Format("{0} is required.", descriptionPropertyName));
                        }
                    }

                    return null;
                });

            }
        }
    }

}
