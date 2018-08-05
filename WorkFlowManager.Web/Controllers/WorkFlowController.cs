using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Extensions;
using WorkFlowManager.Common.InfraStructure;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.Validation;
using WorkFlowManager.Common.ViewModels;
using WorkFlowManager.Helper;
using WorkFlowManager.Services.DbServices;

namespace WorkFlowManager.Web.Controllers
{
    public class WorkFlowController : Controller
    {

        private const string ViewForm = "ProcessForm";
        private readonly IUnitOfWork _unitOfWork;
        private readonly WorkFlowService _workFlowService;
        private readonly FormService _formService;
        private readonly DecisionMethodService _decisionMethodService;

        public WorkFlowController(IUnitOfWork unitOfWork, WorkFlowService workFlowService)
        {
            _unitOfWork = unitOfWork;
            _workFlowService = workFlowService;
            _formService = new FormService(_unitOfWork);
            _decisionMethodService = new DecisionMethodService(_unitOfWork);
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        [MultipleButton(Name = "action", Argument = "Save")]
        public ActionResult Save(ProcessForm formData)
        {
            if (SaveProcessForm(ref formData))
            {
                ProcessFormInitialize(ref formData);
                ModelState.Clear();
            }
            return View(ViewForm, formData);
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        [MultipleButton(Name = "action", Argument = "SaveClose")]
        public ActionResult SaveClose(ProcessForm formData)
        {
            if (SaveProcessForm(ref formData))
            {
                return RedirectToAction("Index", new { taskId = formData.TaskId }).WithMessage(this, string.Format("{0} Saved Successfully.", formData.Name), MessageType.Success);
            }
            else
            {
                //ProcessFormInitialize(ref formData);
                return View(ViewForm, formData);
            }
        }


        public ProcessForm ProcessFormLoad(ProcessForm formData)
        {
            var process = _unitOfWork.Repository<Process>().Get(x => x.Id == formData.Id, x => x.DocumentList);

            if (process != null)
            {
                formData.TemplateFileList = new FileUpload(process.DocumentList, FileType.ProcessTemplateFile, process.Id);
                formData.AnalysisFileList = new FileUpload(process.DocumentList, FileType.AnalysisFile, process.Id);
                formData.ProcessUniqueCode = process.ProcessUniqueCode;
            }

            return formData;
        }

        public bool SaveProcessForm(ref ProcessForm formData)
        {
            if (formData.Id != 0)
            {
                formData = ProcessFormLoad(formData);
            }

            bool result = ValidationHelper.Validate(formData, new ProcessFormValidator(_unitOfWork), ModelState);

            if (result == false)
            {
                ProcessFormInitialize(ref formData);
                return result;
            }

            _workFlowService.AddOrUpdate(formData);
            return result;
        }


        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public ActionResult Delete(int processId, int taskId)
        {
            try
            {
                string result = _workFlowService.Delete(processId);
                return RedirectToAction("Index", new { taskId = taskId }).WithMessage(this, result, MessageType.Success);
            }
            catch (System.Exception ex)
            {

                return RedirectToAction("Index", new { taskId = taskId }).WithMessage(this, string.Format("{0}", ex.Message), MessageType.Danger);
            }
        }

        public ActionResult EditByProcessCode(string processCode)
        {
            var process = _workFlowService.GetProcess(processCode);
            return Edit(process.Id);
        }

        public ActionResult Edit(int processId)
        {
            var process = _workFlowService.GetProcess(processId);

            if (process == null)
                return HttpNotFound();


            var formData = Mapper.Map<Process, ProcessForm>(process);
            ProcessFormInitialize(ref formData);

            return View(ViewForm, formData);
        }


        public ActionResult SetNext(string taskId, string processCode, int? nextId)
        {
            try
            {
                _workFlowService.SetNextByProcessCode(processCode, nextId);
                return RedirectToAction("Index", new { taskId = taskId }).WithMessage(this, "Done.", MessageType.Success);
            }
            catch (System.Exception ex)
            {

                return RedirectToAction("Index", new { taskId = taskId }).WithMessage(this, string.Format("{0}", ex.Message), MessageType.Danger);
            }
        }
        public void ProcessFormInitialize(ref ProcessForm formData)
        {
            var mainProcessList = _workFlowService.GetMainProcessList(formData.TaskId);

            formData.MainProcessList = (mainProcessList != null ? new SelectList(mainProcessList, "Id", "Name") : null);

            if (formData.ProcessType == ProcessType.DecisionPoint)
            {
                formData.DecisionMethodList = new SelectList(_workFlowService.GetDecisionMethodList(formData.TaskId), "Id", "MethodName");
                formData.RepetitionHourList = new SelectList(Enumerable.Range(1, 24));
            }
            else if (formData.ProcessType == ProcessType.Process)
            {
                formData.FormViewList = new SelectList(_workFlowService.GetFormViewList(formData.TaskId), "Id", "FormName");
            }


            if (formData.ProcessType == ProcessType.OptionList || formData.ProcessType == ProcessType.Process)
            {
                var monitoringList = formData.MonitoringRoleList;
                formData.MonitoringRoleList = Global.GetAllRoles().Select(rol => new MonitoringRoleCheckbox
                {
                    ProjectRole = rol,
                    IsChecked = monitoringList != null ? monitoringList.Where(x => x.IsChecked == true).Any(t => t.ProjectRole == rol) : false
                }).ToList();
            }

            if (formData.ConditionId != null && formData.ConditionId != 0)
            {
                var condition = _unitOfWork.Repository<Condition>().Get((int)formData.ConditionId);
                if (condition != null)
                {
                    formData.ConditionName = condition.Name;
                    formData.AssignedRole = condition.AssignedRole;
                }
            }
            else
            {
                if (formData.Id == 0)
                {
                    formData.AssignedRole = (formData.ProcessType == ProcessType.DecisionPoint ? ProjectRole.Sistem : ProjectRole.ProjectProcurmentOfficer);
                }
            }
            formData = ProcessFormLoad(formData);
        }

        public ActionResult New(int taskId, ProcessType processType = ProcessType.Process, int conditionId = 0)
        {
            //var _roleList = Global.GetAllRoles();
            IEnumerable<Process> mainProcessList = null;
            List<MonitoringRoleCheckbox> monitoringRoleList = null;
            Condition condition = null;

            //if (processType == ProcessType.OptionList || processType == ProcessType.Process)
            //{
            //    monitoringRoleList = _roleList.Select(rol => new MonitoringRoleCheckbox
            //    {
            //        ProjectRole = rol,
            //        IsChecked = false
            //    }).ToList();

            //    mainProcessList = _workFlowService.GetMainProcessList(taskId);

            //    if (processType == ProcessType.OptionList)
            //    {
            //        condition = _unitOfWork.Repository<Condition>().Get(conditionId);
            //    }
            //}
            var processForm = new ProcessForm();

            processForm.TaskId = taskId;
            processForm.ProcessType = processType;
            processForm.ConditionId = conditionId;

            ProcessFormInitialize(ref processForm);


            //return View(ViewForm, new ProcessForm
            //{
            //    ConditionName = (condition != null ? condition.Name : null),
            //    AssignedRole = (condition != null ? condition.AssignedRole : (processType == ProcessType.DecisionPoint ? ProjectRole.Sistem : ProjectRole.ProjectProcurmentOfficer)),
            //    ConditionId = conditionId,
            //    TaskId = taskId,
            //    ProcessType = processType,
            //    MainProcessList = (mainProcessList != null ? new SelectList(mainProcessList, "Id", "NameWithRole") : null),
            //    DecisionMethodList = (processType == ProcessType.DecisionPoint ? new SelectList(_workFlowService.GetDecisionMethodList(taskId), "Id", "MethodName") : null),
            //    RepetitionHourList = (processType == ProcessType.DecisionPoint ? new SelectList(Enumerable.Range(1, 24)) : null),
            //    FormViewList = (processType == ProcessType.Process ? new SelectList(_workFlowService.GetFormViewList(taskId), "Id", "FormName") : null),
            //    AnalysisFileList = null,
            //    TemplateFileList = null,
            //    MonitoringRoleList = monitoringRoleList
            //});

            return View(ViewForm, processForm);

        }


        [HttpGet]
        // GET: Pydb/IsAkisi
        public ActionResult WorkFlow(int taskId = 0)
        {
            var taskList = _workFlowService.GetTaskList();

            var actkiveTask = taskList.SingleOrDefault(x => x.Id == taskId);
            var mainProcess = _workFlowService.GetMainProcessList(taskId);

            var nextProcessList = _workFlowService.GetProcessList(taskId).Select(x => new NextProcess
            {
                MainProcessList = (mainProcess != null ? new SelectList(mainProcess, "Id", "Name") : null),
                Process = x
            });

            WorkFlowViewModel workFlowViewModel = null;

            if (nextProcessList.Count() > 0)
            {
                workFlowViewModel = new WorkFlowViewModel
                {
                    ActiveTaskId = taskId,
                    FirstProcessId = (actkiveTask.StartingProcessId != null ? (int)actkiveTask.StartingProcessId : 0),
                    NextProcessList = nextProcessList
                };
            }
            else
            {
                workFlowViewModel = new WorkFlowViewModel
                {
                    ActiveTaskId = taskId,
                    NextProcessList = null
                };
            }


            return PartialView("_WorkFlowWorkbench", workFlowViewModel);

        }


        // GET: Pydb/IsAkisi
        public ActionResult Index(int taskId = 0)
        {

            var taskList = _workFlowService.GetTaskList();

            if (taskId == 0)
            {
                taskId = taskList.ToList().First().Id;
            }

            var activeTask = taskList.SingleOrDefault(x => x.Id == taskId);


            WorkFlowIndexViewModel workFlowIndexViewModel = null;


            workFlowIndexViewModel = new WorkFlowIndexViewModel
            {
                ActiveTaskId = taskId,
                ActiveTaskName = string.Format("{0}({1})", activeTask.WorkFlow.Name, activeTask.Name),
                TaskList = taskList
            };


            return View(workFlowIndexViewModel);

        }

        [ChildActionOnly]
        public ActionResult SpecialForm(int taskId)
        {
            return PartialView("_SpecialForms", _formService.Read(taskId));
        }



        [ChildActionOnly]
        public ActionResult DecisionMethod(int taskId)
        {
            return PartialView("_DecisionMethod", _decisionMethodService.Read(taskId));
        }


        [ChildActionOnly]
        public ActionResult WorkFlowSummary(int taskId)
        {
            return PartialView("_WorkFlowSummary", _workFlowService.GetWorkFlowSummary(taskId));
        }

        [ChildActionOnly]
        public ActionResult ShowWorkFlow(int taskId)
        {
            return Content(_workFlowService.GetWorkFlowDiagram(taskId));
        }


        public ActionResult ShowWorkFlowFullScreen(int taskId)
        {
            string gorevAkis = _workFlowService.GetWorkFlowDiagram(taskId);
            return PartialView("_MShowWorkFlow", new WorkFlowView { Flag = false, WorkFlowText = gorevAkis });
        }

    }
}