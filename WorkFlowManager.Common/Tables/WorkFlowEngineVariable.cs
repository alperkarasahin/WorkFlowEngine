using System.ComponentModel.DataAnnotations;

namespace WorkFlowManager.Common.Tables
{
    public class WorkFlowEngineVariable
    {
        [Key]
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
