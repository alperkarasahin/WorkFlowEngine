using System.Collections.Generic;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Common.Dto
{
    public class WorkFlowDTO
    {
        public IEnumerable<WorkFlowTraceVM> ProgressGorevListesi { get; set; }
        public IEnumerable<UserProcessViewModel> GormeyeYetkiliIslemListesi { get; set; }
        public IEnumerable<UserProcessViewModel> GeriGidilebilecekIslemListesi { get; set; }
    }
}
