using BonusCalcListener.Boundary;
using BonusCalcListener.UseCase.Interfaces;
using System;
using System.Threading.Tasks;

namespace BonusCalcListener.UseCase
{
    public class UpdateExistingWorkOrderPayElementsUseCase : IUpdateExistingWorkOrderPayElements
    {
        public Task ProcessMessageAsync(EntityEventSns message)
        {
            throw new NotImplementedException();
        }
    }
}
