using AutoMapper;
using System.Web.Mvc;
using WorkFlowManager.Common.Constants;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.Validation;
using WorkFlowManager.Common.ViewModels;
using WorkFlowManager.Helper;
using WorkFlowManager.Services.DbServices;

namespace WorkFlowManager.Services.CustomForms
{
    public class TestWorkFlowForm : IWorkFlowForm
    {
        private readonly TestWorkFlowProcessService _testWorkFlowProcessService;
        private readonly IUnitOfWork _unitOfWork;

        public TestWorkFlowForm(IUnitOfWork unitOfWork, TestWorkFlowProcessService testWorkFlowProcessService)
        {
            _unitOfWork = unitOfWork;
            _testWorkFlowProcessService = testWorkFlowProcessService;
        }

        public void Save(WorkFlowFormViewModel formData)
        {
            _testWorkFlowProcessService.WorkFlowFormSave<TestForm, TestWorkFlowFormViewModel>(formData);
        }

        public bool Validate(WorkFlowFormViewModel formData, ModelStateDictionary modelState)
        {
            return ValidationHelper.Validate((TestWorkFlowFormViewModel)formData, new TestWorkFlowFormViewModelValidator(_unitOfWork), modelState);
        }

        public WorkFlowFormViewModel Load(WorkFlowFormViewModel workFlowFormViewModel)
        {
            TestForm testForm = _unitOfWork.Repository<TestForm>().Get(x => x.OwnerId == workFlowFormViewModel.OwnerId);

            if (testForm == null)
            {
                testForm = new TestForm();
                testForm.OwnerId = workFlowFormViewModel.OwnerId;
                _unitOfWork.Repository<TestForm>().Add(testForm);
                _unitOfWork.Complete();
            }
            TestWorkFlowFormViewModel testFormViewModel = Mapper.Map<TestForm, TestWorkFlowFormViewModel>(testForm);
            Mapper.Map(workFlowFormViewModel, testFormViewModel);

            return testFormViewModel;
        }
    }
}
