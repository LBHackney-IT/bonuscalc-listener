using BonusCalcListener.Boundary;
using BonusCalcListener.Infrastructure;
using System;

namespace BonusCalcListener.UseCase
{
    public class PayElementMapper : IMapPayElements
    {
        const int REACTIVE_REPAIRS_TYPE = 301;
        const int OVERTIME_JOB_TYPE = 401;
        const int OUT_OF_HOURS_JOB_TYPE = 501;

        private static string OVERTIME_RATE = Environment.GetEnvironmentVariable("OVERTIME_RATE") ?? "21.60";

        public PayElement BuildPayElement(WorkOrderOperativeSmvData eventData)
        {
            if (!eventData.ClosedTime.HasValue)
                throw new ArgumentException($"Cannot create a pay element with null closed time, WorkOrder: {eventData.WorkOrderId}, operative: {eventData.OperativePrn}");

            if (!eventData.JobPercentage.HasValue || eventData.JobPercentage <= 0)
                throw new ArgumentException($"Cannot have a job percentage of zero or less than zero, WorkOrder: {eventData.WorkOrderId}");

            if (eventData.IsOutOfHours.HasValue && (bool) eventData.IsOutOfHours)
            {
                return BuildOutOfHoursPayElement(eventData);
            }
            else if (eventData.IsOvertime.HasValue && (bool) eventData.IsOvertime)
            {
                return BuildOvertimePayElement(eventData);
            }
            else
            {
                return BuildReactivePayElement(eventData);
            }
        }

        private PayElement BuildReactivePayElement(WorkOrderOperativeSmvData eventData)
        {
            var operativeJobSmvMinutes = 0.0m;
            var operativeJobSmvHours = 0.0m;

            if (eventData.WorkOrderStatusCode == RepairsStatusCodes.Completed)
            {
                operativeJobSmvMinutes = (decimal) (eventData.StandardMinuteValue * eventData.JobPercentage / 100 ?? 0);
                operativeJobSmvHours = operativeJobSmvMinutes / 60;
            }

            return new PayElement
            {
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

        private PayElement BuildOutOfHoursPayElement(WorkOrderOperativeSmvData eventData)
        {
            var operativeCost = 0.0m;

            if (eventData.WorkOrderStatusCode == RepairsStatusCodes.Completed)
            {
                operativeCost = (decimal) (eventData.OperativeCost * eventData.JobPercentage / 100 ?? 0);
            }

            return new PayElement
            {
                PayElementTypeId = OUT_OF_HOURS_JOB_TYPE,
                WorkOrder = eventData.WorkOrderId,
                Address = eventData.Address,
                Comment = eventData.Description,
                ClosedAt = eventData.ClosedTime,
                Monday = 0.0m,
                Tuesday = 0.0m,
                Wednesday = 0.0m,
                Thursday = 0.0m,
                Friday = 0.0m,
                Saturday = 0.0m,
                Sunday = 0.0m,
                Duration = 0.0m,
                Value = operativeCost,
                ReadOnly = true
            };
        }

        private PayElement BuildOvertimePayElement(WorkOrderOperativeSmvData eventData)
        {
            var operativeCost = 0.0m;

            if (eventData.WorkOrderStatusCode == RepairsStatusCodes.Completed)
            {
                operativeCost = Convert.ToDecimal(OVERTIME_RATE);
            }

            return new PayElement
            {
                PayElementTypeId = OVERTIME_JOB_TYPE,
                WorkOrder = eventData.WorkOrderId,
                Address = eventData.Address,
                Comment = eventData.Description,
                ClosedAt = eventData.ClosedTime,
                Monday = 0.0m,
                Tuesday = 0.0m,
                Wednesday = 0.0m,
                Thursday = 0.0m,
                Friday = 0.0m,
                Saturday = 0.0m,
                Sunday = 0.0m,
                Duration = 0.0m,
                Value = operativeCost,
                ReadOnly = true
            };
        }
    }
}
