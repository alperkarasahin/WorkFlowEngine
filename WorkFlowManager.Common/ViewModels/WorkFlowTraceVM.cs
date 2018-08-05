using WorkFlowManager.Common.Enums;

namespace WorkFlowManager.Common.ViewModels
{
    public class WorkFlowTraceVM
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public ProjectRole AssignedRole { get; set; }
        public int? ConditionOptionId { get; set; }
        public string Description { get; set; }
        public ProcessStatus? ProcessStatus { get; set; }

    }
}
