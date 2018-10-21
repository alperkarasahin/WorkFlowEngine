using System.Collections.Generic;
using WorkFlowManager.Common.Enums;

namespace WorkFlowManager.Common.Tables
{
    public class WorkFlowTrace : BaseTable
    {
        public int OwnerId { get; set; }
        public BaseTable Owner { get; set; }
        public int ProcessId { get; set; }
        public Process Process { get; set; }
        public int? ConditionOptionId { get; set; }
        public ConditionOption ConditionOption { get; set; }
        public string Description { get; set; }
        public ProcessStatus ProcessStatus { get; set; }
        public string JobId { get; set; }

        public bool IsCondition => Process.GetType() == typeof(Condition);

        public List<BusinessProcess> SubProcessList { get; set; }
    }
}
