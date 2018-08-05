using WorkFlowManager.Common.Constants;

namespace WorkFlowManager.Common.Dto
{
    public class FormObject
    {
        public IConstants ConstantObject { get; set; }

        public FormObject(IConstants ConstantObject)
        {
            this.ConstantObject = ConstantObject;
        }
    }
}
