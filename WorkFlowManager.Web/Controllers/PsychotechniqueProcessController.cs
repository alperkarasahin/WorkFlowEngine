using System.Linq;
using System.Web.Mvc;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Services.DbServices;

namespace WorkFlowManager.Web.Controllers
{
    public class PsychotechniqueProcessController : WorkFlowProcessController
    {
        private WorkFlowProcessService _workFlowProcessService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly WorkFlowService _workFlowService;
        public PsychotechniqueProcessController(IUnitOfWork unitOfWork, WorkFlowProcessService workFlowProcessService, WorkFlowService workFlowService) : base(workFlowProcessService)
        {
            _unitOfWork = unitOfWork;
            _workFlowProcessService = workFlowProcessService;
            _workFlowService = workFlowService;
        }

    }
}