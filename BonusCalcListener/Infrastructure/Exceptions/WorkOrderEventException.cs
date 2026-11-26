using BonusCalcListener.Boundary;
using System;
using System.Globalization;

namespace BonusCalcListener.Infrastructure.Exceptions
{
    /// <summary>
    /// Base exception for failures processing a work order event from the repairs API.
    /// Carries enough context to identify the job and replay the message from the DLQ.
    /// </summary>
    public abstract class WorkOrderEventException : Exception
    {
        /// <summary>The SQS message id (as shown in the queue / DLQ).</summary>
        public string MessageId { get; }

        /// <summary>The id of the event as published by the repairs API.</summary>
        public Guid EventId { get; }

        public Guid CorrelationId { get; }
        public string EventType { get; }
        public string WorkOrderId { get; }
        public int? WorkOrderStatusCode { get; }
        public string OperativePrn { get; }
        public DateTime? ClosedTime { get; }

        protected WorkOrderEventException(string reason, EntityEventSns entityEvent, string messageId)
            : base(BuildMessage(reason, entityEvent, messageId))
        {
            MessageId = messageId;
            EventId = entityEvent?.Id ?? Guid.Empty;
            CorrelationId = entityEvent?.CorrelationId ?? Guid.Empty;
            EventType = entityEvent?.EventType;
            WorkOrderId = entityEvent?.EventData?.WorkOrderId;
            WorkOrderStatusCode = entityEvent?.EventData?.WorkOrderStatusCode;
            OperativePrn = entityEvent?.EventData?.OperativePrn;
            ClosedTime = entityEvent?.EventData?.ClosedTime;
        }

        private static string BuildMessage(string reason, EntityEventSns entityEvent, string messageId)
        {
            var data = entityEvent?.EventData;
            var closedTime = data?.ClosedTime?.ToString("o", CultureInfo.InvariantCulture);

            return $"{reason} " +
                   $"WorkOrderId: {data?.WorkOrderId ?? "null"}; " +
                   $"OperativePrn: {data?.OperativePrn ?? "null"}; " +
                   $"ClosedTime: {closedTime ?? "null"}; " +
                   $"StatusCode: {(data == null ? "null" : data.WorkOrderStatusCode.ToString(CultureInfo.InvariantCulture))}; " +
                   $"MessageId: {messageId ?? "null"}; " +
                   $"EventId: {entityEvent?.Id}; " +
                   $"CorrelationId: {entityEvent?.CorrelationId}";
        }
    }
}
