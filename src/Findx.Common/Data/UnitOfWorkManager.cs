using Findx.DependencyInjection;
using System.Collections.Generic;
namespace Findx.Data
{
    public class UnitOfWorkManager : IUnitOfWorkManager, IScopeDependency
    {
        private readonly IEnumerable<IUnitOfWork> _unitOfWorks;

        public UnitOfWorkManager(IEnumerable<IUnitOfWork> unitOfWorks)
        {
            _unitOfWorks = unitOfWorks;
        }

        public void BeginTran()
        {
            foreach (var unitOfWork in _unitOfWorks)
                unitOfWork.BeginTran();
        }

        public void CommitTran()
        {
            foreach (var unitOfWork in _unitOfWorks)
                unitOfWork.CommitTran();
        }

        public void RollbackTran()
        {
            foreach (var unitOfWork in _unitOfWorks)
                unitOfWork.RollbackTran();
        }
    }
}
