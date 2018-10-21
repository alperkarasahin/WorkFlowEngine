using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Tables;

namespace WorkFlowManager.Common.Constants
{
    public static class WorkFlowUtil
    {
        private static string SentenceForDecisionPoint(bool systemAction, string sentence)
        {
            string rslt = "";
            string prefix = systemAction ? "(system)" : "(user)";
            sentence = prefix + " " + sentence;
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


        private static string SubProcessDiagram(string processUniqueId, string name)
        {
            return string.Format("subgraph {1};{0}-b3-->{0}-subProcessEndPoint;{0}( ) --> {0}-b2( );{0} --> {0}-b3( );{0}-b2( )-->{0}-subProcessEndPoint( );end;", processUniqueId, name);
        }

        public static void SetWorkFlowDiagram(IUnitOfWork _unitOfWork, int taskId)
        {
            string charForPrefixProcess = "(";
            string charForSuffixProcess = ")";

            string stopDummyProcessCode = "dummy_stopping_point";
            string stopDummyProcessName = "Stop";

            string startDummyProcessCode = "dummy_starting_point";
            string startDummyProcessName = "Start";

            var subProcessList = new Dictionary<string, string>();
            var task = _unitOfWork.Repository<Task>().Get(x => x.Id == taskId);
            string workFlowDiagram = "";
            StringBuilder resultDiagram = new StringBuilder();

            if (task.StartingProcessId != null)
            {
                var firstProcess = _unitOfWork.Repository<Process>().Get((int)task.StartingProcessId);

                resultDiagram.Append(string.Format(@"{0}((""{1}""))-->{2};", startDummyProcessCode, startDummyProcessName, firstProcess.ProcessUniqueCode));
                resultDiagram.Append(string.Format("style {0} fill:#f96,stroke:#333,stroke-width:2px;;", startDummyProcessCode));
            }

            var processList = _unitOfWork.Repository<Process>()
                .GetList(x => x.TaskId == taskId, x => x.NextProcess);


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
                            workFlowDiagram = string.Format(@"{0}{1}{{""{2}""}}-->|""{3}""|{4}{{""{5}""}};", workFlowDiagram, conditionForOption.ProcessUniqueCode, SentenceForDecisionPoint(conditionForOption.GetType() == typeof(DecisionPoint), conditionForOption.Name), islem.Name, islem.NextProcess.ProcessUniqueCode, SentenceForDecisionPoint(islem.NextProcess.GetType() == typeof(DecisionPoint), islem.NextProcess.Name), charForPrefixProcess, charForSuffixProcess);
                        }
                        else
                        {
                            if (islem.NextProcess.GetType() == typeof(SubProcess))
                            {
                                if (!subProcessList.TryGetValue(islem.NextProcess.ProcessUniqueCode, out _))
                                {
                                    subProcessList.Add(islem.NextProcess.ProcessUniqueCode, islem.NextProcess.Name);
                                }
                                var subProcessStartPoint = string.Format("{0}", islem.NextProcess.ProcessUniqueCode);
                                workFlowDiagram = string.Format(@"{0}{1}{{""{2}""}}-->|""{3}""|{4}{6}""{5}""{7};", workFlowDiagram, conditionForOption.ProcessUniqueCode, SentenceForDecisionPoint(conditionForOption.GetType() == typeof(DecisionPoint), conditionForOption.Name), islem.Name, subProcessStartPoint, islem.NextProcess.Name, charForPrefixProcess, charForSuffixProcess);
                            }
                            else
                            {
                                workFlowDiagram = string.Format(@"{0}{1}{{""{2}""}}-->|""{3}""|{4}{6}""{5}""{7};", workFlowDiagram, conditionForOption.ProcessUniqueCode, SentenceForDecisionPoint(conditionForOption.GetType() == typeof(DecisionPoint), conditionForOption.Name), islem.Name, islem.NextProcess.ProcessUniqueCode, islem.NextProcess.Name, charForPrefixProcess, charForSuffixProcess);
                            }
                        }
                    }
                    else
                    {
                        workFlowDiagram = string.Format(@"{0}{1}{{""{2}""}}-->|""{3}""|{4}((""{5}""));", workFlowDiagram, conditionForOption.ProcessUniqueCode, SentenceForDecisionPoint(conditionForOption.GetType() == typeof(DecisionPoint), conditionForOption.Name), islem.Name, stopDummyProcessCode, stopDummyProcessName, charForPrefixProcess, charForSuffixProcess);
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
                            workFlowDiagram = string.Format(@"{0}{1}{5}""{2}""{6}-->{3}{{""{4}""}};", workFlowDiagram, islem.ProcessUniqueCode, islem.Name, islem.NextProcess.ProcessUniqueCode, SentenceForDecisionPoint(islem.NextProcess.GetType() == typeof(DecisionPoint), islem.NextProcess.Name), charForPrefixProcess, charForSuffixProcess);
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
                else if (islem.GetType() == typeof(SubProcess))
                {
                    if (!subProcessList.TryGetValue(islem.ProcessUniqueCode, out _))
                    {
                        subProcessList.Add(islem.ProcessUniqueCode, islem.Name);
                    }
                    var subProcessEndPoint = string.Format("{0}-subProcessEndPoint", islem.ProcessUniqueCode);

                    if (islem.NextProcess != null)
                    {
                        if (islem.NextProcess.GetType() == typeof(Condition) || islem.NextProcess.GetType() == typeof(DecisionPoint))
                        {
                            workFlowDiagram = string.Format(@"{0}{1}{5}""{2}""{6}-->{3}{{""{4}""}};", workFlowDiagram, subProcessEndPoint, islem.Name, islem.NextProcess.ProcessUniqueCode, SentenceForDecisionPoint(islem.NextProcess.GetType() == typeof(DecisionPoint), islem.NextProcess.Name), charForPrefixProcess, charForSuffixProcess);
                        }
                        else
                        {
                            workFlowDiagram = string.Format(@"{0}{1}{5}""{2}""{6}-->{3}{5}""{4}""{6};", workFlowDiagram, subProcessEndPoint, islem.Name, islem.NextProcess.ProcessUniqueCode, islem.NextProcess.Name, charForPrefixProcess, charForSuffixProcess);
                        }
                    }
                    else
                    {
                        workFlowDiagram = string.Format(@"{0}{1}{5}""{2}""{6}-->{3}((""{4}""));", workFlowDiagram, subProcessEndPoint, islem.Name, stopDummyProcessCode, stopDummyProcessName, charForPrefixProcess, charForSuffixProcess);
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
            if (subProcessList.Count() > 0)
            {

                foreach (var subProcess in subProcessList)
                {
                    resultDiagram.Append(SubProcessDiagram(subProcess.Key, subProcess.Value));
                }
            }

            task.WorkFlowDiagram = string.Format("{0}{1}", "graph TD;", resultDiagram.ToString());
            _unitOfWork.Repository<Task>().Update(task);
            _unitOfWork.Complete();
        }


    }
}
