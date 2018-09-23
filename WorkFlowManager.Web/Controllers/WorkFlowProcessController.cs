using AutoMapper;
using System.Web.Mvc;
using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Extensions;
using WorkFlowManager.Common.InfraStructure;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.ViewModels;
using WorkFlowManager.Services.DbServices;

namespace WorkFlowManager.Web.Controllers
{

    public class WorkFlowProcessController : Controller
    {

        private readonly WorkFlowProcessService _workFlowProcessService;

        public WorkFlowProcessController(WorkFlowProcessService workFlowProcessService)
        {
            _workFlowProcessService = workFlowProcessService;
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public ActionResult Iptal(int workFlowTraceId)
        {
            _workFlowProcessService.WorkFlowProcessCancel(workFlowTraceId);

            var surecKontrolSonuc = _workFlowProcessService.SetNextProcessForWorkFlow(workFlowTraceId);

            return RedirectToAction("Index", surecKontrolSonuc).WithMessage(this, "Process cancelled.", MessageType.Success);

        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        [MultipleButton(Name = "action", Argument = "GeriAl")]
        public ActionResult GeriAl(WorkFlowFormViewModel formData)
        {
            if (formData.Description == null)
            {
                string errorMessage = "Please enter your cancellation reason.";
                ModelState.AddModelError("ProcessForm.Description", errorMessage);
                UserProcessViewModel kullaniciIslemVM = _workFlowProcessService.GetUserProcessVM(formData.Id);
                var workFlowBase = _workFlowProcessService.WorkFlowBaseInfo(kullaniciIslemVM);
                _workFlowProcessService.SetWorkFlowTraceForm(formData, workFlowBase);

                return View(formData.ProcessTaskSpecialFormTemplateView, formData).WithMessage(this, "Error occured!", MessageType.Warning);
            }

            WorkFlowTrace islem = Mapper.Map<WorkFlowFormViewModel, WorkFlowTrace>(formData);
            _workFlowProcessService.AddOrUpdate(islem);

            var workFlowTraceId = formData.Id;
            _workFlowProcessService.CancelWorkFlowTrace(workFlowTraceId, formData.TargetProcessId);

            var surecKontrolSonuc = _workFlowProcessService.SetNextProcessForWorkFlow(workFlowTraceId);

            return RedirectToAction("Index", surecKontrolSonuc).WithMessage(this, "Previous step started.", MessageType.Success);

        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        [MultipleButton(Name = "action", Argument = "SaveAsDraft")]
        public ActionResult SaveAsDraft(WorkFlowFormViewModel formData)
        {
            if (ModelState.IsValid)
            {
                if (formData.ProcessFormViewCompleted && formData.GetType() != typeof(WorkFlowFormViewModel))
                {
                    _workFlowProcessService.CustomFormSave(formData);
                }
                else
                {

                    WorkFlowTrace torSatinAlmaIslem = Mapper.Map<WorkFlowFormViewModel, WorkFlowTrace>(formData);
                    _workFlowProcessService.AddOrUpdate(torSatinAlmaIslem);
                }

                UserProcessViewModel kullaniciIslemVM = _workFlowProcessService.GetUserProcessVM(formData.Id);
                var workFlowBase = _workFlowProcessService.WorkFlowBaseInfo(kullaniciIslemVM);
                _workFlowProcessService.SetWorkFlowTraceForm(formData, workFlowBase);


                return View(formData.ProcessTaskSpecialFormTemplateView, formData).WithMessage(this, string.Format("{0} saved successfully.", formData.ProcessName), MessageType.Success);
            }
            else
            {
                UserProcessViewModel kullaniciIslemVM = _workFlowProcessService.GetUserProcessVM(formData.Id);
                var workFlowBase = _workFlowProcessService.WorkFlowBaseInfo(kullaniciIslemVM);
                _workFlowProcessService.SetWorkFlowTraceForm(formData, workFlowBase);

                return View(formData.ProcessTaskSpecialFormTemplateView, formData).WithMessage(this, string.Format("Validation error!"), MessageType.Danger);
            }
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        [MultipleButton(Name = "action", Argument = "SaveAndSend")]
        public ActionResult SaveAndSend(WorkFlowFormViewModel formData)
        {

            if (ModelState.IsValid)
            {

                bool fullFormValidate = _workFlowProcessService.FullFormValidate(formData, ModelState);

                if (!fullFormValidate)
                {

                    UserProcessViewModel userProcessVM = _workFlowProcessService.GetUserProcessVM(formData.Id);
                    var workFlowBase = _workFlowProcessService.WorkFlowBaseInfo(userProcessVM);
                    _workFlowProcessService.SetWorkFlowTraceForm(formData, workFlowBase);

                    return View(formData.ProcessTaskSpecialFormTemplateView, formData).WithMessage(this, "Validation error!", MessageType.Warning);
                }

                WorkFlowTrace workFlowTrace = Mapper.Map<WorkFlowFormViewModel, WorkFlowTrace>(formData);

                if (formData.ProcessFormViewCompleted)
                {
                    if (formData.GetType() != typeof(WorkFlowFormViewModel))
                    {
                        _workFlowProcessService.CustomFormSave(formData);
                    }
                    else
                    {
                        _workFlowProcessService.AddOrUpdate(workFlowTrace);
                    }
                }
                else
                {
                    _workFlowProcessService.AddOrUpdate(workFlowTrace);
                }


                _workFlowProcessService.GoToWorkFlowNextProcess(workFlowTrace.OwnerId);
                var targetProcess = _workFlowProcessService.SetNextProcessForWorkFlow(workFlowTrace.Id);

                return RedirectToAction("Index", targetProcess).WithMessage(this, "Saved Successfully.", MessageType.Success);
            }
            else
            {
                UserProcessViewModel userProcessVM = _workFlowProcessService.GetUserProcessVM(formData.Id);
                var workFlowBase = _workFlowProcessService.WorkFlowBaseInfo(userProcessVM);
                _workFlowProcessService.SetWorkFlowTraceForm(formData, workFlowBase);

                return View(formData.ProcessTaskSpecialFormTemplateView, formData).WithMessage(this, string.Format("Validation error!"), MessageType.Danger);
            }
        }

    }
}

