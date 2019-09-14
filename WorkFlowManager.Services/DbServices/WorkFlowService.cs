using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using WorkFlowManager.Common.Constants;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Factory;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Services.DbServices
{
    public class WorkFlowService
    {
        private readonly IUnitOfWork _unitOfWork;
        public WorkFlowService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<WorkFlow> GetWorkFlowList()
        {
            return _unitOfWork.Repository<WorkFlow>()
                .GetList();
        }

        public IEnumerable<Task> GetTaskList()
        {
            return _unitOfWork.Repository<Task>()
                .GetList(null, x => x.WorkFlow);
        }

        public IEnumerable<Process> GetProcessList(int gorevId)
        {
            return _unitOfWork.Repository<Process>()
                .Find(x => x.TaskId == gorevId);
        }


        public string GetWorkFlowDiagram(int gorevId)
        {
            var gorev = _unitOfWork.Repository<Task>().Get(gorevId);
            return gorev.WorkFlowDiagram;
        }

        public void SetWorkFlowDiagram(int taskId)
        {
            WorkFlowUtil.SetWorkFlowDiagram(_unitOfWork, taskId);
        }

        public string Delete(int processId)
        {
            Process process = _unitOfWork.Repository<Process>().GetForUpdate(x => x.Id == processId, x => x.MonitoringRoleList, x => x.DocumentList);
            int taskId = process.TaskId;

            if (process.GetType() == typeof(Condition))
            {
                var optionList = _unitOfWork.Repository<ConditionOption>().GetList(x => x.ConditionId == processId);
                if (optionList.Count() > 0)
                {
                    throw new Exception("Options must be removed first.");
                }
            }
            else if (process.GetType() == typeof(DecisionPoint)) //Karar Noktasi ise önce seçenekleri silinecek
            {
                var optionList = _unitOfWork.Repository<ConditionOption>().GetListForUpdate(x => x.ConditionId == processId).ToList();

                var secenekIdListesi = optionList.Select(t => t.Id).ToList();
                var monitoringRolList = _unitOfWork.Repository<ProcessMonitoringRole>().GetListForUpdate(x => secenekIdListesi.Any(t => t == x.ProcessId)).ToList();
                _unitOfWork.Repository<ProcessMonitoringRole>().RemoveRange(monitoringRolList);
                _unitOfWork.Repository<ConditionOption>().RemoveRange(optionList);
                _unitOfWork.Complete();
            }

            if (process.DocumentList != null && process.DocumentList.Count() > 0)
            {
                _unitOfWork.Repository<Document>().RemoveRange(process.DocumentList);
            }

            var processList = _unitOfWork.Repository<Process>().GetList(null, x => x.MonitoringRoleList);
            Process before = GetBefore(processId);

            string result = null;
            Process after = null;
            if (before != null)
            {
                if (process.NextProcessId != null)
                {
                    after = _unitOfWork.Repository<Process>().Get((int)process.NextProcessId);
                }
            }

            if (before != null && after != null)
            {
                before.NextProcessId = after.Id;
                result = string.Format("{0}->{1} link created.", before.Name, after.Name);
            }

            var deletedTaskName = process.Name;


            if (process.MonitoringRoleList != null && process.MonitoringRoleList.Count() > 0)
            {
                _unitOfWork.Repository<ProcessMonitoringRole>().RemoveRange(process.MonitoringRoleList);
            }

            Task task = _unitOfWork.Repository<Task>().Get(process.TaskId);
            if (task.StartingProcessId == process.Id)
            {
                task.StartingProcessId = null;
                _unitOfWork.Repository<Task>().Update(task);
                _unitOfWork.Complete();
            }

            _unitOfWork.Repository<Process>().Remove(process);
            if (before != null)
            {
                _unitOfWork.Repository<Process>().Update(before);
            }

            _unitOfWork.Complete();

            SetStartingProcess(taskId);
            SetWorkFlowDiagram(taskId);

            result = string.Format("{0} removed. {1}", deletedTaskName, result);
            return result;

        }
        public int AddOrUpdate<T>(T process) where T : Process
        {
            T processRecorded = null;

            int? processRecordedNextProcessId = null;


            if (process.Id == 0)
            {
                processRecorded = process;
                processRecorded.ProcessUniqueCode = Guid.NewGuid().ToString();
                _unitOfWork.Repository<T>().Add(processRecorded);
                if (processRecorded.GetType() == typeof(DecisionPoint))
                {
                    var optionYes = ProcessFactory.CreateDecisionPointYesOption("Yes", (DecisionPoint)(object)processRecorded);
                    var optionNo = ProcessFactory.CreateDecisionPointNoOption("No", (DecisionPoint)(object)processRecorded);

                    _unitOfWork.Repository<ConditionOption>().Add(optionYes);
                    _unitOfWork.Repository<ConditionOption>().Add(optionNo);
                }
            }
            else
            {
                processRecorded = _unitOfWork.Repository<T>().GetForUpdate(x => x.Id == process.Id, x => x.MonitoringRoleList, x => x.DocumentList);
                processRecordedNextProcessId = processRecorded.NextProcessId;

                if (processRecorded.MonitoringRoleList != null)
                {
                    var deletedRoleList = processRecorded.MonitoringRoleList.Where(x => !process.MonitoringRoleList.Any(t => t.ProjectRole == x.ProjectRole)).ToList();
                    foreach (var role in deletedRoleList)
                    {
                        _unitOfWork.Repository<ProcessMonitoringRole>().Remove(role);
                    }
                }

                if (processRecorded.DocumentList != null)
                {
                    var deletedDocumentList = processRecorded.DocumentList.Where(x => !process.DocumentList.Any(t => t.MediaName == x.MediaName)).ToList();
                    foreach (var document in deletedDocumentList)
                    {
                        _unitOfWork.Repository<Document>().Remove(document);
                    }
                }



                Mapper.Map(process, processRecorded);
                _unitOfWork.Repository<T>().Update(processRecorded);
            }
            _unitOfWork.Complete();

            if (processRecordedNextProcessId != null && processRecorded.NextProcessId != null && processRecordedNextProcessId != processRecorded.NextProcessId)
            {
                SetNextByProcessCode(processRecorded.ProcessUniqueCode, processRecorded.NextProcessId);
            }
            return processRecorded.Id;
        }



        private Process GetBefore(int processId)
        {
            foreach (var islem in _unitOfWork.Repository<Process>().GetAll())
            {
                if (islem.NextProcessId == processId)
                {
                    return islem;
                }
            }
            return null;
        }


        public void SetNextByProcessCode(string processCode, int? nextProcessId)
        {
            var process = _unitOfWork.Repository<Process>().Get(x => x.ProcessUniqueCode == processCode);

            if (nextProcessId != null)
            {

                /*
                    1) 1-->2-->3-->4-->5 (put 2 after 4)

                    2) 1-->3-->4-->2-->5


                    A)4.NextProcessId = 2
                    B)1.NextProcessId = 3
                    c)2.NextProcessId = 5
                */
                Process process_1 = GetBefore(process.Id);
                Process process_5 = _unitOfWork.Repository<Process>().Get((int)nextProcessId);


                Process process_3 = null;
                if (process.NextProcessId != null)
                {
                    _unitOfWork.Repository<Process>().Get((int)process.NextProcessId);
                }
                Process process_4 = null;
                if (process_3 != null && process_3.NextProcessId != null)
                {
                    process_4 = _unitOfWork.Repository<Process>().Get((int)process_3.NextProcessId);
                }
                //A
                if (process_4 != null)
                {
                    process_4.NextProcessId = process.Id;
                    _unitOfWork.Repository<Process>().Update(process_4);
                }
                //B
                if (process_1 != null && process_3 != null)
                {
                    process_1.NextProcessId = process_3.Id;
                    _unitOfWork.Repository<Process>().Update(process_1);
                }

                //C
                if (process_5 != null)
                {
                    process.NextProcessId = process_5.Id;
                }
            }
            else
            {
                process.NextProcessId = null;
            }

            _unitOfWork.Repository<Process>().Update(process);
            _unitOfWork.Complete();


            SetStartingProcess(process.TaskId);
            SetWorkFlowDiagram(process.TaskId);

        }

        public IEnumerable<DecisionMethod> GetDecisionMethodList(int taskId)
        {
            return _unitOfWork.Repository<DecisionMethod>()
                .GetList(x => x.TaskId == taskId);
        }

        public IEnumerable<FormView> GetFormViewList(int taskId)
        {
            return _unitOfWork.Repository<FormView>()
                .GetList(x => x.TaskId == taskId);
        }

        public SummaryOfWorkFlowViewModel GetWorkFlowSummary(int taskId)
        {
            var mainProcessList = GetMainProcessList(taskId);
            var workFlowSummary = new SummaryOfWorkFlowViewModel();
            List<int> formIdList = new List<int>();
            List<int> decisionMethodList = new List<int>();
            foreach (var item in mainProcessList)
            {
                if (item is Condition)
                {
                    if (item is DecisionPoint)
                    {
                        workFlowSummary.NumberOfDecisionPoint++;
                        var kararMetodId = ((DecisionPoint)item).DecisionMethodId;
                        if (!decisionMethodList.Contains(kararMetodId))
                        {
                            decisionMethodList.Add(kararMetodId);
                        }
                    }
                    else
                    {
                        workFlowSummary.NumberOfCondition++;
                    }
                }
                else if (item is Process)
                {
                    if (item.FormViewId != null)
                    {
                        workFlowSummary.NumberOfProcessWhicHasSpecialForm++;
                        if (!formIdList.Contains((int)item.FormViewId))
                        {
                            formIdList.Add((int)item.FormViewId);
                        }
                    }
                    else
                    {
                        workFlowSummary.NumberOfStandartProcess++;
                    }
                }
            }

            workFlowSummary.NumberOfTotalProcess =
                workFlowSummary.NumberOfCondition +
                workFlowSummary.NumberOfDecisionPoint +
                workFlowSummary.NumberOfProcessWhicHasSpecialForm +
                workFlowSummary.NumberOfStandartProcess;


            workFlowSummary.NumberOfWillBeDesignedDecisionMethod = 0;

            foreach (var decisionMethodId in decisionMethodList)
            {
                if (!_unitOfWork.Repository<DecisionMethod>().Get(decisionMethodId).Completed)
                {
                    workFlowSummary.NumberOfWillBeDesignedDecisionMethod++;
                }
            }

            //isAkisOzeti.TasarlanacakKararMetoduSayisi = kullanilanKararMetoduIdleri.Count();

            var specialFormList = _unitOfWork.Repository<Process>().GetList(x => x.TaskId == taskId && formIdList.Any(v => v == x.FormViewId), x => x.FormView);
            foreach (var form in specialFormList)
            {
                if (!form.FormView.Completed)
                {

                    if (form.FormView.FormComplexity == FormComplexity.Easy)
                    {
                        workFlowSummary.NumberOfWillBeDesignedSimpleForm++;
                        workFlowSummary.NumberOfWillBeDesignedSimplePage = workFlowSummary.NumberOfWillBeDesignedSimplePage + form.FormView.NumberOfPage;
                    }
                    else if (form.FormView.FormComplexity == FormComplexity.Middle)
                    {
                        workFlowSummary.NumberOfWillBeDesignedMiddleForm++;
                        workFlowSummary.NumberOfWillBeDesignedMiddlePage = workFlowSummary.NumberOfWillBeDesignedMiddlePage + form.FormView.NumberOfPage;
                    }
                    else if (form.FormView.FormComplexity == FormComplexity.Complex)
                    {
                        workFlowSummary.NumberOfWillBeDesignedComplexForm++;
                        workFlowSummary.NumberOfWillBeDesignedComplexPage = workFlowSummary.NumberOfWillBeDesignedComplexPage + form.FormView.NumberOfPage;
                    }
                }

            }

            workFlowSummary.NumberOfTotalJobWillBeComplete = workFlowSummary.NumberOfWillBeDesignedDecisionMethod +
                (workFlowSummary.NumberOfWillBeDesignedSimpleForm + workFlowSummary.NumberOfWillBeDesignedMiddleForm + workFlowSummary.NumberOfWillBeDesignedComplexForm);


            return workFlowSummary;
        }

        public int CreateNewTask(WorkFlow testWorkFlow)
        {
            var newTask = new Task()
            {
                WorkFlow = testWorkFlow,
                Name = string.Format("Test - {0}", Guid.NewGuid()),
                MethodServiceName = "TestWorkFlowProcessService",
                Controller = "WorkFlowProcess",
                SpecialFormTemplateView = "WorkFlowTemplate"
            };
            _unitOfWork.Repository<Task>().Add(newTask);
            _unitOfWork.Complete();
            return newTask.Id;
        }

        public WorkFlow GetTestWorkFlow()
        {
            var testFlow = _unitOfWork.Repository<WorkFlow>().GetAll().FirstOrDefault(x => x.Name.CompareTo("Test Work Flow") == 0);
            if (testFlow == null)
            {
                testFlow = new WorkFlow()
                {
                    Name = "Test Work Flow"
                };

                _unitOfWork.Repository<WorkFlow>().Add(testFlow);
                _unitOfWork.Complete();
            }

            return testFlow;
        }

        public IEnumerable<Process> GetMainProcessList(int gorevId)
        {
            return _unitOfWork.Repository<Process>().GetList().Where(x =>
            {
                if (x.TaskId != gorevId)
                {
                    return false;
                }
                bool rslt = true;
                if (x.GetType() == typeof(ConditionOption))
                {
                    rslt = false;
                }
                return rslt;
            }).OrderBy(x => x.Name);
        }


        public Process GetProcess(int processId)
        {
            Process process = _unitOfWork.Repository<Process>().Get(x => x.Id == processId, x => x.MonitoringRoleList, x => x.DocumentList);

            if (process.GetType() == typeof(ConditionOption))
            {
                process = _unitOfWork.Repository<ConditionOption>().Get(x => x.Id == processId, x => x.MonitoringRoleList, x => x.Condition);
            }
            if (process.GetType() == typeof(DecisionPoint))
            {
                process = _unitOfWork.Repository<DecisionPoint>().Get(x => x.Id == processId, x => x.MonitoringRoleList, x => x.DecisionMethod);
            }
            return process;
        }

        public Process GetProcess(string processCode)
        {
            Process process = _unitOfWork.Repository<Process>().Get(x => x.ProcessUniqueCode == processCode);

            return GetProcess(process.Id);
        }


        private IEnumerable<Process> ProcessListWhichBeforeIsNull(int gorevId)
        {
            return GetMainProcessList(gorevId).Where(x => GetBefore(x.Id) == null);
        }

        private void SetStartingProcess(int taskId)
        {
            Task task = _unitOfWork.Repository<Task>().Get(taskId);

            var processListWhichBeforeIsNull = ProcessListWhichBeforeIsNull(taskId);
            if (processListWhichBeforeIsNull.Count() > 0)
            {
                if (task.StartingProcessId == null || processListWhichBeforeIsNull.Count() == 1)
                {
                    task.StartingProcessId = processListWhichBeforeIsNull.First().Id;
                    _unitOfWork.Repository<Task>().Update(task);
                    _unitOfWork.Complete();
                }
            }
        }

        public void AddOrUpdate(ProcessForm formData)
        {
            int processId = 0;

            if (formData.ProcessType == ProcessType.Process)
            {
                var process = Mapper.Map<ProcessForm, Process>(formData);
                processId = AddOrUpdate(process);
            }
            else if (formData.ProcessType == ProcessType.Condition)
            {
                var process = Mapper.Map<ProcessForm, Condition>(formData);
                processId = AddOrUpdate(process);
            }
            else if (formData.ProcessType == ProcessType.OptionList)
            {
                var process = Mapper.Map<ProcessForm, ConditionOption>(formData);
                processId = AddOrUpdate(process);
            }
            else if (formData.ProcessType == ProcessType.DecisionPoint)
            {
                var process = Mapper.Map<ProcessForm, DecisionPoint>(formData);
                processId = AddOrUpdate(process);
            }
            else if (formData.ProcessType == ProcessType.SubProcess)
            {
                var process = Mapper.Map<ProcessForm, SubProcess>(formData);
                processId = AddOrUpdate(process);
            }

            if (formData.Id == 0 && processId != 0)
            {
                formData.Id = processId;
                if (formData.ProcessType == ProcessType.Process || formData.ProcessType == ProcessType.Condition)
                {
                    SetStartingProcess(formData.TaskId);
                }
            }


            SetWorkFlowDiagram(formData.TaskId);
        }
    }
}