using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Services.DbServices
{

    public class WorkFlowDataService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WorkFlowDataService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IEnumerable<ProcessVM> GetWorkFlowProcessList(int taskId)
        {
            return Mapper.Map<IEnumerable<Process>, IEnumerable<ProcessVM>>(_unitOfWork.Repository<Process>().GetList(x => x.TaskId == taskId));
        }

        public IEnumerable<UserProcessViewModel> GetWorkFlowTraceList()
        {
            var islemList = GetWorkFlowTraceIdList();

            foreach (var workFlowTraceId in islemList)
            {
                yield return GetWorkFlowTrace(workFlowTraceId);
            }
        }

        public UserProcessViewModel GetWorkFlowTrace(int workFlowTraceId)
        {
            var workFlowTrace =
                _unitOfWork.Repository<WorkFlowTrace>()
                .Get(x => x.Id == workFlowTraceId,
                        x => x.Process,
                            x => x.ConditionOption,
                                x => x.Process.Task,
                                    x => x.ConditionOption.MonitoringRoleList,
                                        x => x.Process.MonitoringRoleList);

            UserProcessViewModel userProcess = null;
            if (workFlowTrace != null)
            {
                userProcess =
                new UserProcessViewModel
                {
                    Id = workFlowTrace.Id,
                    ProcessStatus = workFlowTrace.ProcessStatus,
                    OwnerId = workFlowTrace.OwnerId,
                    ProcessId = workFlowTrace.ProcessId,
                    ProcessVariableName = (workFlowTrace.Process.GetType() == typeof(Condition) || workFlowTrace.Process.GetType() == typeof(DecisionPoint) ? ((Condition)workFlowTrace.Process).VariableName : null),
                    AssignedRole = workFlowTrace.Process.AssignedRole,
                    Description = workFlowTrace.Description,
                    ProcessName = workFlowTrace.Process.Name,
                    Controller = workFlowTrace.Process.Task.Controller,
                    SpecialFormTemplateView = workFlowTrace.Process.Task.SpecialFormTemplateView,
                    ProcessUniqueCode = workFlowTrace.Process.ProcessUniqueCode,
                    TaskId = workFlowTrace.Process.TaskId,
                    TaskName = workFlowTrace.Process.Task.Name,
                    NextProcessId = workFlowTrace.Process.NextProcessId,
                    IsCondition = (workFlowTrace.Process.GetType() == typeof(Condition)),
                    ProcessDescription = (workFlowTrace.ConditionOption == null ? workFlowTrace.Process.Description : string.Format("{0}({1})", workFlowTrace.Process.Description, workFlowTrace.ConditionOption.Description)),
                    ProcessNotificationMessage = (workFlowTrace.ConditionOption == null ? workFlowTrace.Process.NotificationMessage : string.Format("{0}({1})", workFlowTrace.Process.NotificationMessage, workFlowTrace.ConditionOption.NotificationMessage)),
                    ConditionOptionId = workFlowTrace.ConditionOptionId,
                    UpdatedTime = workFlowTrace.UpdatedTime,
                    CreatedTime = workFlowTrace.CreatedTime,
                    ProcessMonitoringRolList =
                        (workFlowTrace.ConditionOption ?? workFlowTrace.Process)
                        .MonitoringRoleList
                        .Select(t => new ProcessMonitoringRole
                        {
                            ProcessId = t.ProcessId,
                            ProjectRole = t.ProjectRole
                        })
                        .ToList(),
                    IslemBelgeListesi = (workFlowTrace.DocumentList != null ? workFlowTrace.DocumentList.ToList() : null)
                };
            }
            return userProcess;
        }

        private List<int> GetWorkFlowTraceIdList()
        {
            return
                _unitOfWork.Repository<WorkFlowTrace>()
                .GetList()
                .Select(x => x.Id)
                .ToList();
        }

    }
}
