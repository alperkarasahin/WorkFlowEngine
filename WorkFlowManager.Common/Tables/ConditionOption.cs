using WorkFlowManager.Common.Enums;

namespace WorkFlowManager.Common.Tables
{
    public class ConditionOption : Process
    {
        public ConditionOption()
        {

        }
        public ConditionOption(Task task, string name, ProjectRole assignedRole, Condition condition, string value = null) : base(task, name, assignedRole)
        {
            Condition = condition;
            Value = value;
            condition.AddOption(this);
        }
        public int ConditionId { get; set; }
        public Condition Condition { get; set; }
        public string Value { get; set; }

    }
}
