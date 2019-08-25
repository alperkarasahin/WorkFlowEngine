using AutoMapper;
using Hangfire;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using WorkFlowManager.Common.Constants;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Dto;
using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.Validation;
using WorkFlowManager.Common.ViewModels;
using WorkFlowManager.Helper;

namespace WorkFlowManager.Services.DbServices
{
    public class WorkFlowProcessService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly WorkFlowDataService _workFlowDataService;
        public Dictionary<string, IWorkFlowForm> workFlowFormList = new Dictionary<string, IWorkFlowForm>();

        //public void FormSave(WorkFlowFormViewModel formData);

        public WorkFlowProcessService(IUnitOfWork unitOfWork, WorkFlowDataService workFlowDataService)
        {
            _unitOfWork = unitOfWork;
            _workFlowDataService = workFlowDataService;
        }

        protected int GetOwnerIdFromId(int id)
        {
            var workFlowTrace = _unitOfWork.Repository<WorkFlowTrace>().Get(x => x.Id == id);
            int rslt = -1;
            if (workFlowTrace != null)
            {
                rslt = workFlowTrace.OwnerId;
            }
            return rslt;
        }

        public int GetLastProcessId(int ownerId)
        {
            var workFlowTraceList = _unitOfWork.Repository<WorkFlowTrace>().Find(x => x.OwnerId == ownerId).OrderByDescending(x => x.Id);


            if (workFlowTraceList.Count() > 0)
            {
                return workFlowTraceList.First().Id;
            }

            return 0;

        }

