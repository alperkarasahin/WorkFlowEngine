namespace WorkFlowManager.Common.DataAccess.Migrations
{
    using Factory;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using WorkFlowManager.Common.Constants;
    using WorkFlowManager.Common.DataAccess._Context;
    using WorkFlowManager.Common.DataAccess._UnitOfWork;
    using WorkFlowManager.Common.Dto;
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

            #region Driving License Flow Example
            var applicationWorkFlow = _unitOfWork.Repository<WorkFlow>().Get(x => x.Name == "Application Flow");

            if (applicationWorkFlow == null)
            {
                applicationWorkFlow = new WorkFlow { Name = "Application Flow" };
                _unitOfWork.Repository<WorkFlow>().Add(applicationWorkFlow);
            }

            var taskDrivingLicense = _unitOfWork.Repository<Task>().Get(x => x.Name == "Driving License Application Flow");
            if (taskDrivingLicense == null)
            {
                taskDrivingLicense = new Task { Name = "Driving License Application Flow", WorkFlow = applicationWorkFlow, MethodServiceName = "TestWorkFlowProcessService", SpecialFormTemplateView = "WorkFlowTemplate", Controller = "WorkFlowProcess" };
                _unitOfWork.Repository<Task>().Add(taskDrivingLicense);
                _unitOfWork.Complete();
            }

            var masterTest1 = _unitOfWork.Repository<BusinessProcess>().Get(x => x.Name == "Applicant Information");
            if (masterTest1 == null)
            {
                masterTest1 = new BusinessProcess() { Name = "Applicant Information", RelatedTask = taskDrivingLicense };
                _unitOfWork.Repository<BusinessProcess>().Add(masterTest1);

                #region DrivingLicense

                #region Sub Task 1
                var subTask1 = new Task { TopTask = taskDrivingLicense, Name = "Psychotechnique Report Flow", WorkFlow = applicationWorkFlow, MethodServiceName = "PsychotechniqueService", SpecialFormTemplateView = "WorkFlowTemplate", Controller = "WorkFlowProcess" };
                _unitOfWork.Repository<Task>().Add(subTask1);
                _unitOfWork.Complete();

                var psychotechnique = ProcessFactory.CreateCondition(subTask1, "Select Psychotechnique Result", Enums.ProjectRole.Officer, "PSYCHOTECHNICQUE");
                var psychotechniqueOption1 = ProcessFactory.CreateConditionOption("Adequate", Enums.ProjectRole.Officer, psychotechnique, "ADEQUATE");
                var psychotechniqueOption2 = ProcessFactory.CreateConditionOption("InAdequate", Enums.ProjectRole.Officer, psychotechnique, "INADEQUATE");

                subTask1.StartingProcess = psychotechnique;

                _unitOfWork.Repository<Condition>().Add(psychotechnique);
                _unitOfWork.Complete();
                WorkFlowUtil.SetWorkFlowDiagram(_unitOfWork, subTask1.Id);
                #endregion End Of Sub Task 1

                #region Sub Task 2
                var subTask2 = new Task { TopTask = taskDrivingLicense, Name = "Physical Examination Report Flow", WorkFlow = applicationWorkFlow, MethodServiceName = "PhysicalExaminationService", SpecialFormTemplateView = "WorkFlowTemplate", Controller = "WorkFlowProcess" };
                _unitOfWork.Repository<Task>().Add(subTask2);
                _unitOfWork.Complete();

                var physicalExamination = ProcessFactory.CreateCondition(subTask2, "Select Physical Examination Result", Enums.ProjectRole.Officer, "PHYSICALEXAMINATION");
                var physicalExaminationOption1 = ProcessFactory.CreateConditionOption("Adequate", Enums.ProjectRole.Officer, physicalExamination, "ADEQUATE");
                var physicalExaminationOption2 = ProcessFactory.CreateConditionOption("InAdequate", Enums.ProjectRole.Officer, physicalExamination, "INADEQUATE");

                subTask2.StartingProcess = physicalExamination;
                _unitOfWork.Repository<Condition>().Add(physicalExamination);
                _unitOfWork.Complete();
                WorkFlowUtil.SetWorkFlowDiagram(_unitOfWork, subTask2.Id);
                #endregion End Of Sub Task 2

                var testWorkFlowForm = new FormView() { FormName = "Test Form", ViewName = "TestWorkFlowForm", Task = taskDrivingLicense, Completed = true };
                _unitOfWork.Repository<FormView>().Add(testWorkFlowForm);

                var isAgeLessThan18 = new DecisionMethod() { MethodName = "Is Age Less Than 18", MethodFunction = "IsAgeLessThan(Id, 18)", Task = taskDrivingLicense };
                _unitOfWork.Repository<DecisionMethod>().Add(isAgeLessThan18);

                var isAgeGreaterThan20Method = new DecisionMethod() { MethodName = "Is Age Greater Than 18", MethodFunction = "IsAgeGreaterThan(Id, 18)", Task = taskDrivingLicense };
                _unitOfWork.Repository<DecisionMethod>().Add(isAgeGreaterThan20Method);

                _unitOfWork.Complete();

                var process = ProcessFactory.CreateProcess(taskDrivingLicense, "Applicant Detail", Enums.ProjectRole.Officer);
                process.IsDescriptionMandatory = true;

                taskDrivingLicense.StartingProcess = process;

                var eyeCondition = ProcessFactory.CreateCondition(taskDrivingLicense, "Select Eye Condition", Enums.ProjectRole.Officer, "EYECONDITION");
                var eyeConditionOption1 = ProcessFactory.CreateConditionOption("Blind", Enums.ProjectRole.Officer, eyeCondition, "BLIND");
                var eyeConditionOption2 = ProcessFactory.CreateConditionOption("Normal", Enums.ProjectRole.Officer, eyeCondition, "NORMAL");
                var eyeConditionOption3 = ProcessFactory.CreateConditionOption("Color-Blind", Enums.ProjectRole.Officer, eyeCondition, "COLORBLIND");

                process.NextProcess = eyeCondition;

                var candidateIsNotSuitableForDrivingLicense = ProcessFactory.CreateProcess(taskDrivingLicense, "Candidate is not suitable for driving license", Enums.ProjectRole.Officer);
                eyeConditionOption1.NextProcess = candidateIsNotSuitableForDrivingLicense;

                var process2 = ProcessFactory.CreateProcess(taskDrivingLicense, "Age Information", Enums.ProjectRole.Officer, "Enter your age", testWorkFlowForm);
                eyeConditionOption2.NextProcess = process2;
                eyeConditionOption3.NextProcess = process2;

                var ifAgeLessThan18 = ProcessFactory.CreateDecisionPoint(taskDrivingLicense, "If Age Less Than 18", isAgeLessThan18);

                var ifAgeLessThan18Option1 = ProcessFactory.CreateDecisionPointYesOption("Yes - Age Less Than 18", ifAgeLessThan18);
                var ifAgeLessThan18Option2 = ProcessFactory.CreateDecisionPointNoOption("No - Age Greater Than 18", ifAgeLessThan18);

                process2.NextProcess = ifAgeLessThan18;

                List<TaskVariable> taskVariableList = new List<TaskVariable>();
                taskVariableList.Add(new TaskVariable { TaskId = subTask1.Id, VariableName = "PSYCHOTECHNICQUETASKCOUNT" });
                taskVariableList.Add(new TaskVariable { TaskId = subTask2.Id, VariableName = "PHYSICALEXAMINATIONTASKCOUNT" });
                var processHealthInformation = ProcessFactory.CreateSubProcess(taskDrivingLicense, "Health Information", taskVariableList);

                var isHealthStatusAdequate = new DecisionMethod() { MethodName = "Is Health Status Adequate", MethodFunction = "IsHealthStatusAdequate(Id)", Task = taskDrivingLicense };
                _unitOfWork.Repository<DecisionMethod>().Add(isHealthStatusAdequate);

                var ifHealthStatusAdequate = ProcessFactory.CreateDecisionPoint(taskDrivingLicense, "If Health Status Adequate", isHealthStatusAdequate);

                var ifHealthStatusAdequateOption1 = ProcessFactory.CreateDecisionPointYesOption("Yes - Health Status Is Adequate", ifHealthStatusAdequate);
                var ifHealthStatusAdequateOption2 = ProcessFactory.CreateDecisionPointNoOption("No - Health Status Is Not Adequate", ifHealthStatusAdequate);

                processHealthInformation.NextProcess = ifHealthStatusAdequate;
                ifHealthStatusAdequateOption2.NextProcess = candidateIsNotSuitableForDrivingLicense;

                var isCandidateColorBlind = new DecisionMethod() { MethodName = "Is Candidate Color Blind", MethodFunction = "IsCandidateColorBlind(Id)", Task = taskDrivingLicense };
                _unitOfWork.Repository<DecisionMethod>().Add(isCandidateColorBlind);

                var ifCandidateIsColorBlind = ProcessFactory.CreateDecisionPoint(taskDrivingLicense, "If Candidate Is Color Blind", isCandidateColorBlind);

                var ifCandidateIsColorBlindOption1 = ProcessFactory.CreateDecisionPointYesOption("Yes - Candidate Is Color-blind", ifCandidateIsColorBlind);
                var ifCandidateIsColorBlindOption2 = ProcessFactory.CreateDecisionPointNoOption("No - Candidate Is Normal", ifCandidateIsColorBlind);

                ifAgeLessThan18Option2.NextProcess = processHealthInformation;

                var isAgeRaisedTo18DecisionPoint = ProcessFactory.CreateDecisionPoint(taskDrivingLicense, "Is Age Raised To 18?", isAgeGreaterThan20Method);

                ifAgeLessThan18Option1.NextProcess = isAgeRaisedTo18DecisionPoint;

                var option5 = ProcessFactory.CreateDecisionPointNoOption("No - Increase Age", isAgeRaisedTo18DecisionPoint);
                option5.NextProcess = isAgeRaisedTo18DecisionPoint;

                var option6 = ProcessFactory.CreateDecisionPointYesOption("Yes - Age Raised To 18", isAgeRaisedTo18DecisionPoint);
                option6.NextProcess = processHealthInformation;

                ifHealthStatusAdequateOption1.NextProcess = ifCandidateIsColorBlind;

                var normalDrivingLicense = ProcessFactory.CreateProcess(taskDrivingLicense, "Normal driving license", Enums.ProjectRole.Officer);
                var restrictedDrivingLicense = ProcessFactory.CreateProcess(taskDrivingLicense, "Restricted driving license", Enums.ProjectRole.Officer);

                ifCandidateIsColorBlindOption2.NextProcess = normalDrivingLicense;
                ifCandidateIsColorBlindOption1.NextProcess = restrictedDrivingLicense;


                _unitOfWork.Complete();
                WorkFlowUtil.SetWorkFlowDiagram(_unitOfWork, taskDrivingLicense.Id);
                #endregion DrivingLicense
            }
            //update
            if (masterTest1.RelatedTask == null)
            {
                masterTest1.RelatedTask = taskDrivingLicense;
            }
            #endregion Driving License Flow Example

            #region Purchasing Flow Example
            var purchasingWorkFlow = _unitOfWork.Repository<WorkFlow>().Get(x => x.Name == "Purchasing Flow");

            if (purchasingWorkFlow == null)
            {
                purchasingWorkFlow = new WorkFlow { Name = "Purchasing Flow" };
                _unitOfWork.Repository<WorkFlow>().Add(purchasingWorkFlow);
            }

            var taskPurchasing = _unitOfWork.Repository<Task>().Get(x => x.Name == "Purchasing System Flow");
            if (taskPurchasing == null)
            {
                taskPurchasing = new Task { Name = "Purchasing System Flow", WorkFlow = purchasingWorkFlow, MethodServiceName = "PurchasingWorkFlowProcessService", SpecialFormTemplateView = "WorkFlowTemplate", Controller = "WorkFlowProcess" };
                _unitOfWork.Repository<Task>().Add(taskPurchasing);
                _unitOfWork.Complete();
            }


            var masterTest2 = _unitOfWork.Repository<BusinessProcess>().Get(x => x.Name == "Purchasing Example");

            if (masterTest2 == null)
            {
                masterTest2 = new BusinessProcess() { Name = "Purchasing Example", RelatedTask = taskPurchasing };
                _unitOfWork.Repository<BusinessProcess>().Add(masterTest2);


                #region PurchasingTask

                // - 1
                var purchaseRequestForm = new FormView() { FormName = "Purchase Request Form", ViewName = "PurchaseRequestForm", Task = taskPurchasing, Completed = false };
                _unitOfWork.Repository<FormView>().Add(purchaseRequestForm);
                var purchaseRequest = ProcessFactory.CreateProcess(taskPurchasing, "Purchase Request", Enums.ProjectRole.PurchasingOfficer, "Enter Purchase Information", purchaseRequestForm);

                // - 2
                var analyseAndConfirmPurchaseRequest = ProcessFactory.CreateCondition(taskPurchasing, "Analyse & Confirm Purchase Request", Enums.ProjectRole.SpendingOfficer);
                var analyseAndConfirmPurchaseRequestOption1 = ProcessFactory.CreateConditionOption("Purchase Is Suitable", Enums.ProjectRole.PurchasingOfficer, analyseAndConfirmPurchaseRequest);
                var analyseAndConfirmPurchaseRequestOption2 = ProcessFactory.CreateConditionOption("Purchase Is Not Suitable", Enums.ProjectRole.System, analyseAndConfirmPurchaseRequest);

                // - 3
                var supplierSelectionForm = new FormView() { FormName = "Supplier Selection Form", ViewName = "SupplierSelectionForm", Task = taskPurchasing, Completed = false };
                _unitOfWork.Repository<FormView>().Add(supplierSelectionForm);
                var supplierSelection = ProcessFactory.CreateProcess(taskPurchasing, "Supplier Selection", Enums.ProjectRole.PurchasingOfficer, "Select Supplier", supplierSelectionForm);

                // - 4
                var proposalForm = new FormView() { FormName = "Proposal Form", ViewName = "ProposalForm", Task = taskPurchasing, Completed = false };
                _unitOfWork.Repository<FormView>().Add(proposalForm);
                var preparationOfProposalForm = ProcessFactory.CreateProcess(taskPurchasing, "Proposal Form", Enums.ProjectRole.PurchasingOfficer, "Prepare Propasal Form", proposalForm);

                // - 5 
                var confirmationOfProposalForm = ProcessFactory.CreateCondition(taskPurchasing, "Confirmation Of Proposal Form", Enums.ProjectRole.SpendingOfficer, "PROPOSALFORMSTATUS");
                var confirmationOfProposalFormOption1 = ProcessFactory.CreateConditionOption("Proposal Form Is Suitable", Enums.ProjectRole.PurchasingOfficer, confirmationOfProposalForm, "CONFIRMED");
                var confirmationOfProposalFormOption2 = ProcessFactory.CreateConditionOption("Proposal Form Is Not Suitable", Enums.ProjectRole.PurchasingOfficer, confirmationOfProposalForm, "REFUSED");

                // - 6
                var orderform = new FormView() { FormName = "Order Form", ViewName = "OrderForm", Task = taskPurchasing, Completed = false };
                _unitOfWork.Repository<FormView>().Add(orderform);
                var ordering = ProcessFactory.CreateProcess(taskPurchasing, "Order Form", Enums.ProjectRole.PurchasingOfficer, "Prepare Order Form", orderform);

                // - 7
                var receivingProductForm = new FormView() { FormName = "Receiving Form", ViewName = "ReceiveForm", Task = taskPurchasing, Completed = false };
                _unitOfWork.Repository<FormView>().Add(receivingProductForm);
                var receiving = ProcessFactory.CreateProcess(taskPurchasing, "Receiving Product Form", Enums.ProjectRole.UnitPurchasingOfficer, "Receive Product Form", receivingProductForm);

                // - 8
                var registerProductForm = new FormView() { FormName = "Register Product Form", ViewName = "RegiserForm", Task = taskPurchasing, Completed = false };
                _unitOfWork.Repository<FormView>().Add(registerProductForm);
                var registerProduct = ProcessFactory.CreateProcess(taskPurchasing, "Register Product", Enums.ProjectRole.PurchasingOfficer, "Register Product", registerProductForm);

                // Set navigation

                taskPurchasing.StartingProcess = purchaseRequest;
                purchaseRequest.NextProcess = analyseAndConfirmPurchaseRequest;
                analyseAndConfirmPurchaseRequestOption1.NextProcess = supplierSelection;

                supplierSelection.NextProcess = preparationOfProposalForm;
                preparationOfProposalForm.NextProcess = confirmationOfProposalForm;

                confirmationOfProposalFormOption1.NextProcess = ordering;
                confirmationOfProposalFormOption2.NextProcess = preparationOfProposalForm;

                ordering.NextProcess = receiving;
                receiving.NextProcess = registerProduct;

                _unitOfWork.Complete();
                WorkFlowUtil.SetWorkFlowDiagram(_unitOfWork, taskPurchasing.Id);
                #endregion PurchasingTask
            }

            #endregion Purchasing Flow Example
            base.Seed(context);


        }
    }
}
