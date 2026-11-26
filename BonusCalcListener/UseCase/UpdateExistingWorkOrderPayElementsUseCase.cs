using BonusCalcListener.Boundary;
using BonusCalcListener.Gateway.Interfaces;
using BonusCalcListener.Infrastructure;
using BonusCalcListener.Infrastructure.Exceptions;
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
        private readonly IOperativesGateway _operativesGateway;

        private static DateTime BonusSchemeStartTime()
        {
            return new DateTime(2021, 11, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        public UpdateExistingWorkOrderPayElementsUseCase(
            ITimesheetGateway timesheetGateway,
            IMapPayElements payElementMapper,
            IDbSaver dbSaver,
            ILogger<UpdateExistingWorkOrderPayElementsUseCase> logger)
        {
            _timesheetGateway = timesheetGateway;
            _payElementMapper = payElementMapper;
            _dbSaver = dbSaver;
            _logger = logger;
        }

        public async Task ProcessMessageAsync(EntityEventSns message, string messageId)
        {
            var data = message.EventData;

            if (data.WorkOrderId == null)
            {
                _logger.LogError("Work order event has no WorkOrderId. MessageId: {MessageId}; EventId: {EventId}; OperativePrn: {OperativePrn}; ClosedTime: {ClosedTime}",
                    messageId, message.Id, data.OperativePrn, data.ClosedTime);
                throw new MissingWorkOrderIdException(message, messageId);
            }

            _logger.LogInformation("Starting to process update to work order ID: {WorkOrderId}", data.WorkOrderId);

            if (data.ClosedTime == null)
            {
                _logger.LogError("Work order event has no ClosedTime. WorkOrderId: {WorkOrderId}; MessageId: {MessageId}; EventId: {EventId}; OperativePrn: {OperativePrn}",
                    data.WorkOrderId, messageId, message.Id, data.OperativePrn);
                throw new MissingClosedTimeException(message, messageId);
            }

            if (data.ClosedTime < BonusSchemeStartTime())
            {
                _logger.LogInformation("WorkOrder {WorkOrderId} will not be recorded because it was closed before the bonus scheme start time. ClosedTime: {ClosedTime}; BonusSchemeStartTime: {BonusSchemeStartTime}; OperativePrn: {OperativePrn}; MessageId: {MessageId}",
                    data.WorkOrderId, data.ClosedTime, BonusSchemeStartTime(), data.OperativePrn, messageId);
                return; // This is a work order from before the restarting of the bonus scheme
            }

            // CloseToBase workOrders should not be counted in bonusCalculation
            if (data.PaymentType != null && data.PaymentType == Domain.PaymentType.CloseToBase)
            {
                _logger.LogInformation("WorkOrder {WorkOrderId} will not be recorded because the paymentType is 'CloseToBase'.", data.WorkOrderId);
                return;
            }

            // Ignore agency workers (those with a payroll number starting with 'T')
            if (data.OperativePrn.StartsWith("T"))
            {
                _logger.LogInformation("WorkOrder {WorkOrderId} will not be recorded because the operative is agency: {OperativePrn}", data.WorkOrderId, data.OperativePrn);
                return;
            }

            /*If the operative is a Hackney operative, but is archived, unarchive them immediately (they are still receiving work)
            * N.B if they have been archived for a while, it may be the case that the timesheet search will fail. However, new timesheets will generate
            * thus preventing a persistent issue for the operative
            */
            // await _operativesGateway.ActivateOperative(data.OperativePrn);

            // Get the timesheet based on the operative id and closed time of the work order
            var operativeTimesheet = await _timesheetGateway.GetCurrentTimeSheetForOperative(data.OperativePrn, data.ClosedTime);

            if (operativeTimesheet == null)
            {
                _logger.LogError("No operative timesheet found to add work elements to. WorkOrderId: {WorkOrderId}; OperativePrn: {OperativePrn}; ClosedTime: {ClosedTime}; StatusCode: {StatusCode}; MessageId: {MessageId}; EventId: {EventId}",
                    data.WorkOrderId, data.OperativePrn, data.ClosedTime, data.WorkOrderStatusCode, messageId, message.Id);
                throw new OperativeTimesheetNotFoundException(message, messageId);
            }

            _logger.LogInformation("Found timesheet {TimesheetId} for {WorkOrderId}", operativeTimesheet.Id, data.WorkOrderId);

            // Ensure that the pay elements list exists
            operativeTimesheet.PayElements ??= new List<PayElement>();

            // Remove any existing pay elements for the work order
            operativeTimesheet.PayElements.RemoveAll(pe => pe.WorkOrder == data.WorkOrderId);

            // Check the status code for a valid state - only 'Completed' and 'No Access' should be recorded
            if (data.WorkOrderStatusCode == RepairsStatusCodes.Completed || data.WorkOrderStatusCode == RepairsStatusCodes.NoAccess)
            {
                var npe = _payElementMapper.BuildPayElement(message.EventData);
                operativeTimesheet.PayElements.Add(npe);

                _logger.LogInformation("Added {Smv} SMVs to operative {OperativePrn} for work order {WorkOrderId}!", npe.Value, data.OperativePrn, data.WorkOrderId);
            }

            await _dbSaver.SaveChangesAsync();
        }
    }
}