        public int StartWorkFlow(int ownerId, Task task)
        {
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

        public int StartWorkFlow(int ownerId, string taskName)
        {
            var task = _unitOfWork.Repository<Task>().GetAll().Where(x => x.Name == taskName).FirstOrDefault();
            return StartWorkFlow(ownerId, task);
        }

        public int StartWorkFlow(int ownerId, int taskId)
        {
            var task = _unitOfWork.Repository<Task>().Get(taskId);
            return StartWorkFlow(ownerId, task);
        }


        public virtual void AddOrUpdate(WorkFlowTrace workFlowTrace)
        {
            WorkFlowTrace workFlowTraceDB = null;
            if (workFlowTrace.Id == 0)
            {
                workFlowTraceDB = Mapper.Map<WorkFlowTrace, WorkFlowTrace>(workFlowTrace);
                _unitOfWork.Repository<WorkFlowTrace>().Add(workFlowTraceDB);
            }
            else
            {
                workFlowTraceDB = _unitOfWork.Repository<WorkFlowTrace>().Get(workFlowTrace.Id);
                Mapper.Map(workFlowTrace, workFlowTraceDB);
                _unitOfWork.Repository<WorkFlowTrace>().Update(workFlowTraceDB);
            }
            _unitOfWork.Complete();
            workFlowTrace.Id = workFlowTraceDB.Id;
        }

        public string DecisionPointJobCallBase(string id, string jobId, string hourInterval)
        {
            WorkFlowTrace torSatinAlmaWorkFlowTrace = _unitOfWork.Repository<WorkFlowTrace>().Get(int.Parse(id));

            torSatinAlmaWorkFlowTrace.JobId = jobId;
            _unitOfWork.Repository<WorkFlowTrace>().Update(torSatinAlmaWorkFlowTrace);
            _unitOfWork.Complete();
            return "OK";
        }

        public string GetWorkFlow(string gorevAkis, int WorkFlowTraceId)
        {
            var workFlowTraceListesi =
                _workFlowDataService
                    .GetWorkFlowTraceList();

            UserProcessViewModel kullaniciIslem =
                workFlowTraceListesi
                    .Single(x => x.Id == WorkFlowTraceId);

            IEnumerable<UserProcessViewModel> workFlowTraceListesiForOwner =
                workFlowTraceListesi
                    .Where(x => x.OwnerId == kullaniciIslem.OwnerId);

            Dictionary<string, int> workFlowTraceTekrarSayisi = new Dictionary<string, int>();
            Dictionary<string, string> workFlowTraceDurumClassi = new Dictionary<string, string>();
            foreach (var workFlowTrace in workFlowTraceListesiForOwner)
            {
                int tekrarSayisi;
                string durumClassi = "";
                string dummy = "";
                if (workFlowTraceTekrarSayisi.TryGetValue(workFlowTrace.ProcessUniqueCode, out tekrarSayisi))
                {
                    tekrarSayisi++;
                    workFlowTraceTekrarSayisi[workFlowTrace.ProcessUniqueCode] = tekrarSayisi;
                }
                else
                {
                    tekrarSayisi++;
                    workFlowTraceTekrarSayisi.Add(workFlowTrace.ProcessUniqueCode, tekrarSayisi);
                }


                if (workFlowTrace.ProcessStatus == Common.Enums.ProcessStatus.Draft)
                {
                    durumClassi = "current";
                }
                else if (workFlowTrace.ProcessStatus == Common.Enums.ProcessStatus.Cancelled)
                {
                    durumClassi = "cancelled";
                }
                else //Tamamlanmışsa ikinci kez tekrar edilip edilmediği kontrol edilecek
                {
                    if (tekrarSayisi > 1)
                    {
                        durumClassi = "revized";
                    }
                    else
                    {
                        durumClassi = "completed";
                    }
                }
                if (workFlowTraceDurumClassi.TryGetValue(workFlowTrace.ProcessUniqueCode, out dummy))
                {
                    workFlowTraceDurumClassi[workFlowTrace.ProcessUniqueCode] = durumClassi;
                }
                else
                {
                    workFlowTraceDurumClassi.Add(workFlowTrace.ProcessUniqueCode, durumClassi);
                }
            }

            StringBuilder statusString = new StringBuilder(
            @"classDef completed fill:#9f6,stroke:#333,stroke-width:2px;" +
            "classDef current fill: yellow,stroke:#333,stroke-width:2px;" +
            "classDef cancelled fill: #e55c59,stroke:#333,stroke-width:2px;" +
            "classDef revized fill:#196,stroke:#333,stroke-width:2px;");
            statusString.Append("\\n");
            foreach (var WorkFlowTraceDurum in workFlowTraceDurumClassi)
            {
                statusString.Append(string.Format("class {0} {1};\\n", WorkFlowTraceDurum.Key, WorkFlowTraceDurum.Value));
            }
            return string.Format("{0}\\n{1}", gorevAkis, statusString.ToString());
        }

        public IEnumerable<UserProcessViewModel> WorkFlowTraceList(int ownerId)
        {
            return
                _workFlowDataService
                    .GetWorkFlowTraceList()
                        .Where(x => x.OwnerId == ownerId);
        }

        public List<WorkFlowTraceVM> ProgressProcessList(int workFlowTraceId)
        {
            UserProcessViewModel kullaniciWorkFlowTrace =
            _workFlowDataService
                .GetWorkFlowTraceList()
                    .Single(x => x.Id == workFlowTraceId);

            ProcessVM sonrakiGorev = null;


            var processList = _workFlowDataService.GetWorkFlowProcessList(kullaniciWorkFlowTrace.TaskId);

            if (kullaniciWorkFlowTrace.NextProcessId != null)
            {
                sonrakiGorev =
                    processList
                        .Where(x => x.Id == (int)kullaniciWorkFlowTrace.NextProcessId)
                            .FirstOrDefault();
            }


            var workFlowTraceListesi = WorkFlowTraceList(kullaniciWorkFlowTrace.OwnerId);

            List<WorkFlowTraceVM> tumWorkFlowTraceler = workFlowTraceListesi
                .OrderBy(x => x.LastlyModifiedTime)
                .Select(x => new WorkFlowTraceVM
                {
                    Description = x.ProcessDescription,
                    ProcessName = x.ProcessName,
                    ProcessId = x.ProcessId,
                    ProcessStatus = x.ProcessStatus,
                    ConditionOptionId = x.ConditionOptionId,
                    OwnerId = x.OwnerId,
                    AssignedRole = x.AssignedRole,
                    Id = x.Id
                })
                .ToList();
            List<WorkFlowTraceVM> progressGosterilecekWorkFlowTraceler = new List<WorkFlowTraceVM>();

            if (tumWorkFlowTraceler.Count() > 1)
            {
                for (int i = 0; i < tumWorkFlowTraceler.Count(); i++)
                {

                    if ((i + 1) < tumWorkFlowTraceler.Count() && tumWorkFlowTraceler[i + 1].Id == workFlowTraceId)
                    {
                        progressGosterilecekWorkFlowTraceler.Add(tumWorkFlowTraceler[i]);
                        progressGosterilecekWorkFlowTraceler.Add(tumWorkFlowTraceler[i + 1]);
                        break;
                    }
                }
            }
            else
            {
                progressGosterilecekWorkFlowTraceler.Add(tumWorkFlowTraceler[0]);
            }


            if (sonrakiGorev != null)
            {
                progressGosterilecekWorkFlowTraceler.Add(new WorkFlowTraceVM
                {
                    ProcessStatus = null,
                    AssignedRole = sonrakiGorev.AssignedRole,
                    Description = sonrakiGorev.Description,
                    ProcessName = sonrakiGorev.Name
                });
            }

            return progressGosterilecekWorkFlowTraceler;

        }

        public List<UserProcessViewModel> TargetProcessListForCancel(int WorkFlowTraceId)
        {
            var birimdekiTumWorkFlowTraceler = _workFlowDataService.GetWorkFlowTraceList();

            UserProcessViewModel kullaniciWorkFlowTrace =
                    birimdekiTumWorkFlowTraceler
                        .Where(x => x.Id == WorkFlowTraceId)
                            .FirstOrDefault();

            var tumIslemler =
                birimdekiTumWorkFlowTraceler
                    .Where(x => x.OwnerId == kullaniciWorkFlowTrace.OwnerId && x.ProcessStatus == Common.Enums.ProcessStatus.Completed && x.Id < WorkFlowTraceId)
                        .OrderByDescending(x => x.Id);

            List<UserProcessViewModel> oncekiWorkFlowTraceListesi = new List<UserProcessViewModel>();
            var taskId = kullaniciWorkFlowTrace.TaskId;//WorkFlowTrace.GorevWorkFlowTrace.GorevId;
            var WorkFlowTraceiYapanRol = kullaniciWorkFlowTrace.AssignedRole;
            var gorevWorkFlowTraceId = kullaniciWorkFlowTrace.ProcessId;
            var gorevWorkFlowTraceListesi = _workFlowDataService.GetWorkFlowProcessList(taskId);

            foreach (var oncekiIslem in tumIslemler)
            {
                ProcessVM gorevWorkFlowTrace = gorevWorkFlowTraceListesi.SingleOrDefault(x => x.Id == oncekiIslem.ProcessId);

                if (gorevWorkFlowTrace.AssignedRole == ProjectRole.System)
                {
                    continue;
                }
                if (gorevWorkFlowTrace.AssignedRole != WorkFlowTraceiYapanRol)
                {
                    break;
                }

                if (gorevWorkFlowTrace.IsCondition)
                {
                    gorevWorkFlowTrace = gorevWorkFlowTraceListesi.SingleOrDefault(x => x.Id == oncekiIslem.ConditionOptionId);
                }

                if (oncekiIslem.ProcessId != gorevWorkFlowTraceId && !oncekiWorkFlowTraceListesi.Any(x => oncekiIslem.ProcessId == x.ProcessId))
                {
                    List<int> elementOfTree = new List<int>();
                    if (!SearchProcessInsideNextPath(elementOfTree, gorevWorkFlowTraceListesi, gorevWorkFlowTraceId, oncekiIslem.ProcessId))
                    {
                        oncekiWorkFlowTraceListesi.Add(oncekiIslem);
                    }
                }
            }
            return oncekiWorkFlowTraceListesi.OrderBy(x => x.Id).ToList();
        }

        public bool SearchProcessInsideNextPath(List<int> elementOfTree, IEnumerable<ProcessVM> gorevWorkFlowTraceListesi, int gorevWorkFlowTraceId, int kontrolEdilecekGorevWorkFlowTraceId)
        {
            if (elementOfTree.Any(x => x == gorevWorkFlowTraceId))
            {
                return false;
            }

            var gorevWorkFlowTrace = gorevWorkFlowTraceListesi.SingleOrDefault(x => x.Id == gorevWorkFlowTraceId);
            var sonuc = false;
            elementOfTree.Add(gorevWorkFlowTraceId);

            if (gorevWorkFlowTrace.IsCondition)
            {
                //Self referansları almayacak şekilde seçenekler listeye alınıyor
                var secenekListesi = gorevWorkFlowTraceListesi.Where(x => x.ConditionId == gorevWorkFlowTrace.Id && x.NextProcessId != gorevWorkFlowTrace.Id);
                foreach (var secenek in secenekListesi)
                {
                    if (SearchProcessInsideNextPath(elementOfTree, gorevWorkFlowTraceListesi, secenek.Id, kontrolEdilecekGorevWorkFlowTraceId))
                    {
                        sonuc = true;
                        break;
                    }
                }
            }
            else
            {
                if (gorevWorkFlowTrace.NextProcessId != null)
                {
                    if (gorevWorkFlowTrace.NextProcessId == kontrolEdilecekGorevWorkFlowTraceId)
                    {
                        sonuc = true;
                    }
                    else
                    {
                        sonuc = SearchProcessInsideNextPath(elementOfTree, gorevWorkFlowTraceListesi, (int)gorevWorkFlowTrace.NextProcessId, kontrolEdilecekGorevWorkFlowTraceId);
                    }
                }
            }
            return sonuc;
        }

        public virtual WorkFlowTrace WorkFlowTraceDetail(int workFlowTraceId)
        {
            WorkFlowTrace WorkFlowTrace = _unitOfWork.Repository<WorkFlowTrace>()
                .Get(
                        x => x.Id == workFlowTraceId,
                        x => x.Process,
                            x => x.Process.DocumentList,
                                x => x.Process.FormView,
                                    x => x.DocumentList,
                                        x => x.Process.Task
                    );

            if (WorkFlowTrace.Process.GetType() == typeof(Condition))
            {
                ((Condition)WorkFlowTrace.Process).OptionList = _unitOfWork.Repository<ConditionOption>().GetList(x => x.ConditionId == WorkFlowTrace.ProcessId).ToList();
            }

            if (WorkFlowTrace.Process.GetType() == typeof(SubProcess))
            {
                WorkFlowTrace.SubProcessList = _unitOfWork.Repository<BusinessProcess>().GetList(x => x.OwnerSubProcessTraceId == workFlowTraceId).ToList();
            }


            return WorkFlowTrace;
        }

        public virtual void WorkFlowFormSave<TClass, TVM>(WorkFlowFormViewModel workFlowFormViewModel)
        where TClass : class
        where TVM : WorkFlowFormViewModel
        {
            TClass form = _unitOfWork.Repository<TClass>().Get(workFlowFormViewModel.OwnerId);


            if (form != null)
            {
                Mapper.Map((TVM)workFlowFormViewModel, form);
                _unitOfWork.Repository<TClass>().Update(form);
            }
            else
            {
                form = (TClass)Activator.CreateInstance(typeof(TClass));
                Mapper.Map((TVM)workFlowFormViewModel, form);
                _unitOfWork.Repository<TClass>().Add(form);
            }
            _unitOfWork.Complete();
        }


        public WorkFlowFormViewModel WorkFlowFormLoad(WorkFlowFormViewModel workFlowFormViewModel)
        {
            WorkFlowFormViewModel workFlowForm = workFlowFormViewModel;

            if (workFlowFormViewModel.ProcessFormViewViewName != null && workFlowFormViewModel.ProcessFormViewCompleted)
            {
                IWorkFlowForm form = GetWorkFlowForm(workFlowFormViewModel.ProcessFormViewViewName);
                if (form != null)
                {
                    workFlowForm = form.Load(workFlowFormViewModel);
                }
            }
            return workFlowForm;
        }

        private IWorkFlowForm GetWorkFlowForm(string formViewName)
        {
            IWorkFlowForm form = null;
            if (formViewName != null && !workFlowFormList.TryGetValue(formViewName, out form))
            {
                Type type = Type.GetType("WorkFlowManager.Services.CustomForms." + formViewName + ", WorkFlowManager.Services");

                var formObject = DependencyResolver.Current.GetService(type);
                if (formObject != null)
                {
                    workFlowFormList.Add(formViewName, (IWorkFlowForm)formObject);

                    form = (IWorkFlowForm)formObject;
                }
            }
            return form;
        }

        public void CustomFormSave(WorkFlowFormViewModel formData)
        {
            if (formData.ProcessFormViewCompleted && formData.ProcessFormViewViewName != null)
            {
                IWorkFlowForm form = GetWorkFlowForm(formData.ProcessFormViewViewName);
                if (form != null)
                {
                    form.Save(formData);
                }
            }
        }


        public bool CustomFormValidate(WorkFlowFormViewModel formData, ModelStateDictionary modelState)
        {
            if (formData.ProcessFormViewCompleted && formData.ProcessFormViewViewName != null)
            {
                IWorkFlowForm form = GetWorkFlowForm(formData.ProcessFormViewViewName);
                if (form != null)
                {
                    return form.Validate(formData, modelState);
                }
            }
            return true;
        }


        public UserProcessViewModel GetUserProcessVM(int workFlowTraceId)
        {
            return _workFlowDataService.GetWorkFlowTraceList().SingleOrDefault(x => x.Id == workFlowTraceId);
        }

        public virtual void WorkFlowProcessCancel(int workFlowTraceId)
        {
            WorkFlowTrace workFlowTrace = WorkFlowTraceDetail(workFlowTraceId);

            if (workFlowTrace.Process.GetType() == typeof(DecisionPoint))
            {
                DecisionPoint decisionPoint = (DecisionPoint)workFlowTrace.Process;

                if (decisionPoint.CancelProcessId != null)
                {
                    int cancelProcessId = (int)decisionPoint.CancelProcessId;

                    if (workFlowTrace.JobId != null)
                    {
                        RecurringJob.RemoveIfExists(workFlowTrace.JobId);
                    }
                    _unitOfWork.Repository<WorkFlowTrace>().Remove(workFlowTraceId);
                    CreateWorkFlowTrace(cancelProcessId, workFlowTrace.OwnerId);
                }
            }
        }


        public virtual void CancelWorkFlowTrace(int workFlowTraceId, int targetProcessId)
        {
            WorkFlowTrace WorkFlowTrace = _unitOfWork.Repository<WorkFlowTrace>().Get(workFlowTraceId);

            WorkFlowTrace.ProcessStatus = ProcessStatus.Cancelled;

            AddOrUpdate(WorkFlowTrace);

            CreateWorkFlowTrace(targetProcessId, WorkFlowTrace.OwnerId);
        }


        public string GetVariable(string key, int ownerId)
        {
            if (key != null)
            {
                var workFlowEngineVariable = _unitOfWork.Repository<WorkFlowEngineVariable>().Get(x => x.OwnerId == ownerId && x.Key == key);

                if (workFlowEngineVariable != null)
                {
                    return workFlowEngineVariable.Value;
                }
                else
                {
                    //If variable not found inside main business process variable list
                    //We fill search it inside sub business process list
                    var subProcessList = _unitOfWork.Repository<BusinessProcess>().GetAll().Where(x => x.OwnerId == ownerId).ToList();
                    foreach (var subProcess in subProcessList)
                    {
                        var rslt = GetVariable(key, subProcess.Id);
                        if (rslt != null)
                        {
                            return rslt;
                        }
                    }
                }
            }
            return null;
        }
        public void SetVariable(string key, string value, int ownerId)
        {
            if (key != null)
            {
                var workFlowEngineVariable = _unitOfWork.Repository<WorkFlowEngineVariable>().Get(x => x.OwnerId == ownerId && x.Key == key);

                if (workFlowEngineVariable != null)
                {
                    workFlowEngineVariable.Value = value;
                    _unitOfWork.Repository<WorkFlowEngineVariable>().Update(workFlowEngineVariable);
                }
                else
                {
                    _unitOfWork.Repository<WorkFlowEngineVariable>().Add(new WorkFlowEngineVariable { OwnerId = ownerId, Key = key, Value = value });
                }
                _unitOfWork.Complete();
            }
        }

        public virtual void GoToWorkFlowNextProcess(int ownerId)
        {
            var WorkFlowTraceList = this.WorkFlowTraceList(ownerId);

            UserProcessViewModel userProcessVMCurrent = WorkFlowTraceList.OrderByDescending(x => x.Id).First();
            int currentUserProcessVMProcessId = userProcessVMCurrent.ProcessId;
            if (userProcessVMCurrent.ConditionOptionId != null)
            {
                currentUserProcessVMProcessId = (int)userProcessVMCurrent.ConditionOptionId;
            }

            WorkFlowTrace workFlowTraceCurrentDB = _unitOfWork.Repository<WorkFlowTrace>().Get(userProcessVMCurrent.Id);
            workFlowTraceCurrentDB.ProcessStatus = Common.Enums.ProcessStatus.Completed;

            AddOrUpdate(workFlowTraceCurrentDB);

            Process currentProcess = _unitOfWork.Repository<Process>().Get(x => x.Id == currentUserProcessVMProcessId, x => x.NextProcess);
            if (currentProcess.GetType() == typeof(ConditionOption))
            {
                var key = userProcessVMCurrent.ProcessVariableName;
                if (key != null)
                {
                    var value = ((ConditionOption)currentProcess).Value;
                    SetVariable(key, value, ownerId);
                }
            }

            if (currentProcess.NextProcessId != null)
            {
                CreateNewWorkFlowTrace(currentProcess.NextProcess, ownerId);
            }
            else
            {
                //If process is last node of the work flow
                //And business process is a sub flow
                //We will check all sub flows (except current) 
                var businessProcess = _unitOfWork.Repository<BusinessProcess>().Get(ownerId);
                if (businessProcess.OwnerSubProcessTraceId != null)
                {
                    var allSubFlowsExceptCurrent = _unitOfWork.Repository<BusinessProcess>().GetAll().Where(x => x.OwnerSubProcessTraceId == businessProcess.OwnerSubProcessTraceId && x.Id != ownerId);
                    bool allSubProcessCompleted = true;
                    foreach (var subFlow in allSubFlowsExceptCurrent)
                    {

                        var draftProcess = _unitOfWork.Repository<WorkFlowTrace>().GetAll().FirstOrDefault(x => x.OwnerId == subFlow.Id && x.ProcessStatus == ProcessStatus.Draft);
                        allSubProcessCompleted = draftProcess == null;
                        if (!allSubProcessCompleted)
                        {
                            break;
                        }

                    }


                    if (allSubProcessCompleted)
                    {
                        GoToWorkFlowNextProcess((int)businessProcess.OwnerId);
                    }


                }
            }
        }

        private void CreateWorkFlowTrace(int targetProcessId, int ownerId)
        {
            var targetProcess = _unitOfWork.Repository<Process>().Get(targetProcessId);
            CreateNewWorkFlowTrace(targetProcess, ownerId);
        }


        private void CreateNewWorkFlowTrace(Process targetProcess, int ownerId)
        {
            WorkFlowTrace workFlowTrace = new WorkFlowTrace()
            {
                ProcessId = targetProcess.Id,
                OwnerId = ownerId,
                ProcessStatus = ProcessStatus.Draft
            };
            AddOrUpdate(workFlowTrace);

            if (targetProcess is DecisionPoint)
            {
                DecisionPointTakeTheNextStep(workFlowTrace.Id);
            }
            else if (targetProcess is SubProcess)
            {
                //Sub process will be start
                var taskVariableList = JsonConvert.DeserializeObject<List<TaskVariable>>(((SubProcess)targetProcess).TaskVariableList);
                foreach (var taskVariable in taskVariableList)
                {
                    var numberOfSubProcessCount = 0;
                    var numberOfSubProcessCountString = GetVariable(taskVariable.VariableName, ownerId);
                    if (numberOfSubProcessCountString == null)
                    {
                        numberOfSubProcessCount = 1;
                    }
                    else
                    {
                        numberOfSubProcessCount = int.Parse(numberOfSubProcessCountString);
                    }

                    var task = _unitOfWork.Repository<Task>().Get(taskVariable.TaskId);

                    var subBusinessProcess = new BusinessProcess() { OwnerId = ownerId, Name = task.Name, OwnerSubProcessTraceId = workFlowTrace.Id };
                    _unitOfWork.Repository<BusinessProcess>().Add(subBusinessProcess);
                    _unitOfWork.Complete();

                    StartWorkFlow(subBusinessProcess.Id, task);
                }
            }
        }


        public void DecisionPointTakeTheNextStep(int workFlowTraceId)
        {
            WorkFlowTrace workFlowTraceDecisionPoint = _unitOfWork.Repository<WorkFlowTrace>().Get(workFlowTraceId);

            DecisionPoint decisionPoint = _unitOfWork.Repository<DecisionPoint>().Get(x => x.Id == workFlowTraceDecisionPoint.ProcessId, x => x.DecisionMethod, x => x.Task);

            if (!string.IsNullOrEmpty(decisionPoint.DecisionMethod.MethodFunction))
            {
                string serviceName = decisionPoint.Task.MethodServiceName;
                string methodName = string.Format("{0}.{1}", serviceName, decisionPoint.DecisionMethod.MethodFunction.Substring(0, decisionPoint.DecisionMethod.MethodFunction.LastIndexOf("(")));

                var parameters = decisionPoint.DecisionMethod.MethodFunction.Substring(
                                        decisionPoint.DecisionMethod.MethodFunction.LastIndexOf("(") + 1,
                                            (decisionPoint.DecisionMethod.MethodFunction.LastIndexOf(")") - decisionPoint.DecisionMethod.MethodFunction.LastIndexOf("(") - 1))
                                                .Split(',')
                                                    .Select(p => p.Trim())
                                                        .ToList();


                List<object> parametersValue = new List<object>();

                foreach (var parameter in parameters)
                {
                    if (!string.IsNullOrWhiteSpace(parameter))
                    {
                        var property = workFlowTraceDecisionPoint.GetType().GetProperties().Where(x => x.Name == parameter).FirstOrDefault();
                        if (property != null)
                        {
                            var value = property.GetValue(workFlowTraceDecisionPoint, null);
                            parametersValue.Add(value);
                        }
                        else
                        {
                            parametersValue.Add(parameter);
                        }
                    }
                }

                var parametersArray = parametersValue.ToArray();

                var methodCallString = string.Format("{0}({1})", methodName, string.Join(",", parametersArray));

                var decisionMethodResult =
                    DynamicMethodCallService.Caller(
                        _unitOfWork,
                                    methodCallString,
                                        _workFlowDataService);

                Process conditionOption = _unitOfWork.Repository<ConditionOption>().Get(x => x.ConditionId == decisionPoint.Id && x.Value == decisionMethodResult);
                if (conditionOption.NextProcessId == decisionPoint.Id)
                {
                    if (workFlowTraceDecisionPoint.JobId == null)
                    {
                        List<object> decisionPointJobCallParameterList = new List<object>();

                        string jobId = Guid.NewGuid().ToString();
                        decisionPointJobCallParameterList.Add(workFlowTraceId);
                        decisionPointJobCallParameterList.Add(jobId);
                        decisionPointJobCallParameterList.Add(decisionPoint.RepetitionFrequenceByHour);

                        var methodJobCallString = string.Format("{0}.DecisionPointJobCall({1})", serviceName, string.Join(",", decisionPointJobCallParameterList.ToArray()));

                        DynamicMethodCallService.Caller(
                            _unitOfWork,
                                    methodJobCallString,
                                        _workFlowDataService);
                    }
                }
                else
                {
                    workFlowTraceDecisionPoint.ConditionOptionId = conditionOption.Id;
                    AddOrUpdate(workFlowTraceDecisionPoint);
                    if (workFlowTraceDecisionPoint.JobId != null)
                    {
                        RecurringJob.RemoveIfExists(workFlowTraceDecisionPoint.JobId);
                    }
                    GoToWorkFlowNextProcess(workFlowTraceDecisionPoint.OwnerId);
                }
            }
        }

        public bool StandartFormValidate(WorkFlowFormViewModel formData, ModelStateDictionary modelState)
        {
            return ValidationHelper.Validate(formData, new WorkFlowFormViewModelValidator(_unitOfWork), modelState);
        }

        public bool FullFormValidate(WorkFlowFormViewModel formData, ModelStateDictionary modelState)
        {
            bool resultStandartForm = StandartFormValidate(formData, modelState);
            bool resultCustomForm = CustomFormValidate(formData, modelState);
            return resultCustomForm && resultStandartForm;
        }

        public object SetNextProcessForWorkFlow(int WorkFlowTraceId)
        {
            WorkFlowTrace WorkFlowTrace = WorkFlowTraceDetail(WorkFlowTraceId);
            int workFlowTraceProcessId = WorkFlowTrace.ProcessId;

            Process process = _unitOfWork.Repository<Process>().Get(workFlowTraceProcessId);

            var workFlowProcessList = WorkFlowTraceList(WorkFlowTrace.OwnerId);
            UserProcessViewModel lastWorkFlowTrace = workFlowProcessList.OrderByDescending(x => x.Id).First();
            int lastWorkFlowTraceProcessId = lastWorkFlowTrace.ProcessId;

            bool nextProcessIsSuitable = false;
            if (process.Id != lastWorkFlowTraceProcessId)
            {
                nextProcessIsSuitable = true;
            }

            object routeObject = null;
            if (nextProcessIsSuitable)
            {
                routeObject = new { workFlowTraceId = lastWorkFlowTrace.Id };
            }
            else
            {
                routeObject = new { controller = "Home" };
            }
            return routeObject;
        }

        public bool WorkFlowPermissionCheck(UserProcessViewModel userProcessVM)
        {
            bool rslt = true;
            if (
                !(
                        (
                            userProcessVM.AssignedRole != ProjectRole.System
                            &&
                            true //TODO
                        )
                        ||
                        (
                            userProcessVM.AssignedRole == ProjectRole.System
                            &&
                            true //TODO
                        )
                )
               )

            {
                var errorMessage = "Access denied!";
                NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(errorMessage);

                rslt = false;
            }

            return rslt;
        }

        public WorkFlowDTO WorkFlowBaseInfo(UserProcessViewModel kullaniciWorkFlowTraceVM)
        {
            return
                new WorkFlowDTO
                {
                    TargetProcessListForCancel = kullaniciWorkFlowTraceVM.ProcessStatus != ProcessStatus.Completed ? TargetProcessListForCancel(kullaniciWorkFlowTraceVM.Id) : new List<UserProcessViewModel>(),
                    AuthorizedProcessList = WorkFlowTraceList(kullaniciWorkFlowTraceVM.OwnerId).Where(x => x.ProcessStatus != ProcessStatus.Draft),
                    ProgressProcessList = ProgressProcessList(kullaniciWorkFlowTraceVM.Id),
                };
        }

        public void SetWorkFlowTraceForm(WorkFlowFormViewModel workFlowFormVM, WorkFlowDTO workFlowBase)
        {
            workFlowFormVM.ProgressProcessList = workFlowBase.ProgressProcessList;
            workFlowFormVM.TargetProcessListForCancel = workFlowBase.TargetProcessListForCancel;
            workFlowFormVM.AuthorizedProcessList = workFlowBase.AuthorizedProcessList;

            if (workFlowFormVM.IsCondition)
            {
                workFlowFormVM.ListOfOptions =
                    _workFlowDataService
                        .GetWorkFlowProcessList(workFlowFormVM.ProcessTaskId)
                            .Where(x => x.ConditionId == workFlowFormVM.ProcessId)
                                .ToList();
            }
        }

    }
}
