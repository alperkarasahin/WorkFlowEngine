using System.Web.Mvc;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Common.Constants
{
    public interface IWorkFlowForm
    {
        void Save(WorkFlowFormViewModel formData);
        bool Validate(WorkFlowFormViewModel formData, ModelStateDictionary modelState);
        WorkFlowFormViewModel Load(WorkFlowFormViewModel workFlowFormViewModel);
    }
}
