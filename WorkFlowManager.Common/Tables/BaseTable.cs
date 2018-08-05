using System;
using System.Collections.Generic;

namespace WorkFlowManager.Common.Tables
{
    public abstract class BaseTable
    {
        public ICollection<Document> DocumentList { get; set; }

        public int Id { get; set; }

        public ICollection<WorkFlowTrace> WorkFlowTraceList { get; set; }

        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public DateTime? LastlyModifiedTime => UpdatedTime ?? CreatedTime;
    }
}
