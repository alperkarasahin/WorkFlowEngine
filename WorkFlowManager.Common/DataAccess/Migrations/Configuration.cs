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

        private string SentenceForDecisionPoint(bool systemAction, string sentence)
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
                            workFlowDiagram = string.Format(@"{0}{1}{{""{2}""}}-->|""{3}""|{4}{{""{5}""}};", workFlowDiagram, conditionForOption.ProcessUniqueCode, SentenceForDecisionPoint(conditionForOption.GetType() == typeof(DecisionPoint), conditionForOption.Name), islem.Name, islem.NextProcess.ProcessUniqueCode, SentenceForDecisionPoint(islem.NextProcess.GetType() == typeof(DecisionPoint), islem.NextProcess.Name), charForPrefixProcess, charForSuffixProcess);
                        }
                        else
                        {
                            workFlowDiagram = string.Format(@"{0}{1}{{""{2}""}}-->|""{3}""|{4}{6}""{5}""{7};", workFlowDiagram, conditionForOption.ProcessUniqueCode, SentenceForDecisionPoint(conditionForOption.GetType() == typeof(DecisionPoint), conditionForOption.Name), islem.Name, islem.NextProcess.ProcessUniqueCode, islem.NextProcess.Name, charForPrefixProcess, charForSuffixProcess);
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


            var masterTest1 = _unitOfWork.Repository<MasterTest>().Get(x => x.Name == "Applicant Information");
            if (masterTest1 == null)
            {
                masterTest1 = new MasterTest() { Name = "Applicant Information" };
                _unitOfWork.Repository<MasterTest>().Add(masterTest1);


                var workFlow = new WorkFlow { Name = "Application Flow" };
                _unitOfWork.Repository<WorkFlow>().Add(workFlow);


                #region TASK1
                var task = new Task { Name = "Driving License Application Flow", WorkFlow = workFlow, MethodServiceName = "TestWorkFlowProcessService", SpecialFormTemplateView = "WorkFlowTemplate", Controller = "TestWorkFlowProcess" };
                _unitOfWork.Repository<Task>().Add(task);


                var testWorkFlowForm = new FormView() { FormName = "Test Form", ViewName = "TestWorkFlowForm", Task = task, Completed = true };
                _unitOfWork.Repository<FormView>().Add(testWorkFlowForm);

                var isAgeLessThan18 = new DecisionMethod() { MethodName = "Is Age Less Than 18", MethodFunction = "IsAgeLessThan(Id, 18)", Task = task };
                _unitOfWork.Repository<DecisionMethod>().Add(isAgeLessThan18);

                var isAgeGreaterThan20Method = new DecisionMethod() { MethodName = "Is Age Greater Than 18", MethodFunction = "IsAgeGreaterThan(Id, 18)", Task = task };
                _unitOfWork.Repository<DecisionMethod>().Add(isAgeGreaterThan20Method);


                _unitOfWork.Complete();


                var process = ProcessFactory.CreateProcess(task, "Applicant Detail", Enums.ProjectRole.Admin);
                process.IsDescriptionMandatory = true;

                task.StartingProcess = process;

                var eyeCondition = ProcessFactory.CreateCondition(task, "Select Eye Condition", Enums.ProjectRole.Admin);
                var eyeConditionOption1 = ProcessFactory.CreateConditionOption("Blind", Enums.ProjectRole.Admin, eyeCondition);
                var eyeConditionOption2 = ProcessFactory.CreateConditionOption("Normal", Enums.ProjectRole.Admin, eyeCondition);
                var eyeConditionOption3 = ProcessFactory.CreateConditionOption("Color-Blind", Enums.ProjectRole.Admin, eyeCondition);

                process.NextProcess = eyeCondition;


                var candidateBlind = ProcessFactory.CreateProcess(task, "Candidate is not suitable for driving license", Enums.ProjectRole.Admin);
                eyeConditionOption1.NextProcess = candidateBlind;



                var process2 = ProcessFactory.CreateProcess(task, "Age Information", Enums.ProjectRole.Admin, "Enter your age", testWorkFlowForm);
                eyeConditionOption2.NextProcess = process2;
                eyeConditionOption3.NextProcess = process2;

                var ifAgeLessThan18 = ProcessFactory.CreateDecisionPoint(task, "If Age Less Than 18", isAgeLessThan18);

                var ifAgeLessThan18Option1 = ProcessFactory.CreateDecisionPointYesOption("Yes - Age Less Than 18", ifAgeLessThan18);
                var ifAgeLessThan18Option2 = ProcessFactory.CreateDecisionPointNoOption("No - Age Greater Than 18", ifAgeLessThan18);

                process2.NextProcess = ifAgeLessThan18;

                var isCandidateColorBlind = new DecisionMethod() { MethodName = "Is Candidate Color Blind", MethodFunction = "IsCandidateColorBlind(Id)", Task = task };
                _unitOfWork.Repository<DecisionMethod>().Add(isCandidateColorBlind);

                var ifCandidateIsColorBlind = ProcessFactory.CreateDecisionPoint(task, "If Candidate Is Color Blind", isCandidateColorBlind);

                var ifCandidateIsColorBlindOption1 = ProcessFactory.CreateDecisionPointYesOption("Yes - Candidate Is Color-blind", ifCandidateIsColorBlind);
                var ifCandidateIsColorBlindOption2 = ProcessFactory.CreateDecisionPointNoOption("No - Candidate Is Normal", ifCandidateIsColorBlind);

                ifAgeLessThan18Option2.NextProcess = ifCandidateIsColorBlind;

                var isAgeRaisedTo18DecisionPoint = ProcessFactory.CreateDecisionPoint(task, "Is Age Raised To 18?", isAgeGreaterThan20Method);
                ifAgeLessThan18Option1.NextProcess = isAgeRaisedTo18DecisionPoint;

                var option5 = ProcessFactory.CreateDecisionPointNoOption("No - Increase Age", isAgeRaisedTo18DecisionPoint);
                option5.NextProcess = isAgeRaisedTo18DecisionPoint;

                var option6 = ProcessFactory.CreateDecisionPointYesOption("Yes - Age Raised To 18", isAgeRaisedTo18DecisionPoint);
                option6.NextProcess = ifCandidateIsColorBlind;


                var normalDrivingLicense = ProcessFactory.CreateProcess(task, "Normal driving license", Enums.ProjectRole.Admin);
                var restrictedDrivingLicense = ProcessFactory.CreateProcess(task, "Restricted driving license", Enums.ProjectRole.Admin);

                ifCandidateIsColorBlindOption2.NextProcess = normalDrivingLicense;
                ifCandidateIsColorBlindOption1.NextProcess = restrictedDrivingLicense;


                _unitOfWork.Complete();
                SetWorkFlowDiagram(_unitOfWork, task.Id);
            }

            #endregion

            base.Seed(context);


        }
    }
}
