namespace WorkFlowManager.Common.DataAccess.Migrations
{
    using System.Data.Entity.Migrations;
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


            var workFlow = _unitOfWork.Repository<WorkFlow>().Get(x => x.Name == "WorkFlow1");
            if (workFlow == null)
            {
                workFlow = new WorkFlow { Name = "WorkFlow1" };
                _unitOfWork.Repository<WorkFlow>().Add(workFlow);
                _unitOfWork.Complete();
            };

            var task = _unitOfWork.Repository<Task>().Get(x => x.Name == "Task1");

            if (task == null)
            {
                task = new Task { Name = "Task1", WorkFlowId = workFlow.Id };
                _unitOfWork.Repository<Task>().Add(task);
                _unitOfWork.Complete();
            }


            var testForm = _unitOfWork.Repository<FormView>().Get(x => x.TaskId == task.Id && x.FormName == "Test Form");
            if (testForm == null)
            {
                testForm = new FormView() { FormName = "Test Form", ViewName = "TestForm", TaskId = task.Id };
                _unitOfWork.Repository<FormView>().Add(testForm);
                _unitOfWork.Complete();
            }

            var testMethod = _unitOfWork.Repository<DecisionMethod>().Get(x => x.TaskId == task.Id && x.MethodName == "Test Method");
            if (testMethod == null)
            {
                testMethod = new DecisionMethod() { MethodName = "Test Method", MethodSql = "TestMethod(Id)", TaskId = task.Id };
                _unitOfWork.Repository<DecisionMethod>().Add(testMethod);
                _unitOfWork.Complete();
            }



            base.Seed(context);


        }
    }
}
