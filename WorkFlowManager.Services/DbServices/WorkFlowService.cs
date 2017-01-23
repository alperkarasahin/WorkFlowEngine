using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Mappers;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Services.DbServices
{
    public class WorkFlowService
    {
        private readonly IUnitOfWork _unitOfWork;
        public WorkFlowService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<WorkFlow> GetWorkFlowList()
        {
            return _unitOfWork.Repository<WorkFlow>()
                .GetList();
        }

        public IEnumerable<Task> GetTaskList()
        {
            return _unitOfWork.Repository<Task>()
                .GetList(null, x => x.WorkFlow);
        }

        public IEnumerable<Process> GetProcessList(int gorevId)
        {
            return _unitOfWork.Repository<Process>()
                .Find(x => x.TaskId == gorevId);
        }

        private string SentenceForDecisionPoint(string sentence)
        {
            string rslt = "";
            string[] wordList = sentence.Split(' ');
            int i = 0;

            foreach (var word in wordList)
            {

                string wordWithTrim = word.Trim();

                if (wordWithTrim != string.Empty)
                {
                    i++;

                    if (i % 2 == 0)
                    {
                        rslt = string.Format("{0}<br/>{1}", rslt, wordWithTrim);
                    }
                    else
                    {
                        rslt = string.Format("{0} {1}", rslt, wordWithTrim);
                    }
                }
            }
            return rslt.Trim();
        }


        public string GetWorkFlowDiagram(int gorevId)
        {
            var gorev = _unitOfWork.Repository<Task>().Get(gorevId);
            return gorev.WorkFlowDiagram;
        }

        public void SetWorkFlowDiagram(int gorevId)
        {
            string charForPrefixProcess = "(";
            string charForSuffixProcess = ")";

            //return "graph TD;A-->B;A-->C;B-->D;D-->A;";
            string stopDummyProcessCode = "dummy_stopping_point";
            string stopDummyProcessName = "Stop";

            string startDummyProcessCode = "dummy_starting_point";
            string startDummyProcessName = "Start";

            var task = _unitOfWork.Repository<Task>().Get(x => x.Id == gorevId);
            string workFlowDiagram = "";
            StringBuilder resultDiagram = new StringBuilder();

            if (task.StartingProcessId != null)
            {
                var firstProcess = _unitOfWork.Repository<Process>().Get((int)task.StartingProcessId);

                resultDiagram.Append(string.Format(@"{0}((""{1}""))-->{2};", startDummyProcessCode, startDummyProcessName, firstProcess.ProcessUniqueCode));
                resultDiagram.Append(string.Format("style {0} fill:#f96,stroke:#333,stroke-width:2px;;", startDummyProcessCode));
            }

            var processList = _unitOfWork.Repository<Process>()
                .GetList(x => x.TaskId == gorevId, x => x.NextProcess);


            foreach (var islem in processList)
            {
                workFlowDiagram = "";
                if (islem.GetType() == typeof(ConditionOption))
                {
                    Condition conditionForOption = _unitOfWork.Repository<Condition>().Get(((ConditionOption)islem).ConditionId);

                    if (islem.NextProcess != null)
                    {
                        if (islem.NextProcess.GetType() == typeof(Condition) || islem.NextProcess.GetType() == typeof(DecisionPoint))
                        {
                            workFlowDiagram = string.Format(@"{0}{1}{{""{2}""}}-->|""{3}""|{4}{{""{5}""}};", workFlowDiagram, conditionForOption.ProcessUniqueCode, SentenceForDecisionPoint(conditionForOption.Name), islem.Name, islem.NextProcess.ProcessUniqueCode, SentenceForDecisionPoint(islem.NextProcess.Name), charForPrefixProcess, charForSuffixProcess);
                        }
                        else
                        {
                            workFlowDiagram = string.Format(@"{0}{1}{{""{2}""}}-->|""{3}""|{4}{6}""{5}""{7};", workFlowDiagram, conditionForOption.ProcessUniqueCode, SentenceForDecisionPoint(conditionForOption.Name), islem.Name, islem.NextProcess.ProcessUniqueCode, islem.NextProcess.Name, charForPrefixProcess, charForSuffixProcess);
                        }
                    }
                    else
                    {
                        workFlowDiagram = string.Format(@"{0}{1}{{""{2}""}}-->|""{3}""|{4}((""{5}""));", workFlowDiagram, conditionForOption.ProcessUniqueCode, SentenceForDecisionPoint(conditionForOption.Name), islem.Name, stopDummyProcessCode, stopDummyProcessName, charForPrefixProcess, charForSuffixProcess);
                    }
                }
                else if (islem.GetType() == typeof(Condition) || islem.GetType() == typeof(DecisionPoint))
                {
                    //
                }
                else if (islem.GetType() == typeof(Process))
                {
                    if (islem.NextProcess != null)
                    {
                        if (islem.NextProcess.GetType() == typeof(Condition) || islem.NextProcess.GetType() == typeof(DecisionPoint))
                        {
                            workFlowDiagram = string.Format(@"{0}{1}{5}""{2}""{6}-->{3}{{""{4}""}};", workFlowDiagram, islem.ProcessUniqueCode, islem.Name, islem.NextProcess.ProcessUniqueCode, SentenceForDecisionPoint(islem.NextProcess.Name), charForPrefixProcess, charForSuffixProcess);
                        }
                        else
                        {
                            workFlowDiagram = string.Format(@"{0}{1}{5}""{2}""{6}-->{3}{5}""{4}""{6};", workFlowDiagram, islem.ProcessUniqueCode, islem.Name, islem.NextProcess.ProcessUniqueCode, islem.NextProcess.Name, charForPrefixProcess, charForSuffixProcess);
                        }
                    }
                    else
                    {
                        workFlowDiagram = string.Format(@"{0}{1}{5}""{2}""{6}-->{3}((""{4}""));", workFlowDiagram, islem.ProcessUniqueCode, islem.Name, stopDummyProcessCode, stopDummyProcessName, charForPrefixProcess, charForSuffixProcess);
                    }
                }

                if (workFlowDiagram.CompareTo("") != 0)
                {
                    resultDiagram.Append(workFlowDiagram);
                }
            }


            if (stopDummyProcessCode != null)
            {
                workFlowDiagram = string.Format("style {0} fill:#f96,stroke:#333,stroke-width:2px;;", stopDummyProcessCode);
                resultDiagram.Append(workFlowDiagram);
            }

            task.WorkFlowDiagram = string.Format("{0}{1}", "graph TD;", resultDiagram.ToString());
            _unitOfWork.Repository<Task>().Update(task);
            _unitOfWork.Complete();
        }

        public string Delete(int processId)
        {
            Process process = _unitOfWork.Repository<Process>().GetForUpdate(x => x.Id == processId, x => x.MonitoringRoleList, x => x.DocumentList);
            int taskId = process.TaskId;

            if (process.GetType() == typeof(Condition)) //Kosula bağlı seçenek varsa silme yapılamaz
            {
                var optionList = _unitOfWork.Repository<ConditionOption>().GetList(x => x.ConditionId == processId);
                if (optionList.Count() > 0)
                {
                    throw new Exception("Options must be removed first.");
                }
            }
            else if (process.GetType() == typeof(DecisionPoint)) //Karar Noktasi ise önce seçenekleri silinecek
            {
                var optionList = _unitOfWork.Repository<ConditionOption>().GetListForUpdate(x => x.ConditionId == processId).ToList();

                var secenekIdListesi = optionList.Select(t => t.Id).ToList();
                var monitoringRolList = _unitOfWork.Repository<ProcessMonitoringRole>().GetListForUpdate(x => secenekIdListesi.Any(t => t == x.ProcessId)).ToList();
                _unitOfWork.Repository<ProcessMonitoringRole>().RemoveRange(monitoringRolList);
                _unitOfWork.Repository<ConditionOption>().RemoveRange(optionList);
                _unitOfWork.Complete();
            }

            //Belgeler varsa belgeler silinecek
            if (process.DocumentList != null && process.DocumentList.Count() > 0)
            {
                _unitOfWork.Repository<Document>().RemoveRange(process.DocumentList);
            }

            //1)Silinmek istenen nodun öncesi varsa alınacak
            //2)Sonrası varsa alınacak
            //3)Once ki nodun sonrası silinecek nodun sonrası olacak


            // 1
            var processList = _unitOfWork.Repository<Process>().GetList(null, x => x.MonitoringRoleList);
            Process onceki = GetBefore(processId);

            string islemSonuc = null;
            Process sonraki = null;
            if (onceki != null)
            {
                if (process.NextProcessId != null)
                {
                    sonraki = _unitOfWork.Repository<Process>().Get((int)process.NextProcessId);
                }
            }

            if (onceki != null && sonraki != null)
            {
                onceki.NextProcessId = sonraki.Id;
                islemSonuc = string.Format("{0}->{1} link created.", onceki.Name, sonraki.Name);
            }

            //Silme işlemi yapılacak
            var silinenIslemTanim = process.Name;


            //İzleyenler varsa silinecek
            //var izleyenListesi = islemListesi.Single(x => x.Id == islemId).IzleyenRoller;
            if (process.MonitoringRoleList != null && process.MonitoringRoleList.Count() > 0)
            {
                _unitOfWork.Repository<ProcessMonitoringRole>().RemoveRange(process.MonitoringRoleList);
                //_unitOfWork.Complete();
            }

            //Başlangıç görevi olup olmadığı kontrol edilecek
            //Başlangıç görevi ise ilişki kaldırılacak
            Task gorev = _unitOfWork.Repository<Task>().Get(process.TaskId);
            if (gorev.StartingProcessId == process.Id)
            {
                gorev.StartingProcessId = null;
                _unitOfWork.Repository<Task>().Update(gorev);
                _unitOfWork.Complete();
            }

            _unitOfWork.Repository<Process>().Remove(process);
            if (onceki != null)
            {
                _unitOfWork.Repository<Process>().Update(onceki);
            }

            _unitOfWork.Complete();

            //Başlangıç görevi set edilecek
            SetStartingProcess(taskId);
            SetWorkFlowDiagram(taskId);

            islemSonuc = string.Format("{0} removed. {1}", silinenIslemTanim, islemSonuc);
            return islemSonuc;

        }
        public int AddOrUpdate<T>(T gorevIslem) where T : Process
        {

            Mapper.Initialize(cfg => cfg.CreateMap<T, T>());
            Process gorevIslem_2 = null;

            int? gorevIslem_2_SonrakiId_Baslangic = null;


            if (gorevIslem.Id == 0)
            {
                gorevIslem_2 = Mapper.Map<T, T>((T)gorevIslem);


                gorevIslem_2.ProcessUniqueCode = Guid.NewGuid().ToString();
                _unitOfWork.Repository<T>().Add((T)gorevIslem_2);
                _unitOfWork.Complete();
                //Kosul noktasi girilmişse Evet ve Hayır isminde iki seçenek girilecek
                if (gorevIslem.GetType() == typeof(DecisionPoint))
                {
                    var secenekEvet = new ConditionOption
                    {
                        TaskId = gorevIslem_2.TaskId,
                        Name = "Yes",
                        ProcessUniqueCode = Guid.NewGuid().ToString(),
                        ConditionId = gorevIslem_2.Id,
                        AssignedRole = gorevIslem_2.AssignedRole,
                        Value = "Y"
                    };

                    var secenekHayir = new ConditionOption
                    {
                        TaskId = gorevIslem_2.TaskId,
                        Name = "No",
                        ProcessUniqueCode = Guid.NewGuid().ToString(),
                        ConditionId = gorevIslem_2.Id,
                        AssignedRole = gorevIslem_2.AssignedRole,
                        Value = "N"
                    };

                    _unitOfWork.Repository<ConditionOption>().Add(secenekEvet);
                    _unitOfWork.Repository<ConditionOption>().Add(secenekHayir);
                }
                _unitOfWork.Complete();
            }
            else
            {
                gorevIslem_2 = _unitOfWork.Repository<T>().GetForUpdate(x => x.Id == gorevIslem.Id, x => x.MonitoringRoleList, x => x.DocumentList);
                gorevIslem_2_SonrakiId_Baslangic = gorevIslem_2.NextProcessId;

                if (gorevIslem_2.MonitoringRoleList != null)
                {
                    //Yeni listede olmayanlar silinecek
                    var silinecekRoller = gorevIslem_2.MonitoringRoleList.Where(x => !gorevIslem.MonitoringRoleList.Any(t => t.ProjectRole == x.ProjectRole)).ToList();
                    foreach (var izleyenRol in silinecekRoller)
                    {
                        _unitOfWork.Repository<ProcessMonitoringRole>().Remove(izleyenRol);
                    }

                    //Eski listede olanlar eklenmeyecek
                    var eklenmeyecekRoller = gorevIslem.MonitoringRoleList.Where(x => gorevIslem_2.MonitoringRoleList.Any(t => t.ProjectRole == x.ProjectRole)).ToList();
                    foreach (var eklenmeyecekRol in eklenmeyecekRoller)
                    {
                        gorevIslem.MonitoringRoleList.Remove(eklenmeyecekRol);
                        //_unitOfWork.Repository<GorevIslemIzleyenRol>().Remove(izleyenRol);
                    }
                }

                if (gorevIslem_2.DocumentList != null)
                {
                    //Yeni listede olmayanlar silinecek
                    var silinecekBelgeler = gorevIslem_2.DocumentList.Where(x => !gorevIslem.DocumentList.Any(t => t.MediaName == x.MediaName)).ToList();
                    foreach (var silinecekBelge in silinecekBelgeler)
                    {
                        _unitOfWork.Repository<Document>().Remove(silinecekBelge);
                    }

                    //Eski listede olanlar eklenmeyecek
                    var eklenmeyecekBelgeler = gorevIslem.DocumentList.Where(x => gorevIslem_2.DocumentList.Any(t => t.MediaName == x.MediaName)).ToList();
                    foreach (var eklenmeyecekBelge in eklenmeyecekBelgeler)
                    {
                        gorevIslem.DocumentList.Remove(eklenmeyecekBelge);
                        //_unitOfWork.Repository<GorevIslemIzleyenRol>().Remove(izleyenRol);
                    }
                }



                Mapper.Map(gorevIslem, gorevIslem_2);
                _unitOfWork.Repository<T>().Update((T)gorevIslem_2);
            }
            _unitOfWork.Complete();

            if (gorevIslem_2_SonrakiId_Baslangic != null && gorevIslem_2.NextProcessId != null && gorevIslem_2_SonrakiId_Baslangic != gorevIslem_2.NextProcessId)
            {
                SetNextByProcessCode(gorevIslem_2.ProcessUniqueCode, gorevIslem_2.NextProcessId);
            }
            return gorevIslem_2.Id;
        }



        private Process GetBefore(int processId)
        {
            foreach (var islem in _unitOfWork.Repository<Process>().GetAll())
            {
                if (islem.NextProcessId == processId)
                {
                    return islem;
                }
            }
            return null;
        }


        public void SetNextByProcessCode(string processCode, int? nextProcessId)
        {
            var process = _unitOfWork.Repository<Process>().Get(x => x.ProcessUniqueCode == processCode);

            if (nextProcessId != null)
            {

                /*
                    1) 1-->2-->3-->4-->5 (put 2 after 4)

                    2) 1-->3-->4-->2-->5


                    A)4.NextProcessId = 2
                    B)1.NextProcessId = 3
                    c)2.NextProcessId = 5
                */
                Process process_1 = GetBefore(process.Id);
                Process process_5 = _unitOfWork.Repository<Process>().Get((int)nextProcessId);


                Process process_3 = null;
                if (process.NextProcessId != null)
                {
                    _unitOfWork.Repository<Process>().Get((int)process.NextProcessId);
                }
                Process process_4 = null;
                if (process_3 != null && process_3.NextProcessId != null)
                {
                    process_4 = _unitOfWork.Repository<Process>().Get((int)process_3.NextProcessId);
                }
                //A
                if (process_4 != null)
                {
                    process_4.NextProcessId = process.Id;
                    _unitOfWork.Repository<Process>().Update(process_4);
                }
                //B
                if (process_1 != null && process_3 != null)
                {
                    process_1.NextProcessId = process_3.Id;
                    _unitOfWork.Repository<Process>().Update(process_1);
                }

                //C
                if (process_5 != null)
                {
                    process.NextProcessId = process_5.Id;
                }
            }
            else
            {
                process.NextProcessId = null;
            }

            _unitOfWork.Repository<Process>().Update(process);
            _unitOfWork.Complete();


            SetStartingProcess(process.TaskId);
            SetWorkFlowDiagram(process.TaskId);

        }

        public IEnumerable<DecisionMethod> GetDecisionMethodList(int taskId)
        {
            return _unitOfWork.Repository<DecisionMethod>()
                .GetList(x => x.TaskId == taskId);
        }

        public IEnumerable<FormView> GetFormViewList(int taskId)
        {
            return _unitOfWork.Repository<FormView>()
                .GetList(x => x.TaskId == taskId);
        }

        public SummaryOfWorkFlowViewModel GetWorkFlowSummary(int taskId)
        {
            var mainProcessList = GetMainProcessList(taskId);
            var workFlowSummary = new SummaryOfWorkFlowViewModel();
            List<int> formIdList = new List<int>();
            List<int> decisionMethodList = new List<int>();
            foreach (var item in mainProcessList)
            {
                if (item is Condition)
                {
                    if (item is DecisionPoint)
                    {
                        workFlowSummary.NumberOfDecisionPoint++;
                        var kararMetodId = ((DecisionPoint)item).DecisionMethodId;
                        if (!decisionMethodList.Contains(kararMetodId))
                        {
                            decisionMethodList.Add(kararMetodId);
                        }
                    }
                    else
                    {
                        workFlowSummary.NumberOfCondition++;
                    }
                }
                else if (item is Process)
                {
                    if (item.FormViewId != null)
                    {
                        workFlowSummary.NumberOfProcessWhicHasSpecialForm++;
                        if (!formIdList.Contains((int)item.FormViewId))
                        {
                            formIdList.Add((int)item.FormViewId);
                        }
                    }
                    else
                    {
                        workFlowSummary.NumberOfStandartProcess++;
                    }
                }
            }

            workFlowSummary.NumberOfTotalProcess =
                workFlowSummary.NumberOfCondition +
                workFlowSummary.NumberOfDecisionPoint +
                workFlowSummary.NumberOfProcessWhicHasSpecialForm +
                workFlowSummary.NumberOfStandartProcess;


            workFlowSummary.NumberOfWillBeDesignedDecisionMethod = 0;

            foreach (var decisionMethodId in decisionMethodList)
            {
                if (!_unitOfWork.Repository<DecisionMethod>().Get(decisionMethodId).Completed)
                {
                    workFlowSummary.NumberOfWillBeDesignedDecisionMethod++;
                }
            }

            //isAkisOzeti.TasarlanacakKararMetoduSayisi = kullanilanKararMetoduIdleri.Count();

            var specialFormList = _unitOfWork.Repository<Process>().GetList(x => x.TaskId == taskId && formIdList.Any(v => v == x.FormViewId), x => x.FormView);
            foreach (var form in specialFormList)
            {
                if (!form.FormView.Completed)
                {

                    if (form.FormView.FormComplexity == FormComplexity.Easy)
                    {
                        workFlowSummary.NumberOfWillBeDesignedSimpleForm++;
                        workFlowSummary.NumberOfWillBeDesignedSimplePage = workFlowSummary.NumberOfWillBeDesignedSimplePage + form.FormView.NumberOfPage;
                    }
                    else if (form.FormView.FormComplexity == FormComplexity.Middle)
                    {
                        workFlowSummary.NumberOfWillBeDesignedMiddleForm++;
                        workFlowSummary.NumberOfWillBeDesignedMiddlePage = workFlowSummary.NumberOfWillBeDesignedMiddlePage + form.FormView.NumberOfPage;
                    }
                    else if (form.FormView.FormComplexity == FormComplexity.Complex)
                    {
                        workFlowSummary.NumberOfWillBeDesignedComplexForm++;
                        workFlowSummary.NumberOfWillBeDesignedComplexPage = workFlowSummary.NumberOfWillBeDesignedComplexPage + form.FormView.NumberOfPage;
                    }
                }

            }

            workFlowSummary.NumberOfTotalJobWillBeComplete = workFlowSummary.NumberOfWillBeDesignedDecisionMethod +
                (workFlowSummary.NumberOfWillBeDesignedSimpleForm + workFlowSummary.NumberOfWillBeDesignedMiddleForm + workFlowSummary.NumberOfWillBeDesignedComplexForm);


            return workFlowSummary;
        }

        public IEnumerable<Process> GetMainProcessList(int gorevId)
        {
            return _unitOfWork.Repository<Process>().GetList().Where(x =>
            {
                if (x.TaskId != gorevId)
                {
                    return false;
                }
                bool rslt = true;
                if (x.GetType() == typeof(ConditionOption))
                {
                    rslt = false;
                }
                return rslt;
            }).OrderBy(x => x.Name);
        }


        public Process GetProcess(int processId)
        {
            Process process = _unitOfWork.Repository<Process>().Get(x => x.Id == processId, x => x.MonitoringRoleList, x => x.DocumentList);

            if (process.GetType() == typeof(ConditionOption))
            {
                process = _unitOfWork.Repository<ConditionOption>().Get(x => x.Id == processId, x => x.MonitoringRoleList, x => x.Condition);
            }
            return process;
        }

        public Process GetProcess(string processCode)
        {
            Process process = _unitOfWork.Repository<Process>().Get(x => x.ProcessUniqueCode == processCode, x => x.MonitoringRoleList);

            if (process.GetType() == typeof(ConditionOption))
            {
                process = _unitOfWork.Repository<ConditionOption>().Get(x => x.ProcessUniqueCode == processCode, x => x.MonitoringRoleList, x => x.Condition);
            }
            return process;
        }


        private IEnumerable<Process> ProcessListWhichBeforeIsNull(int gorevId)
        {
            return GetMainProcessList(gorevId).Where(x => GetBefore(x.Id) == null);
        }

        private void SetStartingProcess(int gorevId)
        {
            Task task = _unitOfWork.Repository<Task>().Get(gorevId);
            //Tek görev varsa o set edilecek

            var processListWhichBeforeIsNull = ProcessListWhichBeforeIsNull(gorevId);
            if (processListWhichBeforeIsNull.Count() > 0)//Başlangıç işlemi set edilecek, birden fazla geliyorsa değişiklik yapılmayacak
            {
                if (task.StartingProcessId == null || processListWhichBeforeIsNull.Count() == 1)
                {
                    task.StartingProcessId = processListWhichBeforeIsNull.First().Id;
                    _unitOfWork.Repository<Task>().Update(task);
                    _unitOfWork.Complete();
                }

            }

        }

        public void AddOrUpdate(ProcessType processType, ProcessForm formData)
        {
            int processId = 0;
            if (processType == ProcessType.Process)
            {
                processId = AddOrUpdate<Process>(formData.ToProcess<Process>());
            }
            else if (processType == ProcessType.Condition)
            {
                processId = AddOrUpdate<Condition>(formData.ToProcess<Condition>());
            }
            else if (processType == ProcessType.OptionList)
            {
                processId = AddOrUpdate<ConditionOption>(formData.ToProcess<ConditionOption>());
            }
            else if (processType == ProcessType.DecisionPoint)
            {
                processId = AddOrUpdate<DecisionPoint>(formData.ToProcess<DecisionPoint>());
            }

            if (formData.Id == 0 && processId != 0)
            {
                formData.Id = processId;
                //Yeni kayıt eklendi
                //Eklenen işlem yada koşul ise  Görevin başlangıç kaydı boş ise ilk kayıt olarak seçilecek
                if (processType == ProcessType.Process || processType == ProcessType.Condition)
                {
                    SetStartingProcess(formData.TaskId);
                }

            }


            SetWorkFlowDiagram(formData.TaskId);
            //oluşan grafik veritabanına kaydedilecek



        }
    }
}