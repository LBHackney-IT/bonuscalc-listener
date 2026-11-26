using BonusCalcListener.Boundary;

namespace BonusCalcListener.Infrastructure.Exceptions
{
    /// <summary>
    /// Thrown when the event received from the repairs API has no ClosedTime.
    /// </summary>
    public class MissingClosedTimeException : WorkOrderEventException
    {
        public MissingClosedTimeException(EntityEventSns entityEvent, string messageId)
            : base("Work order event has no ClosedTime.", entityEvent, messageId)
        {
        }
    }
}
