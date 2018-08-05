namespace WorkFlowManager.Common.Tables
{
    public class DecisionMethod : BaseTable
    {
        public int TaskId { get; set; }
        public Task Task { get; set; }
        public string MethodFunction { get; set; }
        public string MethodName { get; set; }

        public string MethodDescription { get; set; }

        public bool Completed { get; set; }
    }
}
