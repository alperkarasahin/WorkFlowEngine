using System.Collections.Generic;

namespace WorkFlowManager.Common.Tables
{
    public class Task : BaseTable
    {
        public int WorkFlowId { get; set; }
        public WorkFlow WorkFlow { get; set; }
        public string Name { get; set; }

        public int? StartingProcessId { get; set; }
        public Process StartingProcess { get; set; }

        public List<Process> ProcessList { get; set; }

        public List<DecisionMethod> DecisionMethodList { get; set; }
        public List<FormView> FormViewList { get; set; }

        public string WorkFlowDiagram { get; set; }

        public string MethodServiceName { get; set; }
    }
}
