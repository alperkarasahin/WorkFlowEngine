using AutoMapper;
using System;
using System.Web.Mvc;
using WorkFlowManager.Common.Constants;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Dto;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.Validation;
using WorkFlowManager.Common.ViewModels;
using WorkFlowManager.Helper;
using WorkFlowManager.Services.DbServices;

namespace WorkFlowManager.Services.Constants
{
    public class TestConstants : IConstants
    {
        private readonly TestWorkFlowProcessService _testWorkFlowProcessService;
        private readonly IUnitOfWork _unitOfWork;

        public TestConstants(IUnitOfWork unitOfWork, TestWorkFlowProcessService testWorkFlowProcessService)
        {
            _unitOfWork = unitOfWork;
            _testWorkFlowProcessService = testWorkFlowProcessService;
        }
        public void BelgeTuruAyarla(ServiceDTO serviceDto)
        {

        }

        public void Kaydet(WorkFlowFormViewModel formData)
        {
            _testWorkFlowProcessService.WorkFlowFormKaydet<TestForm, TestFormViewModel>(formData);
        }

        public bool Validate(WorkFlowFormViewModel formData, ModelStateDictionary modelState)
        {
            return ValidationHelper.Validate((TestFormViewModel)formData, new TestFormViewModelValidator(_unitOfWork), modelState);
        }

        public Type ViewModelType()
        {
            return typeof(TestFormViewModel);
        }

        public WorkFlowFormViewModel Yukle(ServiceDTO serviceDto)
        {
            WorkFlowTraceForm traceForm = serviceDto.workFlowTraceForm;

            TestForm testForm = _unitOfWork.Repository<TestForm>().Get(x => x.OwnerId == traceForm.OwnerId);

            if (testForm == null)
            {
                testForm = new TestForm();
                testForm.OwnerId = traceForm.OwnerId;
                _unitOfWork.Repository<TestForm>().Add(testForm);
            }


            TestFormViewModel testFormViewModel = Mapper.Map<TestForm, TestFormViewModel>(testForm);


            return testFormViewModel;
        }
    }
}
