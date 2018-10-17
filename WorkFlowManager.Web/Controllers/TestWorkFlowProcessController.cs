using System.Linq;
using System.Web.Mvc;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Services.DbServices;

namespace WorkFlowManager.Web.Controllers
{
    public class TestWorkFlowProcessController : WorkFlowProcessController
    {
        private TestWorkFlowProcessService _testWorkFlowProcessService;
        private readonly IUnitOfWork _unitOfWork;
        public TestWorkFlowProcessController(
            IUnitOfWork unitOfWork, TestWorkFlowProcessService testWorkFlowService) : base(testWorkFlowService)
        {
            _unitOfWork = unitOfWork;
            _testWorkFlowProcessService = testWorkFlowService;
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



    }
}