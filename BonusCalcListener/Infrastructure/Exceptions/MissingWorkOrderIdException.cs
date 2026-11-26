using BonusCalcListener.Boundary;

namespace BonusCalcListener.Infrastructure.Exceptions
{
    /// <summary>
    /// Thrown when the event received from the repairs API has no WorkOrderId.
    /// </summary>
    public class MissingWorkOrderIdException : WorkOrderEventException
    {
        public MissingWorkOrderIdException(EntityEventSns entityEvent, string messageId)
            : base("Work order event has no WorkOrderId.", entityEvent, messageId)
        {
        }
    }
}
