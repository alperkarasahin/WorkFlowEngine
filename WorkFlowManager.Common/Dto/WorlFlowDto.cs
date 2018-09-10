using System.Collections.Generic;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Common.Dto
{
    public class WorkFlowDTO
    {
        public IEnumerable<WorkFlowTraceVM> ProgressProcessList { get; set; }
        public IEnumerable<UserProcessViewModel> AuthorizedProcessList { get; set; }
        public IEnumerable<UserProcessViewModel> TargetProcessListForCancel { get; set; }
    }
}
