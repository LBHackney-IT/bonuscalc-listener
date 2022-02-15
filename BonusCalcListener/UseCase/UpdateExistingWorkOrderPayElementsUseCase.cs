using BonusCalcListener.Boundary;
using BonusCalcListener.Gateway.Interfaces;
using BonusCalcListener.Infrastructure;
using BonusCalcListener.UseCase.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BonusCalcListener.UseCase
{
    public class UpdateExistingWorkOrderPayElementsUseCase : IUpdateExistingWorkOrderPayElements
    {
        private readonly ITimesheetGateway _timesheetGateway;
        private readonly IMapPayElements _payElementMapper;
        private readonly IDbSaver _dbSaver;
        private readonly ILogger<UpdateExistingWorkOrderPayElementsUseCase> _logger;

        private static DateTime BonusSchemeStartTime()
        {
            return new DateTime(2021, 11, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        public UpdateExistingWorkOrderPayElementsUseCase(ITimesheetGateway timesheetGateway, IMapPayElements payElementMapper, IDbSaver dbSaver, ILogger<UpdateExistingWorkOrderPayElementsUseCase> logger)
        {
            _timesheetGateway = timesheetGateway;
            _payElementMapper = payElementMapper;
            _dbSaver = dbSaver;
            _logger = logger;
        }

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            var data = message.EventData;

            if (data.WorkOrderId == null)
            {
                throw new ArgumentNullException("WorkOrderId");
            }

            _logger.LogInformation($"Starting to process update to work order ID: {data.WorkOrderId}");

            if (data.ClosedTime == null)
            {
                throw new ArgumentNullException("ClosedTime");
            }

            if (data.ClosedTime < BonusSchemeStartTime())
            {
                return; // This is a work order from before the restarting of the bonus scheme
            }

            // CloseToBase workOrders should not be counted in bonusCalculation
            if (data.PaymentType != null && data.PaymentType == Domain.PaymentType.CloseToBase)
            {
                _logger.LogInformation($"WorkOrder {data.WorkOrderId} will not be recorded because the paymentType is 'CloseToBase'.");
                return;
            }

            // Get the timesheet based on the operative id and closed time of the work order
            var operativeTimesheet = await _timesheetGateway.GetCurrentTimeSheetForOperative(data.OperativePrn, data.ClosedTime);

            if (operativeTimesheet == null)
            {
                throw new InvalidOperationException($"No operative timesheet found to add work elements to! OperativeID: {data.OperativePrn} ClosedTime: {data.ClosedTime}");
            }

            _logger.LogInformation($"Found timesheet {operativeTimesheet.Id} for {data.WorkOrderId}");

            // Ensure that the pay elements list exists
            operativeTimesheet.PayElements ??= new List<PayElement>();

            // Remove any existing pay elements for the work order
            operativeTimesheet.PayElements.RemoveAll(pe => pe.WorkOrder == data.WorkOrderId);

            // Check the status code for a valid state - only 'Completed' and 'No Access' should be recorded
            if (data.WorkOrderStatusCode == RepairsStatusCodes.Completed || data.WorkOrderStatusCode == RepairsStatusCodes.NoAccess)
            {
                var npe = _payElementMapper.BuildPayElement(message.EventData);
                operativeTimesheet.PayElements.Add(npe);

                _logger.LogInformation($"Added {npe.Value} SMVs to operative {data.OperativePrn} for work order {data.WorkOrderId}!");
            }

            await _dbSaver.SaveChangesAsync();
        }
    }
}
