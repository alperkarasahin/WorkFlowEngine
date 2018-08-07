using System;
using System.Reflection;
using System.Web.Mvc;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Web.Infra
{
    public class WorkFlowFormModelBinder : DefaultModelBinder
    {
        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
            var typeValue = bindingContext.ValueProvider.GetValue("WorkFlowFormType");

            var asmClass = (string)typeValue.ConvertTo(typeof(string));


            var serviceAsmPath = "WorkFlowManager.Common";

            Assembly asm = Assembly.Load(serviceAsmPath);
            Type type = asm.GetType(asmClass);

            if (!typeof(WorkFlowFormViewModel).IsAssignableFrom(type))
            {
                throw new InvalidOperationException("Bad Type");
            }
            var model = Activator.CreateInstance(type);
            bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, type);
            return model;
        }

    }

}
