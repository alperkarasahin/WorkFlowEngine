namespace WorkFlowManager.Common.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Text;
    using WorkFlowManager.Common.DataAccess._Context;
    using WorkFlowManager.Common.DataAccess._UnitOfWork;
    using WorkFlowManager.Common.Tables;

    internal sealed class Configuration : DbMigrationsConfiguration<DataContext>
    {


        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
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


        public void SetWorkFlowDiagram(IUnitOfWork _unitOfWork, int gorevId)
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


        protected override void Seed(DataContext context)
        {

            IUnitOfWork _unitOfWork = new UnitOfWork(context);


            var masterTest1 = _unitOfWork.Repository<MasterTest>().Get(x => x.Name == "Task1 Test");
            if (masterTest1 == null)
            {
                masterTest1 = new MasterTest()
                {
                    Name = "Task1 Test"
                };
                _unitOfWork.Repository<MasterTest>().Add(masterTest1);
                _unitOfWork.Complete();
            }

            var workFlow = _unitOfWork.Repository<WorkFlow>().Get(x => x.Name == "WorkFlow1");
            if (workFlow == null)
            {
                workFlow = new WorkFlow { Name = "WorkFlow1" };
                _unitOfWork.Repository<WorkFlow>().Add(workFlow);
                _unitOfWork.Complete();
            };


            #region TASK1
            var task = _unitOfWork.Repository<Task>().Get(x => x.Name == "Task1");

            if (task == null)
            {
                task = new Task { Name = "Task1", WorkFlowId = workFlow.Id, MethodServiceName = "TestWorkFlowProcessService", SpecialFormTemplateView = "OzelFormSablon", Controller = "TestWorkFlowProcess" };
                _unitOfWork.Repository<Task>().Add(task);
                _unitOfWork.Complete();
            }


            var testForm = _unitOfWork.Repository<FormView>().Get(x => x.TaskId == task.Id && x.FormName == "Test Form");
            if (testForm == null)
            {
                testForm = new FormView() { FormName = "Test Form", ViewName = "TestForm", TaskId = task.Id, Completed = true };
                _unitOfWork.Repository<FormView>().Add(testForm);
                _unitOfWork.Complete();
            }

            var isAgeLessThan20 = _unitOfWork.Repository<DecisionMethod>().Get(x => x.TaskId == task.Id && x.MethodName == "Is Age Less Than 20");
            if (isAgeLessThan20 == null)
            {
                isAgeLessThan20 = new DecisionMethod() { MethodName = "Is Age Less Than 20", MethodFunction = "IsAgeLessThan20(Id)", TaskId = task.Id };
                _unitOfWork.Repository<DecisionMethod>().Add(isAgeLessThan20);
                _unitOfWork.Complete();
            }

            var isAgeGreaterThan20 = _unitOfWork.Repository<DecisionMethod>().Get(x => x.TaskId == task.Id && x.MethodName == "Is Age Greater Than 20");
            if (isAgeGreaterThan20 == null)
            {
                isAgeGreaterThan20 = new DecisionMethod() { MethodName = "Is Age Greater Than 20", MethodFunction = "IsAgeGreaterThan20(Id)", TaskId = task.Id };
                _unitOfWork.Repository<DecisionMethod>().Add(isAgeGreaterThan20);
                _unitOfWork.Complete();
            }



            var process = _unitOfWork.Repository<Process>().Get(x => x.Name == "Process 1" && x.TaskId == task.Id);
            if (process == null)
            {
                process = new Process();
                process.AssignedRole = Enums.ProjectRole.Admin;
                process.ProcessUniqueCode = Guid.NewGuid().ToString();
                process.Name = "Process 1";
                process.TaskId = task.Id;
                _unitOfWork.Repository<Process>().Add(process);

                task.StartingProcess = process;
                _unitOfWork.Repository<Task>().Update(task);
                _unitOfWork.Complete();
            }


            var conditon = _unitOfWork.Repository<Condition>().Get(x => x.Name == "Condition 1" && x.TaskId == task.Id);
            ConditionOption option1 = _unitOfWork.Repository<ConditionOption>().Get(x => x.Name == "Option 1" && x.TaskId == task.Id);
            ConditionOption option2 = _unitOfWork.Repository<ConditionOption>().Get(x => x.Name == "Option 2" && x.TaskId == task.Id);
            ConditionOption option3 = _unitOfWork.Repository<ConditionOption>().Get(x => x.Name == "Age Less Than 20" && x.TaskId == task.Id);
            ConditionOption option4 = _unitOfWork.Repository<ConditionOption>().Get(x => x.Name == "Age Greater Than 20" && x.TaskId == task.Id);



            if (conditon == null)
            {
                conditon = new Condition();
                conditon.AssignedRole = Enums.ProjectRole.Admin;
                conditon.ProcessUniqueCode = Guid.NewGuid().ToString();
                conditon.Name = "Condition 1";
                conditon.TaskId = task.Id;
                _unitOfWork.Repository<Condition>().Add(conditon);
            }

            if (option1 == null)
            {
                option1 = new ConditionOption
                {
                    TaskId = task.Id,
                    Name = "Option 1",
                    ProcessUniqueCode = Guid.NewGuid().ToString(),
                    Condition = conditon,
                    AssignedRole = Enums.ProjectRole.Admin
                };
                _unitOfWork.Repository<ConditionOption>().Add(option1);
            }


            if (option2 == null)
            {
                option2 = new ConditionOption
                {
                    TaskId = task.Id,
                    Name = "Option 2",
                    ProcessUniqueCode = Guid.NewGuid().ToString(),
                    Condition = conditon,
                    AssignedRole = Enums.ProjectRole.Admin
                };
                _unitOfWork.Repository<ConditionOption>().Add(option2);
            }
            process.NextProcess = conditon;
            _unitOfWork.Complete();


            var process2 = _unitOfWork.Repository<Process>().Get(x => x.Name == "Process 2" && x.TaskId == task.Id);
            if (process2 == null)
            {
                process2 = new Process();
                process2.AssignedRole = Enums.ProjectRole.Admin;
                process2.ProcessUniqueCode = Guid.NewGuid().ToString();
                process2.FormView = testForm;
                process2.Name = "Process 2";
                process2.TaskId = task.Id;
                _unitOfWork.Repository<Process>().Add(process2);

                option1.NextProcess = process2;
                _unitOfWork.Repository<ConditionOption>().Update(option1);

                _unitOfWork.Complete();
            }


            var ifAgeLessThan20 = _unitOfWork.Repository<DecisionPoint>().Get(x => x.Name == "If Age Less Than 20" && x.TaskId == task.Id);
            if (ifAgeLessThan20 == null)
            {
                ifAgeLessThan20 = new DecisionPoint();
                ifAgeLessThan20.AssignedRole = Enums.ProjectRole.Sistem;
                ifAgeLessThan20.ProcessUniqueCode = Guid.NewGuid().ToString();
                ifAgeLessThan20.Name = "If Age Less Than 20";
                ifAgeLessThan20.TaskId = task.Id;
                ifAgeLessThan20.DecisionMethod = isAgeLessThan20;

                _unitOfWork.Repository<DecisionPoint>().Add(ifAgeLessThan20);
                _unitOfWork.Complete();
            }

            if (option3 == null)
            {
                option3 = new ConditionOption
                {
                    TaskId = task.Id,
                    Name = "Age Less Than 20",
                    ProcessUniqueCode = Guid.NewGuid().ToString(),
                    Condition = ifAgeLessThan20,
                    AssignedRole = Enums.ProjectRole.Sistem,
                    Value = "Y"
                };
                _unitOfWork.Repository<ConditionOption>().Add(option3);
                _unitOfWork.Complete();
            }
            if (option4 == null)
            {
                option4 = new ConditionOption
                {
                    TaskId = task.Id,
                    Name = "Age Greater Than 20",
                    ProcessUniqueCode = Guid.NewGuid().ToString(),
                    Condition = ifAgeLessThan20,
                    AssignedRole = Enums.ProjectRole.Sistem,
                    Value = "N"

                };
                _unitOfWork.Repository<ConditionOption>().Add(option4);
                _unitOfWork.Complete();
            }


            process2.NextProcess = ifAgeLessThan20;
            _unitOfWork.Complete();

            var ageLessThan20 = _unitOfWork.Repository<Process>().Get(x => x.Name == "Age Less Than 20 Selected" && x.TaskId == task.Id);
            if (ageLessThan20 == null)
            {
                ageLessThan20 = new Process();
                ageLessThan20.AssignedRole = Enums.ProjectRole.Admin;
                ageLessThan20.ProcessUniqueCode = Guid.NewGuid().ToString();
                ageLessThan20.Name = "Age Less Than 20 Selected";
                ageLessThan20.TaskId = task.Id;
                _unitOfWork.Repository<Process>().Add(ageLessThan20);

                option3.NextProcess = ageLessThan20;
                _unitOfWork.Repository<ConditionOption>().Update(option3);

                _unitOfWork.Complete();
            }

            var ageGreaterThan20 = _unitOfWork.Repository<Process>().Get(x => x.Name == "Age Greater Than 20 Selected" && x.TaskId == task.Id);
            if (ageGreaterThan20 == null)
            {
                ageGreaterThan20 = new Process();
                ageGreaterThan20.AssignedRole = Enums.ProjectRole.Admin;
                ageGreaterThan20.ProcessUniqueCode = Guid.NewGuid().ToString();
                ageGreaterThan20.Name = "Age Greater Than 20 Selected";
                ageGreaterThan20.TaskId = task.Id;
                _unitOfWork.Repository<Process>().Add(ageGreaterThan20);

                option4.NextProcess = ageGreaterThan20;
                _unitOfWork.Repository<ConditionOption>().Update(option4);

                _unitOfWork.Complete();
            }




            ConditionOption option5 = _unitOfWork.Repository<ConditionOption>().Get(x => x.Name == "Increase Age" && x.TaskId == task.Id);
            ConditionOption option6 = _unitOfWork.Repository<ConditionOption>().Get(x => x.Name == "Age Raised To 20" && x.TaskId == task.Id);

            var ifAgeGreaterThan20 = _unitOfWork.Repository<DecisionPoint>().Get(x => x.Name == "If Age Greater Than 20" && x.TaskId == task.Id);
            if (ifAgeGreaterThan20 == null)
            {
                ifAgeGreaterThan20 = new DecisionPoint();
                ifAgeGreaterThan20.AssignedRole = Enums.ProjectRole.Sistem;
                ifAgeGreaterThan20.ProcessUniqueCode = Guid.NewGuid().ToString();
                ifAgeGreaterThan20.Name = "If Age Greater Than 20";
                ifAgeGreaterThan20.TaskId = task.Id;
                ifAgeGreaterThan20.DecisionMethod = isAgeGreaterThan20;
                ifAgeGreaterThan20.RepetitionFrequenceByHour = 1;
                ageLessThan20.NextProcess = ifAgeGreaterThan20;
                _unitOfWork.Repository<DecisionPoint>().Add(ifAgeGreaterThan20);
                _unitOfWork.Complete();
            }



            if (option5 == null)
            {
                option5 = new ConditionOption
                {
                    TaskId = task.Id,
                    Name = "Increase Age",
                    ProcessUniqueCode = Guid.NewGuid().ToString(),
                    Condition = ifAgeGreaterThan20,
                    AssignedRole = Enums.ProjectRole.Sistem,
                    Value = "N",
                    NextProcess = ifAgeGreaterThan20
                };
                _unitOfWork.Repository<ConditionOption>().Add(option5);
                _unitOfWork.Complete();
            }
            if (option6 == null)
            {
                option6 = new ConditionOption
                {
                    TaskId = task.Id,
                    Name = "Age Raised To 20",
                    ProcessUniqueCode = Guid.NewGuid().ToString(),
                    Condition = ifAgeGreaterThan20,
                    AssignedRole = Enums.ProjectRole.Sistem,
                    Value = "Y",
                    NextProcess = ageGreaterThan20

                };
                _unitOfWork.Repository<ConditionOption>().Add(option6);
                _unitOfWork.Complete();
            }



            SetWorkFlowDiagram(_unitOfWork, task.Id);
            #endregion


            base.Seed(context);


        }
    }
}
