using FluentValidation;
using FluentValidation.Results;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.DataAccess.Repositories;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Common.Validation
{
    public class DecisionMethodValidation : AbstractValidator<DecisionMethodViewModel>
    {
        private readonly IDbContext _context;
        private readonly IUnitOfWork _unitOfWork;


        public DecisionMethodValidation()
        {
            Validate();
        }

        public DecisionMethodValidation(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _context = unitOfWork.GetContext();
            Validate();
        }

        private void Validate()
        {
            RuleFor(x => x.MethodName)
                .Length(1, 100).WithMessage("Name must be less than 100 characters.");

            RuleFor(x => x.MethodDescription)
                .Length(1, 500).WithMessage("Description must be less than 500 characters.");


            if (_context != null) // database validations
            {
                Custom(model =>
                {
                    //Görev içerisinde aynı isimli iki form olamaz.
                    var metot = _unitOfWork.Repository<DecisionMethod>().Get(x =>
                        (
                            x.Id != model.Id &&
                            (
                                (x.MethodName == model.MethodName && x.TaskId == model.TaskId)
                                ||
                                (x.MethodFunction == model.MethodFunction && x.TaskId == model.TaskId)
                            )
                        )
                    );


                    if (metot != null && metot.MethodName.CompareTo(model.MethodName) == 0)
                    {
                        return new ValidationFailure("MethodName", string.Format("{0} used before. Please change it.", model.MethodName));
                    }

                    if (metot != null && metot.MethodFunction.CompareTo(model.MethodFunction) == 0)
                    {
                        return new ValidationFailure("MethodFunction", string.Format("{0} used before. Please change it.", model.MethodFunction));
                    }



                    return null;
                });
            }

        }
    }
}
