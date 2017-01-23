namespace WorkFlowManager.Common.Tables
{
    public class ConditionOption : Process
    {
        public int ConditionId { get; set; }
        public Condition Condition { get; set; }
        public string Value { get; set; }
    }
}
