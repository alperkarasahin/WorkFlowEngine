namespace WorkFlowManager.Common.Tables
{
    public class HealthInformationForm : BaseTable
    {
        public int OwnerId { get; set; }
        public BaseTable Owner { get; set; }
        public string Name { get; set; }
    }
}
