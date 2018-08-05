using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Extensions;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Common.Mappers
{
    public static class ProcessMapper
    {
        public static T ToProcess<T>(this ProcessForm formData) where T : Process
        {
            Process dto = Mapper.Map<ProcessForm, T>(formData);


            //Belgeler şablon ve analiz olarak ikiye ayrılmıştı. Birleştirilecek.

            if (formData.AnalysisFileList != null && formData.TemplateFileList != null)
            {
                dto.DocumentList = formData.AnalysisFileList.FileList.Concat(formData.TemplateFileList.FileList).ToList();
            }
            return (T)dto;
        }

        public static ProcessForm ToProcessForm(this Process process, IEnumerable<Process> mainProcessList, IEnumerable<DecisionMethod> decisionMethodList, IEnumerable<FormView> formViewList)
        {

            var roleList = ProjectRole.Admin.GetAllValues().Where(x => x != ProjectRole.Sistem).ToList();

            ProcessForm dto = Mapper.Map<Process, ProcessForm>(process);
            foreach (var rol in roleList)
            {
                if (!dto.MonitoringRoleList.Any(x => x.ProjectRole == rol))
                {
                    dto.MonitoringRoleList.Add(new MonitoringRoleCheckbox { IsChecked = false, ProjectRole = rol });
                }
            }

            if (process.GetType() == typeof(ConditionOption))
            {
                dto.ProcessType = ProcessType.OptionList;
            }
            else if (process.GetType() == typeof(DecisionPoint))
            {
                dto.ProcessType = ProcessType.DecisionPoint;
                dto.DecisionMethodList = new SelectList(decisionMethodList, "Id", "MethodName");
                dto.RepetitionHourList = new SelectList(Enumerable.Range(1, 24));
            }
            else if (process.GetType() == typeof(Condition))
            {
                dto.ProcessType = ProcessType.Condition;
            }
            else
            {
                dto.ProcessType = ProcessType.Process;
                dto.FormViewList = new SelectList(formViewList, "Id", "FormName");
            }

            dto.MainProcessList = (mainProcessList != null ? new SelectList(mainProcessList, "Id", "Name") : null);
            dto.TemplateFileList = new FileUpload(process.DocumentList, FileType.ProcessTemplateFile, process.Id);

            dto.AnalysisFileList = new FileUpload(process.DocumentList, FileType.AnalysisFile, process.Id);

            return dto;

        }

    }
}
