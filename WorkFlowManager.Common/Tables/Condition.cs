using System.Collections.Generic;
using WorkFlowManager.Common.Enums;

namespace WorkFlowManager.Common.Tables
{
    public class Condition : Process
    {
        public Condition()
        {
        }
        public Condition(Task task, string name, ProjectRole assignedRole, string description = null, FormView formView = null) : base(task, name, assignedRole, description, formView)
        {
            OptionList = new List<ConditionOption>();
            task.AddProcess(this);
        }

        public List<ConditionOption> OptionList { get; set; }

        public void AddOption(ConditionOption optionList)
        {
            OptionList.Add(optionList);
        }

    }
}
