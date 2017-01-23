using System.Collections.Generic;

namespace WorkFlowManager.Common.Tables
{
    public abstract class BaseTable
    {
        public ICollection<Document> DocumentList { get; set; }

        public int Id { get; set; }
    }
}
