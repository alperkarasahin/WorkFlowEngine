using System.Web.Mvc;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Services.DbServices
{
    public interface IWorkFlow
    {
        void WorkFlowFormSave<TClass, TVM>(WorkFlowFormViewModel satinAlmaOzelFormViewModel) where TClass : class where TVM : WorkFlowFormViewModel;

        bool FormValidate(WorkFlowFormViewModel formData, ModelStateDictionary modelState);


        void FormSave(WorkFlowFormViewModel formData);


        string DecisionPointJobCall(string id, string jobId, string hourInterval);


        bool FullFormValidate(WorkFlowFormViewModel formData, ModelStateDictionary modelState);

    }
}
