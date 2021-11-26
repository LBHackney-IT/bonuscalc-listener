using BonusCalcListener.Boundary;
using BonusCalcListener.Infrastructure;
using System;

namespace BonusCalcListener.UseCase
{
    public class PayElementMapper : IMapPayElements
    {
        const int REACTIVE_REPAIRS_TYPE = 301;
        const int COMPLETED_STATUS = 50;

        public PayElement BuildPayElement(WorkOrderOperativeSmvData eventData, Timesheet operativeTimesheet)
        {
            var operativeJobSmvMinutes = 0.0m;
            var operativeJobSmvHours = 0.0m;

            if (!eventData.ClosedTime.HasValue) throw new ArgumentException($"Cannot create a pay element with null closed time, WorkOrder: {eventData.WorkOrderId}, operative: {eventData.OperativePrn}");

            if (!eventData.JobPercentage.HasValue || eventData.JobPercentage <= 0) throw new ArgumentException($"Cannot have a job percentage of zero or less than zero, WorkOrder: {eventData.WorkOrderId}");

            if (eventData.WorkOrderStatusCode == COMPLETED_STATUS)
            {
                operativeJobSmvMinutes = (decimal) (eventData.StandardMinuteValue * eventData.JobPercentage / 100 ?? 0);
                operativeJobSmvHours = operativeJobSmvMinutes / 60;
            }

            return new PayElement
            {
                TimesheetId = operativeTimesheet.Id,
                PayElementTypeId = REACTIVE_REPAIRS_TYPE,
                WorkOrder = eventData.WorkOrderId,
                Address = eventData.Address,
                Comment = eventData.Description,
                ClosedAt = eventData.ClosedTime,
                Monday = eventData.ClosedTime.Value.DayOfWeek == DayOfWeek.Monday ? operativeJobSmvHours : 0.0m,
                Tuesday = eventData.ClosedTime.Value.DayOfWeek == DayOfWeek.Tuesday ? operativeJobSmvHours : 0.0m,
                Wednesday = eventData.ClosedTime.Value.DayOfWeek == DayOfWeek.Wednesday ? operativeJobSmvHours : 0.0m,
                Thursday = eventData.ClosedTime.Value.DayOfWeek == DayOfWeek.Thursday ? operativeJobSmvHours : 0.0m,
                Friday = eventData.ClosedTime.Value.DayOfWeek == DayOfWeek.Friday ? operativeJobSmvHours : 0.0m,
                Saturday = eventData.ClosedTime.Value.DayOfWeek == DayOfWeek.Saturday ? operativeJobSmvHours : 0.0m,
                Sunday = eventData.ClosedTime.Value.DayOfWeek == DayOfWeek.Sunday ? operativeJobSmvHours : 0.0m,
                Duration = operativeJobSmvHours,
                Value = operativeJobSmvMinutes,
                ReadOnly = true
            };
        }
    }
}
