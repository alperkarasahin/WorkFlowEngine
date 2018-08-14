namespace WorkFlowManager.Common.DataAccess.Migrations
{
    using Factory;
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
                masterTest1 = new MasterTest() { Name = "Task1 Test" };
                _unitOfWork.Repository<MasterTest>().Add(masterTest1);


                var workFlow = new WorkFlow { Name = "WorkFlow1" };
                _unitOfWork.Repository<WorkFlow>().Add(workFlow);


                #region TASK1
                var task = new Task { Name = "Task1", WorkFlow = workFlow, MethodServiceName = "TestWorkFlowProcessService", SpecialFormTemplateView = "OzelFormSablon", Controller = "TestWorkFlowProcess" };
                _unitOfWork.Repository<Task>().Add(task);


                var testForm = new FormView() { FormName = "Test Form", ViewName = "TestForm", Task = task, Completed = true };
                _unitOfWork.Repository<FormView>().Add(testForm);

                var isAgeLessThan20 = new DecisionMethod() { MethodName = "Is Age Less Than 20", MethodFunction = "IsAgeLessThan20(Id)", Task = task };
                _unitOfWork.Repository<DecisionMethod>().Add(isAgeLessThan20);

                var isAgeGreaterThan20 = new DecisionMethod() { MethodName = "Is Age Greater Than 20", MethodFunction = "IsAgeGreaterThan20(Id)", Task = task };
                _unitOfWork.Repository<DecisionMethod>().Add(isAgeGreaterThan20);


                _unitOfWork.Complete();


                var process = ProcessFactory.CreateProcess(task, "Process 1", Enums.ProjectRole.Admin);
                task.StartingProcess = process;

                var condition = ProcessFactory.CreateCondition(task, "Condition 1", Enums.ProjectRole.Admin);
                var option1 = ProcessFactory.CreateConditionOption("Option 1", Enums.ProjectRole.Admin, condition);
                var option2 = ProcessFactory.CreateConditionOption("Option 2", Enums.ProjectRole.Admin, condition);

                process.NextProcess = condition;

                var process2 = ProcessFactory.CreateProcess(task, "Process 2", Enums.ProjectRole.Admin);
                option1.NextProcess = process2;


                var ifAgeLessThan20 = ProcessFactory.CreateDecisionPoint(task, "If Age Less Than 20", isAgeLessThan20);

                var option3 = ProcessFactory.CreateDecisionPointYesOption("Age Less Than 20", ifAgeLessThan20);
                var option4 = ProcessFactory.CreateDecisionPointNoOption("Age Greater Than 20", ifAgeLessThan20);

                process2.NextProcess = ifAgeLessThan20;

                var ageLessThan20 = ProcessFactory.CreateProcess(task, "Age Less Than 20 Selected", Enums.ProjectRole.Admin);
                option3.NextProcess = ageLessThan20;

                var ageGreaterThan20 = ProcessFactory.CreateProcess(task, "Age Greater Than 20 Selected", Enums.ProjectRole.Admin);
                option4.NextProcess = ageGreaterThan20;

                var ifAgeGreaterThan20 = ProcessFactory.CreateDecisionPoint(task, "If Age Greater Than 20", isAgeGreaterThan20);
                ageLessThan20.NextProcess = ifAgeGreaterThan20;

                var option5 = ProcessFactory.CreateDecisionPointNoOption("Increase Age", ifAgeGreaterThan20);
                option5.NextProcess = ifAgeGreaterThan20;

                var option6 = ProcessFactory.CreateDecisionPointYesOption("Age Raised To 20", ifAgeGreaterThan20);
                option6.NextProcess = ageGreaterThan20;

                _unitOfWork.Complete();
                SetWorkFlowDiagram(_unitOfWork, task.Id);
            }

            #endregion

            base.Seed(context);


        }
    }
}
