
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Common.Dto
{
    public class ServiceDTO
    {

        public ServiceDTO(WorkFlowTraceForm workFlowTraceForm)
        {
            this.workFlowTraceForm = workFlowTraceForm;
        }

        public WorkFlowTraceForm workFlowTraceForm { get; set; }

    }
}
