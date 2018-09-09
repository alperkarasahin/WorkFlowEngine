using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using WorkFlowManager.Common.DataAccess._UnitOfWork;

namespace WorkFlowManager.Services.DbServices
{

    public static class DynamicMethodCallService
    {
        public static string Caller(
            IUnitOfWork unitOfWork,
                        string methodCallString,
                            WorkFlowDataService workFlowDataService)
        {

            string serviceAsmPath = ConfigurationManager.AppSettings["ServiceAsmPath"].Trim();
            string serviceBaseDir = ConfigurationManager.AppSettings["ServiceBaseDir"].Trim();


            var splitServiceMethod = methodCallString.Split('.');
            string methodPart = splitServiceMethod[splitServiceMethod.Length - 1];
            string methodName = methodPart.Substring(0, methodPart.LastIndexOf("("));
            string serviceName = methodCallString.Replace("." + methodPart, "");



            object[] parameters = methodPart.Substring(
                                    methodPart.LastIndexOf("(") + 1,
                                        (methodPart.LastIndexOf(")") - methodPart.LastIndexOf("(") - 1))
                                            .Split(',')
                                                .Select(p => p.Trim())
                                                    .ToArray<object>();

            var asmClass = string.Format("{0}{1}.{2}", serviceAsmPath, (string.IsNullOrEmpty(serviceBaseDir) ? "" : "." + serviceBaseDir), serviceName);

            Assembly asm = Assembly.Load(serviceAsmPath);
            Type type = asm.GetType(asmClass);

            Object obj = Activator.CreateInstance(type, unitOfWork, workFlowDataService);
            MethodInfo methodInfo = type.GetMethod(methodName);

            string result;
            if (parameters.Count() == 1 && string.IsNullOrWhiteSpace(parameters[0].ToString()))
            {
                result = methodInfo.Invoke(obj, null).ToString();
            }
            else
            {
                result = methodInfo.Invoke(obj, parameters).ToString();
            }
            return result;
        }
    }
}
