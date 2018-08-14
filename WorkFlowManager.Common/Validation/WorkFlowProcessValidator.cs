using FluentValidation;
using FluentValidation.Results;
using System.Linq;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Common.Validation
{
    public class WorkFlowTraceFormValidator : AbstractValidator<WorkFlowTraceForm>
    {


        private readonly IUnitOfWork _unitOfWork;


        public WorkFlowTraceFormValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            Validate();
        }
        public WorkFlowTraceFormValidator()
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
                    Process gorevIslem = null;

                    if (isKosul && model.ConditionOptionId == null)
                    {
                        return new ValidationFailure("ConditionOptionId", string.Format("You must chose an option"));
                    }
                    var gorevIslemListesi = _unitOfWork.Repository<Process>().GetList(x => x.TaskId == model.ProcessTaskId);

                    if (model.ConditionOptionId != null)
                    {
                        gorevIslem = gorevIslemListesi.FirstOrDefault(x => x.Id == (int)model.ConditionOptionId);
                    }
                    else
                    {
                        gorevIslem = gorevIslemListesi.FirstOrDefault(x => x.Id == model.ProcessId);
                    }

                    //Bundan sonra ki süreçlerde ozel formlu ya da standart form olmasına göre property ismi değişecek
                    string propertyNameFormAciklama = "Description";

                    if (model.ProcessFormViewViewName != null)
                    {
                        propertyNameFormAciklama = "WorkFlowIslemForm.Description";
                    }

                    if (gorevIslem.IsDescriptionMandatory)
                    {
                        if (model.ProcessComment == null)
                        {
                            return new ValidationFailure(propertyNameFormAciklama, string.Format("{0} comment required.", gorevIslem.Description));
                        }
                    }

                    return null;
                });

            }
        }
    }

}
