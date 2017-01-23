using WorkFlowManager.Common.Enums;

namespace WorkFlowManager.Common.Tables
{
    public class ProcessMonitoringRole
    {
        public int ProcessId { get; set; }
        public Process Process { get; set; }
        public ProjectRole ProjectRole { get; set; }
    }
}
