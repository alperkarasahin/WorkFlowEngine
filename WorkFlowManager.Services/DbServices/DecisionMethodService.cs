using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.ViewModels;

namespace WorkFlowManager.Services.DbServices
{

    public class DecisionMethodService : IDisposable
    {
        private readonly IUnitOfWork _unitOfWork;

        public DecisionMethodService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

        private IList<DecisionMethodViewModel> GetAll(int taskId = 0)
        {
            IList<DecisionMethodViewModel> result = new List<DecisionMethodViewModel>();

            result = _unitOfWork.Repository<DecisionMethod>().GetList(x => x.TaskId == taskId)
                .Select(x => new DecisionMethodViewModel()
                {
                    MethodDescription = x.MethodDescription,
                    MethodFunction = x.MethodFunction,
                    MethodName = x.MethodName,
                    TaskId = x.TaskId,
                    Id = x.Id,
                    Completed = x.Completed
                }).ToList();

            return result;
        }

        public IEnumerable<DecisionMethodViewModel> Read(int gorevId)
        {
            return GetAll(gorevId);
        }

        public void Create(DecisionMethodViewModel decisionMethod)
        {
            Update(decisionMethod);
        }


        public void Update(DecisionMethodViewModel decisionMethod)
        {
            DecisionMethod decisionMethodDB = _unitOfWork.Repository<DecisionMethod>().Get(x => x.Id == decisionMethod.Id);

            if (decisionMethodDB == null)
            {
                decisionMethodDB = new DecisionMethod();

                Mapper.Map(decisionMethod, decisionMethodDB);
                _unitOfWork.Repository<DecisionMethod>().Add(decisionMethodDB);
            }
            else
            {
                Mapper.Map(decisionMethod, decisionMethodDB);
            }

            _unitOfWork.Complete();

            decisionMethod.Id = decisionMethodDB.Id;
        }

        public void Destroy(DecisionMethodViewModel decisionMethod)
        {
            //Formun iş akışları içerisinde kullanılmamış olması gerekli
            var numberOfUsedDecisionPoint = _unitOfWork.Repository<DecisionPoint>().GetList(x => x.DecisionMethodId == decisionMethod.Id).Count();
            if (numberOfUsedDecisionPoint > 0)
            {
                throw new Exception("Used Method");
            }
            _unitOfWork.Repository<DecisionMethod>().Remove(decisionMethod.Id);

            _unitOfWork.Complete();

        }


    }
}
