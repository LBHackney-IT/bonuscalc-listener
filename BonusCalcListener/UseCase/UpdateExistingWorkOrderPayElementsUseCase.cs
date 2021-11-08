using BonusCalcListener.Boundary;
using BonusCalcListener.Gateway;
using BonusCalcListener.Infrastructure;
using BonusCalcListener.UseCase.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BonusCalcListener.UseCase
{
    public class UpdateExistingWorkOrderPayElementsUseCase : IUpdateExistingWorkOrderPayElements
    {
        private readonly IPayElementGateway _payElementGateway;
        private readonly IDbSaver _dbSaver;

        public UpdateExistingWorkOrderPayElementsUseCase(IPayElementGateway payElementGateway, IDbSaver dbSaver)
        {
            _payElementGateway = payElementGateway;
            _dbSaver = dbSaver;
        }

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            var data = message.EventData;

            //1. Get the work order (if any) from the database
            var existingPayElementCollection = (await _payElementGateway.GetReactiveRepairsPayElementsByWorkOrderId(data.WorkOrderId)).ToList();

            //2. If any have been found, remove them
            if (existingPayElementCollection.Any())
            {
                existingPayElementCollection.Clear();
            }

            //3. Add the new elements
            existingPayElementCollection.AddRange(message.EventData.CompletedWorkElements.Select(we => we.ToPayElement()));

            await _dbSaver.SaveChangesAsync();

        }
    }
}
