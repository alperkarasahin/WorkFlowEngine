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

        public ActionResult AkisGoster(int workFlowTraceId)
        {
            UserProcessViewModel kullaniciIslemVM = _testWorkFlowProcessService.GetKullaniciProcessVM(workFlowTraceId);
            string gorevAkis = _workFlowService.GetWorkFlowDiagram(kullaniciIslemVM.TaskId);
            var mevcutIsAkisi = _testWorkFlowProcessService.GetIsAkisi(gorevAkis, workFlowTraceId);
            return PartialView("_MAkisGoster", new WorkFlowView { Flag = true, WorkFlowText = mevcutIsAkisi });
        }

        public ActionResult Task1Test()
        {
            var masterTest1 = _unitOfWork.Repository<MasterTest>().Get(x => x.Name == "Task1 Test");
            var task1 = _unitOfWork.Repository<Task>().Get(x => x.Name == "Task1");

            var masterTestTrace = _unitOfWork.Repository<WorkFlowTrace>().Find(x => x.OwnerId == masterTest1.Id).OrderByDescending(x => x.Id);
            if (masterTestTrace.Count() > 0)
            {
                return Index(masterTestTrace.First().Id);
            }

            return StartWorkFlow(masterTest1.Id, task1.Id);
        }

        public ActionResult Task2Test()
        {
            var masterTest2 = _unitOfWork.Repository<MasterTest>().Get(x => x.Name == "Task2 Test");
            var task2 = _unitOfWork.Repository<Task>().Get(x => x.Name == "Task2");
            return StartWorkFlow(masterTest2.Id, task2.Id);
        }

        public ActionResult StartWorkFlow(int ownerId, int taskId)
        {
            int torSatinAlmaIslemId = _testWorkFlowProcessService.StartWorkFlow(ownerId, taskId);
            return Index(torSatinAlmaIslemId);
        }

        public ActionResult Index(int workFlowTraceId)
        {
            UserProcessViewModel kullaniciIslemVM = _testWorkFlowProcessService.GetKullaniciProcessVM(workFlowTraceId);
            if (_testWorkFlowProcessService.WorkFlowYetkiKontrol(kullaniciIslemVM) == false)
            {
                return RedirectToAction("Index", new { controller = "Home" }).WithMessage(this, "Erişim Yetkiniz Yok!", MessageType.Danger);
            }


            WorkFlowTrace torSatinAlmaIslem = _testWorkFlowProcessService.WorkFlowTraceDetail(workFlowTraceId);
            //WorkFlowTraceForm satinAlmaIslemForm = new WorkFlowTraceForm();


            WorkFlowTraceForm satinAlmaIslemForm = Mapper.Map<WorkFlowTrace, WorkFlowTraceForm>(torSatinAlmaIslem);
            int ownerId = satinAlmaIslemForm.OwnerId;
            ActionResult viewResult = null;

            WorkFlowFormViewModel satinAlmaOzelForm = null;

            var workFlowBase = _testWorkFlowProcessService.WorkFlowBaseInfo(kullaniciIslemVM);
            _testWorkFlowProcessService.SetSatinAlmaWorkFlowTraceForm(satinAlmaIslemForm, workFlowBase);


            switch (satinAlmaIslemForm.ProcessFormViewViewName)
            {
                case "TestForm":
                    satinAlmaOzelForm = _testWorkFlowProcessService.SatinAlmaOzelForm<TestFormViewModel>(ownerId, satinAlmaIslemForm);
                    break;

                default:
                    satinAlmaOzelForm = _testWorkFlowProcessService.SatinAlmaOzelForm<WorkFlowFormViewModel>(ownerId, satinAlmaIslemForm);
                    break;

            }

            //satinAlmaOzelForm.Controller = workFlowBase.Controller;
            //satinAlmaOzelForm.OzelFormSablonView = workFlowBase.SpecialFormTemplateView;

            viewResult = View(satinAlmaOzelForm.WorkFlowIslemForm.ProcessTaskSpecialFormTemplateView, satinAlmaOzelForm);
            return viewResult;

        }

    }
}