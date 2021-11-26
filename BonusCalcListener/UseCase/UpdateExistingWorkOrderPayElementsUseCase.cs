using BonusCalcListener.Boundary;
using BonusCalcListener.Gateway;
using BonusCalcListener.Gateway.Interfaces;
using BonusCalcListener.Infrastructure;
using BonusCalcListener.UseCase.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BonusCalcListener.UseCase
{
    public class UpdateExistingWorkOrderPayElementsUseCase : IUpdateExistingWorkOrderPayElements
    {
        private readonly IPayElementGateway _payElementGateway;
        private readonly ITimesheetGateway _timesheetGateway;
        private readonly IMapPayElements _payElementMapper;
        private readonly IDbSaver _dbSaver;
        private readonly ILogger<UpdateExistingWorkOrderPayElementsUseCase> _logger;

        const int REACTIVE_REPAIRS_PAY_ELEMENT_TYPE = 301;

        public UpdateExistingWorkOrderPayElementsUseCase(IPayElementGateway payElementGateway, ITimesheetGateway timesheetGateway, IMapPayElements payElementMapper, IDbSaver dbSaver, ILogger<UpdateExistingWorkOrderPayElementsUseCase> logger)
        {
            _payElementGateway = payElementGateway;
            _timesheetGateway = timesheetGateway;
            _payElementMapper = payElementMapper;
            _dbSaver = dbSaver;
            _logger = logger;
        }

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            var data = message.EventData;

            _logger.LogInformation($"Starting to process update to work order ID: {data.WorkOrderId}");

            //1a. Get the timesheet for the operative based on the opId and closed time of the work order
            var operativeTimesheet = await _timesheetGateway.GetCurrentTimeSheetForOperative(data.OperativePrn, data.ClosedTime);

            if (operativeTimesheet == null)
            {
                throw new InvalidOperationException($"No operative timesheet found to add work elements to! OperativeID: {data.OperativePrn} ClosedTime: {data.ClosedTime}");
            }

            _logger.LogInformation($"Found timesheet {operativeTimesheet.Id} for {data.WorkOrderId}");

            //1b. Get the work order (if any) from the database
            var existingPayElementCollection = await _payElementGateway.GetPayElementsByWorkOrderId(data.WorkOrderId, REACTIVE_REPAIRS_PAY_ELEMENT_TYPE);

            //2. If any have been found, remove them
            if (existingPayElementCollection.Any())
            {
                _logger.LogInformation($"Found an existing pay element(s) for WO {data.WorkOrderId} which will be overwritten");
                existingPayElementCollection.Clear();
            }

            //3a. Create pay element
            var npe = _payElementMapper.BuildPayElement(message.EventData, operativeTimesheet);

            //3b. Add the new elements & save
            existingPayElementCollection.Add(npe);

            _logger.LogInformation($"Added {npe.Value} SMVs to operative {data.OperativePrn} for work order {data.WorkOrderId}!");

            await _dbSaver.SaveChangesAsync();

        }
    }
}
