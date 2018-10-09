using WorkFlowManager.Common.Enums;

namespace WorkFlowManager.Common.Tables
{
    public class DecisionPoint : Condition
    {
        public DecisionPoint()
        {

        }
        public DecisionPoint(Task task, string name, DecisionMethod decisionMethod, string variableName = null, int repetitionFrequenceByHour = 1, string description = null, FormView formView = null) : base(task, name, ProjectRole.System, variableName, description, formView)
        {
            DecisionMethod = decisionMethod;
            RepetitionFrequenceByHour = repetitionFrequenceByHour;
            AssignedRole = ProjectRole.System;
            task.AddProcess(this);
        }

        public int DecisionMethodId { get; set; }
        public DecisionMethod DecisionMethod { get; set; }
        public int RepetitionFrequenceByHour { get; set; }

        public int? CancelProcessId { get; set; }
        public Process CancelProcess { get; set; }

        public string CancelProcessText { get; set; }
        public string CancelProcessLabel => (CancelProcessText == null ? "Cancel" : CancelProcessText);

    }
}
