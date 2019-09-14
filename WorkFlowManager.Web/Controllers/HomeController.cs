using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Web.Models;

namespace WorkFlowManager.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public ActionResult Index()
        {
            var businessProcessList = _unitOfWork.Repository<BusinessProcess>().GetAll().Where(s => s.RelatedTaskId != null);

            var businessProcessDtoList = new List<BusinessProcesDto>();
            foreach (var businessProcess in businessProcessList)
            {
                businessProcessDtoList.Add(new BusinessProcesDto { BusinessProcessId = businessProcess.Id, BusinessProcessName = businessProcess.Name, TaskId = (int)businessProcess.RelatedTaskId });
            }

            return View(new WorkFlowTestModel { TestList = businessProcessDtoList });
        }

        public ActionResult About()
        {
            ViewBag.Message = "Work Flow Engine Design and Running Examples.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "";

            return View();
        }
    }
}