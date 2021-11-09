using BonusCalcListener.Boundary;
using BonusCalcListener.Gateway;
using BonusCalcListener.Gateway.Interfaces;
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
        private readonly ITimesheetGateway _timesheetGateway;
        private readonly IDbSaver _dbSaver;

        public UpdateExistingWorkOrderPayElementsUseCase(IPayElementGateway payElementGateway, ITimesheetGateway timesheetGateway, IDbSaver dbSaver)
        {
            _payElementGateway = payElementGateway;
            _timesheetGateway = timesheetGateway;
            _dbSaver = dbSaver;
        }

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            var data = message.EventData;

            //1. Get the work order (if any) from the database
            var existingPayElementCollection = (await _payElementGateway.GetReactiveRepairsPayElementsByWorkOrderId(data.WorkOrderId)).ToList();

            //1b. Get the timesheet for the operative


            //2. If any have been found, remove them
            if (existingPayElementCollection.Any())
            {
                existingPayElementCollection.Clear();
            }

            //3. Add the new elements
            existingPayElementCollection.AddRange(message.EventData.CompletedTasks.Select(we => we.ToPayElement()));

            await _dbSaver.SaveChangesAsync();

        }
    }
}
