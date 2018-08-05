using System.Collections.Generic;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Common.Dto
{
    public class WorkFlowDTO
    {
        //public string Controller { get; set; }
        //public string SpecialFormTemplateView { get; set; }
        //public int TraceId { get; set; }
        //public int ProcessId { get; set; }
        //public int TaskId { get; set; }
        //public string TaskName { get; set; }
        //public bool IsCondition { get; set; }
        public IEnumerable<WorkFlowTraceVM> ProgressGorevListesi { get; set; }
        public IEnumerable<UserProcessViewModel> GormeyeYetkiliIslemListesi { get; set; }
        public IEnumerable<UserProcessViewModel> GeriGidilebilecekIslemListesi { get; set; }
    }
}
