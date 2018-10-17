using AutoMapper;
using System.Linq;
using System.Web.Mvc;
using WorkFlowManager.Common.Constants;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.ViewModels;
using WorkFlowManager.Services.DbServices;

namespace WorkFlowManager.Services.CustomForms
{
    public class HealthInformationWorkFlowForm : IWorkFlowForm
    {
        private readonly TestWorkFlowProcessService _testWorkFlowProcessService;
        private readonly IUnitOfWork _unitOfWork;

        public HealthInformationWorkFlowForm(IUnitOfWork unitOfWork, TestWorkFlowProcessService testWorkFlowProcessService)
        {
            _unitOfWork = unitOfWork;
            _testWorkFlowProcessService = testWorkFlowProcessService;
        }

        public void Save(WorkFlowFormViewModel formData)
        {
        }

        public bool Validate(WorkFlowFormViewModel formData, ModelStateDictionary modelState)
        {
            return true;
        }

        public WorkFlowFormViewModel Load(WorkFlowFormViewModel workFlowFormViewModel)
        {
            var healthInformationFormList = _unitOfWork.Repository<HealthInformationForm>().GetList(x => x.OwnerId == workFlowFormViewModel.OwnerId).ToList();

            if (healthInformationFormList.Count() == 0)
            {
                var physicalExamination = new HealthInformationForm() { Name = "Physical Examination", OwnerId = workFlowFormViewModel.OwnerId };
                var psychotechniqueResult = new HealthInformationForm() { Name = "Psychotechnique Result", OwnerId = workFlowFormViewModel.OwnerId };

                _unitOfWork.Repository<HealthInformationForm>().Add(physicalExamination);
                _unitOfWork.Repository<HealthInformationForm>().Add(psychotechniqueResult);
                _unitOfWork.Complete();
                healthInformationFormList.Add(physicalExamination);
                healthInformationFormList.Add(psychotechniqueResult);
            }
            HealthInformationFormViewModel healthInformationForm = new HealthInformationFormViewModel { HealthInformationFormList = healthInformationFormList };

            Mapper.Map(workFlowFormViewModel, healthInformationForm);

            return healthInformationForm;
        }


    }
}
