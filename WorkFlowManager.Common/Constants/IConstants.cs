using System;
using System.Web.Mvc;
using WorkFlowManager.Common.Dto;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Common.Constants
{
    public interface IConstants
    {
        void Kaydet(WorkFlowFormViewModel formData);
        WorkFlowFormViewModel Yukle(ServiceDTO serviceDto);
        Type ViewModelType();
        void BelgeTuruAyarla(ServiceDTO serviceDto);

        bool Validate(WorkFlowFormViewModel formData, ModelStateDictionary modelState);
    }
}
