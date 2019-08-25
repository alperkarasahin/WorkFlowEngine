using AutoMapper;
using Hangfire;
using System.Web.Mvc;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Services.DbServices
{
    public class PurchasingWorkFlowProcessService : WorkFlowProcessService, IWorkFlow
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly WorkFlowDataService _workFlowDataService;

        public PurchasingWorkFlowProcessService(
                IUnitOfWork unitOfWork, WorkFlowDataService workFlowDataService)
                : base(unitOfWork, workFlowDataService)
        {
            _unitOfWork = unitOfWork;
            _workFlowDataService = workFlowDataService;
        }


        #region Decission Methods




        #endregion

        #region Workflow

        public bool FormValidate(WorkFlowFormViewModel formData, ModelStateDictionary modelState)
        {
            return base.CustomFormValidate(formData, modelState);
        }


        public void FormSave(WorkFlowFormViewModel formData)
        {
            base.CustomFormSave(formData);
        }

        public override void WorkFlowFormSave<TClass, TVM>(WorkFlowFormViewModel workFlowFormViewModel)
        {
            base.WorkFlowFormSave<TClass, TVM>(workFlowFormViewModel);
            WorkFlowTrace torSatinAlmaIslem = Mapper.Map<WorkFlowFormViewModel, WorkFlowTrace>(workFlowFormViewModel);
            AddOrUpdate(torSatinAlmaIslem);
        }

        public override void WorkFlowProcessCancel(int workFlowTraceId)
        {
            base.WorkFlowProcessCancel(workFlowTraceId);
        }

        public override void CancelWorkFlowTrace(int workFlowTraceId, int targetProcessId)
        {
            base.CancelWorkFlowTrace(workFlowTraceId, targetProcessId);
        }

        public override void GoToWorkFlowNextProcess(int ownerId)
        {
            base.GoToWorkFlowNextProcess(ownerId);
        }

        public string DecisionPointJobCall(string id, string jobId, string hourInterval)
        {
            base.DecisionPointJobCallBase(id, jobId, hourInterval);

            RecurringJob.AddOrUpdate<PurchasingWorkFlowProcessService>(jobId, x => x.DecisionPointTakeTheNextStep(int.Parse(id)), Cron.HourInterval(int.Parse(hourInterval)));
            return "OK";
        }
        #endregion

    }
}
