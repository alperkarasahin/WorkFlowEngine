using AutoMapper;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Dto;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.ViewModels;
using WorkFlowManager.Services.Constants;

namespace WorkFlowManager.Services.DbServices
{
    public class TestWorkFlowProcessService : WorkFlowProcessService, IWorkFlow
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly WorkFlowDataService _workFlowDataService;
        List<FormObject> tamamlanmisFormlar = null;

        public TestWorkFlowProcessService(
                IUnitOfWork unitOfWork, WorkFlowDataService workFlowDataService)
                : base(unitOfWork, workFlowDataService)
        {
            _unitOfWork = unitOfWork;
            _workFlowDataService = workFlowDataService;

            tamamlanmisFormlar = new List<FormObject>
            {
                new FormObject(new TestConstants(_unitOfWork, this))
            };
        }


        #region Decission Methods
        public char ReturnYes()
        {
            return 'Y';
        }

        public char ReturnNo()
        {
            return 'N';
        }

        private int GetOwnerIdFromId(int id)
        {
            var workFlowTrace = _unitOfWork.Repository<WorkFlowTrace>().Get(x => x.Id == id);
            int rslt = -1;
            if (workFlowTrace != null)
            {
                rslt = workFlowTrace.OwnerId;
            }
            return rslt;
        }
        public char IsAgeLessThan20(string id)
        {
            var rslt = 'N';
            int ownerId = GetOwnerIdFromId(int.Parse(id));
            var testForm = _unitOfWork.Repository<TestForm>().Get(x => x.OwnerId == ownerId);
            if (testForm != null)
            {
                if (testForm.Age < 20)
                {
                    rslt = 'Y';
                }
            }
            return rslt;
        }


        public char IsAgeGreaterThan20(string id)
        {
            var rslt = 'N';
            int ownerId = GetOwnerIdFromId(int.Parse(id));
            var testForm = _unitOfWork.Repository<TestForm>().Get(x => x.OwnerId == ownerId);
            if (testForm != null)
            {
                if (testForm.Age > 20)
                {
                    rslt = 'Y';
                }
                else
                {
                    testForm.Age = testForm.Age + 1;
                    _unitOfWork.Repository<TestForm>().Update(testForm);
                    _unitOfWork.Complete();
                }
            }
            return rslt;
        }
        #endregion

        #region Workflow İşlemleri Bölümü
        public int StartWorkFlow(int ownerId, int taskId)
        {

            var task = _unitOfWork.Repository<Task>().Get(taskId);

            WorkFlowTrace workFlowTrace = null;

            workFlowTrace = new WorkFlowTrace()
            {
                ProcessId = (int)task.StartingProcessId,
                OwnerId = ownerId,
                ProcessStatus = Common.Enums.ProcessStatus.Draft
            };
            AddOrUpdate(workFlowTrace);
            return workFlowTrace.Id;
        }


        public WorkFlowFormViewModel SatinAlmaOzelForm<T>(int ownerId, WorkFlowTraceForm satinAlmaIslemForm) where T : WorkFlowFormViewModel
        {
            return base.WorkFlowFormYukle<T>(ownerId, satinAlmaIslemForm, tamamlanmisFormlar);
        }

        public bool OzelFormValidate(WorkFlowFormViewModel formData, ModelStateDictionary modelState)
        {
            return base.OzelFormValidate(formData, modelState, tamamlanmisFormlar);
        }


        public override void OzelFormKaydet(WorkFlowFormViewModel formData)
        {
            base.OzelFormKaydet(formData, tamamlanmisFormlar);
        }

        public override void WorkFlowFormKaydet<TClass, TVM>(WorkFlowFormViewModel workFlowFormViewModel)
        {
            base.WorkFlowFormKaydet<TClass, TVM>(workFlowFormViewModel);
            WorkFlowTrace torSatinAlmaIslem = Mapper.Map<WorkFlowTraceForm, WorkFlowTrace>(workFlowFormViewModel.WorkFlowIslemForm);
            AddOrUpdate(torSatinAlmaIslem);
        }

        public override void WorkFlowProcessIptal(int workFlowTraceId)
        {
            base.WorkFlowProcessIptal(workFlowTraceId);
        }

        public override void WorkFlowWorkFlowTraceiGeriAl(int workFlowTraceId, int targetProcessId)
        {
            base.WorkFlowWorkFlowTraceiGeriAl(workFlowTraceId, targetProcessId);
        }

        public override void WorkFlowWorkFlowNextProcess(int ownerId)
        {
            base.WorkFlowWorkFlowNextProcess(ownerId);
        }

        public string KararNoktasiSurecKontrolJobCall(string id, string jobId, string hourInterval)
        {
            base.KararNoktasiSurecKontrolJobCallBase(id, jobId, hourInterval);

            RecurringJob.AddOrUpdate<TestWorkFlowProcessService>(jobId, x => x.KararNoktasiSurecKontrol(int.Parse(id)), Cron.HourInterval(int.Parse(hourInterval)));
            return "OK";
        }

        public override bool FullFormValidate(WorkFlowFormViewModel formData, ModelStateDictionary modelState)
        {
            return base.FullFormValidate(formData, modelState, tamamlanmisFormlar);
        }

        UserProcessViewModel IWorkFlow.KullaniciSonIslemVM(int ownerId)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
