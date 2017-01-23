namespace WorkFlowManager.Common.Tables
{
    public class DecisionPoint : Condition
    {
        public int DecisionMethodId { get; set; }
        public DecisionMethod DecisionMethod { get; set; }
        public int RepetitionFrequenceByHour { get; set; }
    }
}
