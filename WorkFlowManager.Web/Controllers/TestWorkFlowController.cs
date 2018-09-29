using AutoMapper;
using System.Linq;
using System.Web.Mvc;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Extensions;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.ViewModels;
using WorkFlowManager.Services.DbServices;

namespace WorkFlowManager.Web.Controllers
{
    public class TestWorkFlowProcessController : WorkFlowProcessController
    {
        private TestWorkFlowProcessService _testWorkFlowProcessService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly WorkFlowService _workFlowService;
        public TestWorkFlowProcessController(
            IUnitOfWork unitOfWork, TestWorkFlowProcessService testWorkFlowService, WorkFlowService workFlowService) : base(testWorkFlowService)
        {
            _unitOfWork = unitOfWork;
            _testWorkFlowProcessService = testWorkFlowService;
            _workFlowService = workFlowService;
        }

        public ActionResult ShowWorkFlow(int workFlowTraceId)
        {
            UserProcessViewModel kullaniciIslemVM = _testWorkFlowProcessService.GetUserProcessVM(workFlowTraceId);
            string gorevAkis = _workFlowService.GetWorkFlowDiagram(kullaniciIslemVM.TaskId);
            var mevcutIsAkisi = _testWorkFlowProcessService.GetWorkFlow(gorevAkis, workFlowTraceId);
            return PartialView("_MAkisGoster", new WorkFlowView { Flag = true, WorkFlowText = mevcutIsAkisi });
        }

        public ActionResult Task1Test()
        {
            var masterTest1 = _unitOfWork.Repository<MasterTest>().GetAll().FirstOrDefault();
            var task1 = _unitOfWork.Repository<Task>().GetAll().FirstOrDefault();

            var masterTestTrace = _unitOfWork.Repository<WorkFlowTrace>().Find(x => x.OwnerId == masterTest1.Id).OrderByDescending(x => x.Id);
            if (masterTestTrace.Count() > 0)
            {
                return Index(masterTestTrace.First().Id);
            }

            return StartWorkFlow(masterTest1.Id, task1.Id);
        }


        public ActionResult StartWorkFlow(int ownerId, int taskId)
        {
            int torSatinAlmaIslemId = _testWorkFlowProcessService.StartWorkFlow(ownerId, taskId);
            return Index(torSatinAlmaIslemId);
        }

        public ActionResult Index(int workFlowTraceId)
        {
            UserProcessViewModel kullaniciIslemVM = _testWorkFlowProcessService.GetUserProcessVM(workFlowTraceId);
            if (_testWorkFlowProcessService.WorkFlowPermissionCheck(kullaniciIslemVM) == false)
            {
                return RedirectToAction("Index", new { controller = "Home" }).WithMessage(this, "Erişim Yetkiniz Yok!", MessageType.Danger);
            }

            WorkFlowTrace workFlowTrace = _testWorkFlowProcessService.WorkFlowTraceDetail(workFlowTraceId);

            WorkFlowFormViewModel workFlowTraceForm = Mapper.Map<WorkFlowTrace, WorkFlowFormViewModel>(workFlowTrace);
            int ownerId = workFlowTraceForm.OwnerId;
            ActionResult viewResult = null;

            WorkFlowFormViewModel workFlowForm = null;

            var workFlowBase = _testWorkFlowProcessService.WorkFlowBaseInfo(kullaniciIslemVM);
            _testWorkFlowProcessService.SetWorkFlowTraceForm(workFlowTraceForm, workFlowBase);

            workFlowForm = _testWorkFlowProcessService.WorkFlowFormLoad(workFlowTraceForm);

            viewResult = View(workFlowForm.ProcessTaskSpecialFormTemplateView, workFlowForm);
            return viewResult;

        }

    }
}