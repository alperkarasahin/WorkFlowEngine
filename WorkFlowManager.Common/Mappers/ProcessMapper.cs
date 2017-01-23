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
            Mapper.Initialize(cfg =>
                cfg.CreateMap<ProcessForm, T>()
                    .ForMember(a => ((T)a).MonitoringRoleList,
                        opt => opt.MapFrom(c => c.MonitoringRoleList.Where(x => x.IsChecked == true).Select(t => new ProcessMonitoringRole { ProcessId = c.Id, ProjectRole = t.ProjectRole }))));

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


            Mapper.Initialize(cfg =>
                cfg.CreateMap<Process, ProcessForm>()
                    .ForMember(a => a.MonitoringRoleList, opt => opt.MapFrom(c => c.MonitoringRoleList.Select(t => new MonitoringRoleCheckbox { IsChecked = true, ProjectRole = t.ProjectRole })))
                    .ForMember(a => a.ConditionId, opt => opt.MapFrom(c => (c as ConditionOption).ConditionId))
                    .ForMember(a => a.ConditionName, opt => opt.MapFrom(c => (c as ConditionOption).Condition.Name))
                    .ForMember(a => a.DecisionMethodId, opt => opt.MapFrom(c => (c as DecisionPoint).DecisionMethodId))
                    .ForMember(a => a.RepetitionFrequenceByHour, opt => opt.MapFrom(c => (c as DecisionPoint).RepetitionFrequenceByHour))
                    .ForMember(a => a.Value, opt => opt.MapFrom(c => (c as ConditionOption).Value))
                );

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
