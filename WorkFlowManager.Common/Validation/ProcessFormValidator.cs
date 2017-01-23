using FluentValidation;
using FluentValidation.Results;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.DataAccess.Repositories;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Common.Validation
{
    public class ProcessFormValidator : AbstractValidator<ProcessForm>
    {
        private readonly IDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public ProcessFormValidator()
        {
            Validate();
        }

        public ProcessFormValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _context = unitOfWork.GetContext();
            Validate();
        }
        private void Validate()
        {
            RuleFor(x => x.Description)
                .Length(1, 500).WithMessage("Description length must be less than 500 characters");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .Length(1, 100).WithMessage("Name length must be less than 100 characters");

            RuleFor(x => x.MessageForMonitor)
                .Length(1, 100).WithMessage("Message length must be less than 100 characters");


            Custom(model =>
            {
                if (model.ProcessType == Enums.ProcessType.Process)
                {
                    if (model.IsStandardForm == true)
                    {
                        //if (model.StantartFormDosyaYuklemeZorunlu == false && model.StandartFormAciklamaZorunlu == false)
                        //{
                        //    return new ValidationFailure("StandartFormAciklamaZorunlu", string.Format("En az bir stantart form bilgisi zorunlu olarak seçilmeli."));
                        //}
                    }
                    else //Özel Form ise
                    {
                        // Form türü seçilmiş olmalı
                        //Analiz girilmiş olmalı
                        //Dokümanlar yüklenmiş olmalı
                        //if (model.FormTur == null)
                        //{
                        //    return new ValidationFailure("FormTur", string.Format("Şablon belge türünü seçmelisiniz."));
                        //}

                        if (model.FormViewId == null)
                        {
                            return new ValidationFailure("FormViewId", string.Format("Form must be selected"));
                        }
                        if (model.SpecialFormAnalysis == null)
                        {
                            return new ValidationFailure("SpecialFormAnalysis", string.Format("Analysis is required."));
                        }

                    }
                }
                else if (model.ProcessType == Enums.ProcessType.DecisionPoint)
                {
                    if (model.DecisionMethodId == null)
                    {
                        return new ValidationFailure("DecisionMethodId", string.Format("Decision method is required."));
                    }
                }
                return null;
            });

            //if (_context != null) // database validations
            //{
            //    Custom(model =>
            //    {
            //        bool ayniKodluProjeOlamaz = _unitOfWork.Repository<Proje>().GetAll().Any(x => x.Kodu == model.Kodu && x.Id != model.ProjeId);

            //        if (ayniKodluProjeOlamaz)
            //            return new ValidationFailure("Kodu", string.Format("{0} kodu farklı bir projede kullanılmış. Lütfen farklı bir kod giriniz.", model.Kodu));

            //        return null;
            //    });
            //}

        }
    }
}
