using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Extensions;
using WorkFlowManager.Common.InfraStructure;
using WorkFlowManager.Common.Mappers;
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
        public WorkFlowController(IUnitOfWork unitOfWork, WorkFlowService workFlowService, FormService formService, DecisionMethodService decisionMethodService)
        {
            _unitOfWork = unitOfWork;
            _workFlowService = workFlowService;
            _formService = formService;
            _decisionMethodService = decisionMethodService;
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
                ProcessFormInitialize(ref formData);
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

            _workFlowService.AddOrUpdate(formData.ProcessType, formData);
            return result;
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


            formData = ProcessFormLoad(formData);

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


            var gorevIslemForm = process.ToProcessForm(
                _workFlowService.GetMainProcessList(process.TaskId),
                    _workFlowService.GetDecisionMethodList(process.TaskId),
                        _workFlowService.GetFormViewList(process.TaskId));

            return View(ViewForm, gorevIslemForm);
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

        public ActionResult New(int taskId, ProcessType processType = ProcessType.Process, int conditionId = 0)
        {
            var _roleList = Global.GetAllRoles();
            IEnumerable<Process> mainProcessList = null;
            List<MonitoringRoleCheckbox> monitoringRoleList = null;
            Condition condition = null;

            if (processType == ProcessType.OptionList || processType == ProcessType.Process)
            {
                monitoringRoleList = _roleList.Select(rol => new MonitoringRoleCheckbox
                {
                    ProjectRole = rol,
                    IsChecked = false
                }).ToList();

                mainProcessList = _workFlowService.GetMainProcessList(taskId);

                if (processType == ProcessType.OptionList)
                {
                    condition = _unitOfWork.Repository<Condition>().Get(conditionId);
                }
            }



            return View(ViewForm, new ProcessForm
            {
                ConditionName = (condition != null ? condition.Name : null),
                AssignedRole = (condition != null ? condition.AssignedRole : (processType == ProcessType.DecisionPoint ? ProjectRole.Sistem : ProjectRole.ProjectProcurmentOfficer)),
                ConditionId = conditionId,
                TaskId = taskId,
                ProcessType = processType,
                MainProcessList = (mainProcessList != null ? new SelectList(mainProcessList, "Id", "NameWithRole") : null),
                DecisionMethodList = (processType == ProcessType.DecisionPoint ? new SelectList(_workFlowService.GetDecisionMethodList(taskId), "Id", "MethodName") : null),
                RepetitionHourList = (processType == ProcessType.DecisionPoint ? new SelectList(Enumerable.Range(1, 24)) : null),
                FormViewList = (processType == ProcessType.Process ? new SelectList(_workFlowService.GetFormViewList(taskId), "Id", "FormName") : null),
                AnalysisFileList = null,
                TemplateFileList = null,
                MonitoringRoleList = monitoringRoleList
            });



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
            return PartialView("_SpecialForms", taskId);
        }

        public ActionResult FormEditingInline_Read([DataSourceRequest] DataSourceRequest request, int taskId)
        {
            return Json(_formService.Read(taskId).ToDataSourceResult(request));
        }

        [AcceptVerbs(System.Web.Mvc.HttpVerbs.Post)]
        public ActionResult FormEditingInline_Create([DataSourceRequest] DataSourceRequest request, FormViewViewModel formView)
        {
            formView.FormName = formView.FormName.Trim();
            formView.ViewName = formView.FormName.ToPascalCase();
            bool result = ValidationHelper.Validate(formView, new FormViewValidation(_unitOfWork), ModelState);


            if (result)
            {
                _formService.Create(formView);
            }

            return Json(new[] { formView }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(System.Web.Mvc.HttpVerbs.Post)]
        public ActionResult FormEditingInline_Update([DataSourceRequest] DataSourceRequest request, FormViewViewModel formView)
        {
            formView.FormName = formView.FormName.Trim();
            formView.ViewName = formView.FormName.ToPascalCase();
            bool result = ValidationHelper.Validate(formView, new FormViewValidation(_unitOfWork), ModelState);


            if (result)
            {
                _formService.Update(formView);
            }

            return Json(new[] { formView }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(System.Web.Mvc.HttpVerbs.Post)]
        public ActionResult FormEditingInline_Destroy([DataSourceRequest] DataSourceRequest request, FormViewViewModel formView)
        {
            if (formView != null)
            {
                _formService.Destroy(formView);
            }

            return Json(new[] { formView }.ToDataSourceResult(request, ModelState));
        }

        [ChildActionOnly]
        public ActionResult DecisionMethod(int taskId)
        {
            return PartialView("_DecisionMethod", taskId);
        }

        public ActionResult DecisionMethodEditingInline_Read([DataSourceRequest] DataSourceRequest request, int taskId)
        {
            return Json(_decisionMethodService.Read(taskId).ToDataSourceResult(request));
        }

        [AcceptVerbs(System.Web.Mvc.HttpVerbs.Post)]
        public ActionResult DecisionMethodEditingInline_Create([DataSourceRequest] DataSourceRequest request, DecisionMethodViewModel decisionMethod)
        {
            decisionMethod.MethodSql = decisionMethod.MethodSql.Trim();
            decisionMethod.MethodName = decisionMethod.MethodName.Trim();

            bool result = ValidationHelper.Validate(decisionMethod, new DecisionMethodValidation(_unitOfWork), ModelState);
            if (result)
            {
                _decisionMethodService.Create(decisionMethod);
            }

            return Json(new[] { decisionMethod }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(System.Web.Mvc.HttpVerbs.Post)]
        public ActionResult DecisionMethodEditingInline_Update([DataSourceRequest] DataSourceRequest request, DecisionMethodViewModel decisionMethod)
        {
            decisionMethod.MethodSql = decisionMethod.MethodSql.Trim();
            decisionMethod.MethodName = decisionMethod.MethodName.Trim();

            bool result = ValidationHelper.Validate(decisionMethod, new DecisionMethodValidation(_unitOfWork), ModelState);
            if (result)
            {
                _decisionMethodService.Update(decisionMethod);
            }

            return Json(new[] { decisionMethod }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(System.Web.Mvc.HttpVerbs.Post)]
        public ActionResult DecisionMethodEditingInline_Destroy([DataSourceRequest] DataSourceRequest request, DecisionMethodViewModel decisonMethod)
        {
            if (decisonMethod != null)
            {
                _decisionMethodService.Destroy(decisonMethod);
            }

            return Json(new[] { decisonMethod }.ToDataSourceResult(request, ModelState));
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