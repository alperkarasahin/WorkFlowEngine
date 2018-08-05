namespace WorkFlowManager.Common.Tables
{
    public class DecisionPoint : Condition
    {
        public int DecisionMethodId { get; set; }
        public DecisionMethod DecisionMethod { get; set; }
        public int RepetitionFrequenceByHour { get; set; }

        public int? CancelProcessId { get; set; }
        public Process CancelProcess { get; set; }

        public string CancelProcessText { get; set; }
        public string CancelProcessLabel => (CancelProcessText == null ? "Cancel" : CancelProcessText);
    }
}
