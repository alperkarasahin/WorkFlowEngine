using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Services.DbServices
{
    public class FormService : IDisposable
    {
        private readonly IUnitOfWork _unitOfWork;
        public FormService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        private IList<FormViewViewModel> GetAll(int taskId = 0)
        {
            IList<FormViewViewModel> result = new List<FormViewViewModel>();

            result = _unitOfWork.Repository<FormView>().GetList(x => x.TaskId == taskId)
                .Select(x => new FormViewViewModel()
                {
                    FormComplexity = x.FormComplexity,
                    FormName = x.FormName,
                    TaskId = x.TaskId,
                    Id = x.Id,
                    NumberOfPage = x.NumberOfPage,
                    ViewName = x.ViewName,
                    FormDescription = x.FormDescription,
                    Completed = x.Completed
                }).ToList();

            return result;
        }


        public IEnumerable<FormViewViewModel> Read(int taskId)
        {
            return GetAll(taskId);
        }

        public void Create(FormViewViewModel formView)
        {
            Update(formView);
        }


        public void Update(FormViewViewModel formView)
        {

            FormView formViewDB = _unitOfWork.Repository<FormView>().Get(x => x.Id == formView.Id);

            if (formViewDB == null)
            {
                formViewDB = new FormView();

                Mapper.Map(formView, formViewDB);
                _unitOfWork.Repository<FormView>().Add(formViewDB);
            }
            else
            {
                Mapper.Map(formView, formViewDB);
            }

            _unitOfWork.Complete();

            formView.Id = formViewDB.Id;
        }

        public void Destroy(FormViewViewModel formView)
        {
            var numberOfUsingProcess = _unitOfWork.Repository<Process>().GetList(x => x.FormViewId == formView.Id).Count();
            if (numberOfUsingProcess > 0)
            {
                throw new Exception("Form is being used.");
            }


            _unitOfWork.Repository<FormView>().Remove(formView.Id);

            _unitOfWork.Complete();

        }

    }
}
