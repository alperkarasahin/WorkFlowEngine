using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Tables;

namespace WorkFlowManager.Common.Factory
{
    public static class ProcessFactory
    {
        public static Process CreateProcess(Task task, string name, ProjectRole assignedRole, string description = null, FormView formView = null)
        {
            return new Process(task, name, assignedRole, description, formView);
        }

        public static Condition CreateCondition(Task task, string name, ProjectRole assignedRole, string description = null, FormView formView = null)
        {
            return new Condition(task, name, assignedRole, description, formView);
        }
        public static DecisionPoint CreateDecisionPoint(Task task, string name, DecisionMethod decisionMethod, int repetitionFrequenceByHour = 1, string description = null, FormView formView = null)
        {
            return new DecisionPoint(task, name, decisionMethod, repetitionFrequenceByHour, description, formView);
        }


        public static ConditionOption CreateConditionOption(string name, ProjectRole assignedRole, Condition condition)
        {
            return new ConditionOption(condition.Task, name, assignedRole, condition);
        }

        public static ConditionOption CreateDecisionPointYesOption(string name, DecisionPoint decisionPoint)
        {
            return new ConditionOption(decisionPoint.Task, name, ProjectRole.Sistem, decisionPoint, "Y");
        }
        public static ConditionOption CreateDecisionPointNoOption(string name, DecisionPoint decisionPoint)
        {
            return new ConditionOption(decisionPoint.Task, name, ProjectRole.Sistem, decisionPoint, "N");
        }




    }
}
