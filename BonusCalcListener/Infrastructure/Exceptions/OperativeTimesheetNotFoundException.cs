using BonusCalcListener.Boundary;

namespace BonusCalcListener.Infrastructure.Exceptions
{
    /// <summary>
    /// Thrown when the operative has no timesheet for the week the work order was closed in. Once the timesheet exists the message can be redriven from the DLQ.
    /// </summary>
    public class OperativeTimesheetNotFoundException : WorkOrderEventException
    {
        public OperativeTimesheetNotFoundException(EntityEventSns entityEvent, string messageId)
            : base("No operative timesheet found to add work elements to!", entityEvent, messageId)
        {
        }
    }
}
