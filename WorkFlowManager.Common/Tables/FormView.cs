using WorkFlowManager.Common.Enums;

namespace WorkFlowManager.Common.Tables
{
    public class FormView : BaseTable
    {
        public int TaskId { get; set; }
        public Task Task { get; set; }

        public string FormName { get; set; } //Harcama Talimatı Formu
        public string FormDescription { get; set; } //Harcama Talimatı Formu

        public string ViewName { get; set; } //HarcamaTalimatiFormu

        public FormComplexity FormComplexity { get; set; }

        public int NumberOfPage { get; set; }

        public bool Completed { get; set; }
    }
}
