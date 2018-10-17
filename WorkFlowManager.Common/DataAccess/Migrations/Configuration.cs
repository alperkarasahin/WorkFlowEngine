namespace WorkFlowManager.Common.DataAccess.Migrations
{
    using Factory;
    using System.Data.Entity.Migrations;
    using WorkFlowManager.Common.Constants;
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
                var task = new Task { Name = "Driving License Application Flow", WorkFlow = workFlow, MethodServiceName = "TestWorkFlowProcessService", SpecialFormTemplateView = "WorkFlowTemplate", Controller = "WorkFlowProcess" };
                _unitOfWork.Repository<Task>().Add(task);


                _unitOfWork.Complete();

                //////// Sub Task 1
                var subTask1 = new Task { TopTask = task, Name = "Psychotechnique Report Flow", WorkFlow = workFlow, MethodServiceName = "PsychotechniqueService", SpecialFormTemplateView = "WorkFlowTemplate", Controller = "WorkFlowProcess" };
                _unitOfWork.Repository<Task>().Add(subTask1);
                _unitOfWork.Complete();

                var psychotechnique = ProcessFactory.CreateCondition(subTask1, "Select Psychotechnique Result", Enums.ProjectRole.Officer, "PSYCHOTECHNICQUE");
                var psychotechniqueOption1 = ProcessFactory.CreateConditionOption("Adequate", Enums.ProjectRole.Officer, psychotechnique, "ADEQUATE");
                var psychotechniqueOption2 = ProcessFactory.CreateConditionOption("InAdequate", Enums.ProjectRole.Officer, psychotechnique, "INADEQUATE");

                subTask1.StartingProcess = psychotechnique;

                _unitOfWork.Repository<Condition>().Add(psychotechnique);
                _unitOfWork.Complete();
                WorkFlowUtil.SetWorkFlowDiagram(_unitOfWork, subTask1.Id);
                //////// End Of Sub Task 1


                //////// Sub Task 2
                var subTask2 = new Task { TopTask = task, Name = "Physical Examination Report Flow", WorkFlow = workFlow, MethodServiceName = "PhysicalExaminationService", SpecialFormTemplateView = "WorkFlowTemplate", Controller = "WorkFlowProcess" };
                _unitOfWork.Repository<Task>().Add(subTask2);
                _unitOfWork.Complete();

                var physicalExamination = ProcessFactory.CreateCondition(subTask2, "Select Physical Examination Result", Enums.ProjectRole.Officer, "PHYSICALEXAMINATION");
                var physicalExaminationOption1 = ProcessFactory.CreateConditionOption("Adequate", Enums.ProjectRole.Officer, physicalExamination, "ADEQUATE");
                var physicalExaminationOption2 = ProcessFactory.CreateConditionOption("InAdequate", Enums.ProjectRole.Officer, physicalExamination, "INADEQUATE");

                subTask2.StartingProcess = physicalExamination;
                _unitOfWork.Repository<Condition>().Add(physicalExamination);
                _unitOfWork.Complete();
                WorkFlowUtil.SetWorkFlowDiagram(_unitOfWork, subTask2.Id);
                //////// End Of Sub Task 2




                var testWorkFlowForm = new FormView() { FormName = "Test Form", ViewName = "TestWorkFlowForm", Task = task, Completed = true };
                _unitOfWork.Repository<FormView>().Add(testWorkFlowForm);




                var isAgeLessThan18 = new DecisionMethod() { MethodName = "Is Age Less Than 18", MethodFunction = "IsAgeLessThan(Id, 18)", Task = task };
                _unitOfWork.Repository<DecisionMethod>().Add(isAgeLessThan18);

                var isAgeGreaterThan20Method = new DecisionMethod() { MethodName = "Is Age Greater Than 18", MethodFunction = "IsAgeGreaterThan(Id, 18)", Task = task };
                _unitOfWork.Repository<DecisionMethod>().Add(isAgeGreaterThan20Method);


                _unitOfWork.Complete();


                var process = ProcessFactory.CreateProcess(task, "Applicant Detail", Enums.ProjectRole.Officer);
                process.IsDescriptionMandatory = true;

                task.StartingProcess = process;

                var eyeCondition = ProcessFactory.CreateCondition(task, "Select Eye Condition", Enums.ProjectRole.Officer, "EYECONDITION");
                var eyeConditionOption1 = ProcessFactory.CreateConditionOption("Blind", Enums.ProjectRole.Officer, eyeCondition, "BLIND");
                var eyeConditionOption2 = ProcessFactory.CreateConditionOption("Normal", Enums.ProjectRole.Officer, eyeCondition, "NORMAL");
                var eyeConditionOption3 = ProcessFactory.CreateConditionOption("Color-Blind", Enums.ProjectRole.Officer, eyeCondition, "COLORBLIND");

                process.NextProcess = eyeCondition;


                var candidateBlind = ProcessFactory.CreateProcess(task, "Candidate is not suitable for driving license", Enums.ProjectRole.Officer);
                eyeConditionOption1.NextProcess = candidateBlind;



                var process2 = ProcessFactory.CreateProcess(task, "Age Information", Enums.ProjectRole.Officer, "Enter your age", testWorkFlowForm);
                eyeConditionOption2.NextProcess = process2;
                eyeConditionOption3.NextProcess = process2;

                var ifAgeLessThan18 = ProcessFactory.CreateDecisionPoint(task, "If Age Less Than 18", isAgeLessThan18);

                var ifAgeLessThan18Option1 = ProcessFactory.CreateDecisionPointYesOption("Yes - Age Less Than 18", ifAgeLessThan18);
                var ifAgeLessThan18Option2 = ProcessFactory.CreateDecisionPointNoOption("No - Age Greater Than 18", ifAgeLessThan18);

                process2.NextProcess = ifAgeLessThan18;

                var healthForm = new FormView() { FormName = "Health Information", ViewName = "HealthInformationWorkFlowForm", Task = task, Completed = true };
                _unitOfWork.Repository<FormView>().Add(healthForm);

                var processHealthInformation = ProcessFactory.CreateProcess(task, "Health Information (Start Sub-Process)", Enums.ProjectRole.Officer, "Health Information Sub Process Starting Area", healthForm);

                var isSubProcessCompleted = new DecisionMethod() { MethodName = "Is Sub Process Completed", MethodFunction = "IsSubProcessCompleted(Id)", Task = task };
                _unitOfWork.Repository<DecisionMethod>().Add(isSubProcessCompleted);

                var ifSubProcessCompleted = ProcessFactory.CreateDecisionPoint(task, "If Sub Process Completed", isSubProcessCompleted);

                var ifSubProcessCompletedOption1 = ProcessFactory.CreateDecisionPointYesOption("Yes - Sub Process Completed", ifSubProcessCompleted);
                var ifSubProcessCompletedOption2 = ProcessFactory.CreateDecisionPointNoOption("No - Sub Process Does Not Completed", ifSubProcessCompleted);

                processHealthInformation.NextProcess = ifSubProcessCompleted;
                ifSubProcessCompletedOption2.NextProcess = ifSubProcessCompleted;



                var isCandidateColorBlind = new DecisionMethod() { MethodName = "Is Candidate Color Blind", MethodFunction = "IsCandidateColorBlind(Id)", Task = task };
                _unitOfWork.Repository<DecisionMethod>().Add(isCandidateColorBlind);

                var ifCandidateIsColorBlind = ProcessFactory.CreateDecisionPoint(task, "If Candidate Is Color Blind", isCandidateColorBlind);

                var ifCandidateIsColorBlindOption1 = ProcessFactory.CreateDecisionPointYesOption("Yes - Candidate Is Color-blind", ifCandidateIsColorBlind);
                var ifCandidateIsColorBlindOption2 = ProcessFactory.CreateDecisionPointNoOption("No - Candidate Is Normal", ifCandidateIsColorBlind);

                ifAgeLessThan18Option2.NextProcess = processHealthInformation;

                var isAgeRaisedTo18DecisionPoint = ProcessFactory.CreateDecisionPoint(task, "Is Age Raised To 18?", isAgeGreaterThan20Method);

                ifAgeLessThan18Option1.NextProcess = isAgeRaisedTo18DecisionPoint;

                var option5 = ProcessFactory.CreateDecisionPointNoOption("No - Increase Age", isAgeRaisedTo18DecisionPoint);
                option5.NextProcess = isAgeRaisedTo18DecisionPoint;

                var option6 = ProcessFactory.CreateDecisionPointYesOption("Yes - Age Raised To 18", isAgeRaisedTo18DecisionPoint);
                option6.NextProcess = processHealthInformation;

                ifSubProcessCompletedOption1.NextProcess = ifCandidateIsColorBlind;

                var normalDrivingLicense = ProcessFactory.CreateProcess(task, "Normal driving license", Enums.ProjectRole.Officer);
                var restrictedDrivingLicense = ProcessFactory.CreateProcess(task, "Restricted driving license", Enums.ProjectRole.Officer);

                ifCandidateIsColorBlindOption2.NextProcess = normalDrivingLicense;
                ifCandidateIsColorBlindOption1.NextProcess = restrictedDrivingLicense;


                _unitOfWork.Complete();
                WorkFlowUtil.SetWorkFlowDiagram(_unitOfWork, task.Id);
            }

            #endregion

            base.Seed(context);


        }
    }
}
