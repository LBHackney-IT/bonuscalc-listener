using Microsoft.Extensions.DependencyInjection;
using BonusCalcListener.Boundary;
using BonusCalcListener.UseCase.Interfaces;
using System;

namespace BonusCalcListener.Factories
{
    //Chooses most suitable service for message processing, given the message type
    public static class MessageProcessorFactory
    {
        public static IMessageProcessing CreateMessageProcessor(EntityEventSns evt, IServiceProvider serviceProvider)
        {
            if (evt is null) throw new ArgumentNullException(nameof(evt));
            if (serviceProvider is null) throw new ArgumentNullException(nameof(serviceProvider));

            IMessageProcessing processor = null;

            switch (evt.EventType)
            {
                case RepairsEventTypes.WorkOrderUpdatedEvent:
                    processor = serviceProvider.GetService<IUpdateExistingWorkOrderPayElements>();
                    break;

                default:
                    throw new ArgumentException($"Unknown event type: {evt.EventType}");
            }

            return processor;
        }
    }
}
