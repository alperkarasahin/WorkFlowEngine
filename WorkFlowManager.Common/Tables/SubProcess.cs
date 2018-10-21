using System;
using System.Collections.Generic;
using WorkFlowManager.Common.Enums;

namespace WorkFlowManager.Common.Tables
{
    public class SubProcess : Process
    {
        public SubProcess(Task task, string name, string taskVariableList)
        {
            Task = task;
            Name = name;
            AssignedRole = ProjectRole.System;
            ProcessUniqueCode = Guid.NewGuid().ToString();
            DocumentList = new HashSet<Document>();
            TaskVariableList = taskVariableList;
            task.AddProcess(this);
        }
        public SubProcess()
        {

        }
        public string TaskVariableList { get; set; }
    }
}
