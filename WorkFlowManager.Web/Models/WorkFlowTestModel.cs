using System.Collections.Generic;

namespace WorkFlowManager.Web.Models
{
    public class BusinessProcesDto
    {
        public int BusinessProcessId { get; set; }
        public string BusinessProcessName { get; set; }
        public int TaskId { get; set; }
    }

    public class WorkFlowTestModel
    {
        public List<BusinessProcesDto> TestList { get; set; }
    }
}