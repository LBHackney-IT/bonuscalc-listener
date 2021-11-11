using BonusCalcListener.Boundary;
using BonusCalcListener.Infrastructure;
using System;

namespace BonusCalcListener.UseCase
{
    public class PayElementMapper : IMapPayElements
    {
        const int REACTIVE_REPAIRS_TYPE = 301;
        public PayElement BuildPayElement(WorkOrderOperativeSmvData eventData, Timesheet operativeTimesheet)
        {
            if (!eventData.ClosedTime.HasValue) throw new ArgumentException($"Cannot create a pay element with null closed time, WorkOrder: {eventData.WorkOrderId}, operative: {eventData.OperativeId}");

            if (!eventData.JobPercentage.HasValue || eventData.JobPercentage <= 0) throw new ArgumentException($"Cannot have a job percentage of zero or less than zero, WorkOrder: {eventData.WorkOrderId}");

            var operativeJobSmvHours = (decimal) (eventData.StandardMinuteValue * eventData.JobPercentage ?? 0) / 60;

            return new PayElement
            {
                TimesheetId = operativeTimesheet.Id,
                PayElementTypeId = REACTIVE_REPAIRS_TYPE,
                Address = eventData.Address,
                WorkOrder = eventData.WorkOrderId,
                Duration = operativeJobSmvHours,
                ReadOnly = true,
                Monday = eventData.ClosedTime?.DayOfWeek == DayOfWeek.Monday ? operativeJobSmvHours : 0.0m,
                Tuesday = eventData.ClosedTime?.DayOfWeek == DayOfWeek.Tuesday ? operativeJobSmvHours : 0.0m,
                Wednesday = eventData.ClosedTime?.DayOfWeek == DayOfWeek.Wednesday ? operativeJobSmvHours : 0.0m,
                Thursday = eventData.ClosedTime?.DayOfWeek == DayOfWeek.Thursday ? operativeJobSmvHours : 0.0m,
                Friday = eventData.ClosedTime?.DayOfWeek == DayOfWeek.Friday ? operativeJobSmvHours : 0.0m,
                Saturday = eventData.ClosedTime?.DayOfWeek == DayOfWeek.Saturday ? operativeJobSmvHours : 0.0m,
                Sunday = eventData.ClosedTime?.DayOfWeek == DayOfWeek.Sunday ? operativeJobSmvHours : 0.0m,
                Comment = eventData.Description
            };
        }
    }
}
