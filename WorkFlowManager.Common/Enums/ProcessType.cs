using System.ComponentModel;

namespace WorkFlowManager.Common.Enums
{
    public enum ProcessType
    {
        [Description("Process")]
        Process = 5,
        [Description("Condition")]
        Condition = 10,
        [Description("Option List")]
        OptionList = 15,
        [Description("Decision Point")]
        DecisionPoint = 20
    }
}
