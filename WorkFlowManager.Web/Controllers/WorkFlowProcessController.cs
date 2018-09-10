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
                UserProcessViewModel kullaniciIslemVM = _workFlowProcessService.GetKullaniciProcessVM(formData.Id);
                var workFlowBase = _workFlowProcessService.WorkFlowBaseInfo(kullaniciIslemVM);
                _workFlowProcessService.SetSatinAlmaWorkFlowTraceForm(formData, workFlowBase);

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
        [MultipleButton(Name = "action", Argument = "TaslakKaydetOzelForm")]
        public ActionResult TaslakKaydetOzelForm(WorkFlowFormViewModel formData)
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

                UserProcessViewModel kullaniciIslemVM = _workFlowProcessService.GetKullaniciProcessVM(formData.Id);
                var workFlowBase = _workFlowProcessService.WorkFlowBaseInfo(kullaniciIslemVM);
                _workFlowProcessService.SetSatinAlmaWorkFlowTraceForm(formData, workFlowBase);


                return View(formData.ProcessTaskSpecialFormTemplateView, formData).WithMessage(this, string.Format("{0} saved successfully.", formData.ProcessName), MessageType.Success);
            }
            else
            {
                UserProcessViewModel kullaniciIslemVM = _workFlowProcessService.GetKullaniciProcessVM(formData.Id);
                var workFlowBase = _workFlowProcessService.WorkFlowBaseInfo(kullaniciIslemVM);
                _workFlowProcessService.SetSatinAlmaWorkFlowTraceForm(formData, workFlowBase);

                return View(formData.ProcessTaskSpecialFormTemplateView, formData).WithMessage(this, string.Format("Validation error!"), MessageType.Danger);
            }
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        [MultipleButton(Name = "action", Argument = "GonderOzelForm")]
        public ActionResult KaydetGonderOzelForm(WorkFlowFormViewModel formData)
        {

            if (ModelState.IsValid)
            {

                bool fullFormValidate = _workFlowProcessService.FullFormValidate(formData, ModelState);

                if (!fullFormValidate)
                {

                    UserProcessViewModel kullaniciIslemVM = _workFlowProcessService.GetKullaniciProcessVM(formData.Id);
                    var workFlowBase = _workFlowProcessService.WorkFlowBaseInfo(kullaniciIslemVM);
                    _workFlowProcessService.SetSatinAlmaWorkFlowTraceForm(formData, workFlowBase);

                    return View(formData.ProcessTaskSpecialFormTemplateView, formData).WithMessage(this, "Validation error!", MessageType.Warning);
                }

                WorkFlowTrace torSatinAlmaIslem = Mapper.Map<WorkFlowFormViewModel, WorkFlowTrace>(formData);

                if (formData.ProcessFormViewCompleted)
                {
                    if (formData.GetType() != typeof(WorkFlowFormViewModel))
                    {
                        _workFlowProcessService.CustomFormSave(formData);
                    }
                    else
                    {
                        _workFlowProcessService.AddOrUpdate(torSatinAlmaIslem);
                    }
                }
                else
                {
                    _workFlowProcessService.AddOrUpdate(torSatinAlmaIslem);
                }


                _workFlowProcessService.WorkFlowWorkFlowNextProcess(torSatinAlmaIslem.OwnerId);
                var surecKontrolSonuc = _workFlowProcessService.SetNextProcessForWorkFlow(torSatinAlmaIslem.Id);

                return RedirectToAction("Index", surecKontrolSonuc).WithMessage(this, "Saved Successfully.", MessageType.Success);
            }
            else
            {
                UserProcessViewModel kullaniciIslemVM = _workFlowProcessService.GetKullaniciProcessVM(formData.Id);
                var workFlowBase = _workFlowProcessService.WorkFlowBaseInfo(kullaniciIslemVM);
                _workFlowProcessService.SetSatinAlmaWorkFlowTraceForm(formData, workFlowBase);

                return View(formData.ProcessTaskSpecialFormTemplateView, formData).WithMessage(this, string.Format("Validation error!"), MessageType.Danger);
            }
        }

    }
}

