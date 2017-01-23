using FluentValidation;
using FluentValidation.Results;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.DataAccess.Repositories;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Common.Validation
{
    public class FormViewValidation : AbstractValidator<FormViewViewModel>
    {
        private readonly IDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public FormViewValidation()
        {
            Validate();
        }

        public FormViewValidation(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _context = unitOfWork.GetContext();
            Validate();
        }

        private void Validate()
        {
            RuleFor(x => x.FormName)
                .Length(1, 50).WithMessage("Name must be less than 50 characters");

            RuleFor(x => x.FormDescription)
                .Length(1, 500).WithMessage("Description must be less than 500 characters");


            if (_context != null) // database validations
            {
                Custom(model =>
                {
                    //Görev içerisinde aynı isimli iki form olamaz.
                    var formView = _unitOfWork.Repository<FormView>().Get(x =>
                        (
                            x.Id != model.Id &&
                            (
                                (x.FormName == model.FormName && x.TaskId == model.TaskId)
                                ||
                                (x.ViewName == model.ViewName && x.TaskId == model.TaskId)
                            )
                        )
                    );
                    if (formView != null)
                    {
                        return new ValidationFailure("FormName", string.Format("{0} is used before. Please change it.", model.FormName));
                    }



                    return null;
                });
            }

        }
    }
}
