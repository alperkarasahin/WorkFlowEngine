using System.Web.Mvc;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Services.DbServices
{
    public interface IWorkFlow
    {
        WorkFlowFormViewModel SatinAlmaOzelForm<T>(int ownerId, WorkFlowTraceForm satinAlmaIslemForm) where T : WorkFlowFormViewModel;

        void WorkFlowFormKaydet<TClass, TVM>(WorkFlowFormViewModel satinAlmaOzelFormViewModel) where TClass : class where TVM : WorkFlowFormViewModel;

        bool OzelFormValidate(WorkFlowFormViewModel formData, ModelStateDictionary modelState);


        void OzelFormKaydet(WorkFlowFormViewModel formData);


        string KararNoktasiSurecKontrolJobCall(string id, string jobId, string hourInterval);


        bool FullFormValidate(WorkFlowFormViewModel formData, ModelStateDictionary modelState);


        UserProcessViewModel KullaniciSonIslemVM(int ownerId);

    }
}
