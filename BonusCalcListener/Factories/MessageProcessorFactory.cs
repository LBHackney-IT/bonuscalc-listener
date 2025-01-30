using Microsoft.Extensions.DependencyInjection;
using BonusCalcListener.Boundary;
using BonusCalcListener.UseCase.Interfaces;
using System;

namespace BonusCalcListener.Factories
{
    //Chooses most suitable service for message processing, given the message type
    public static class MessageProcessorFactory
    {
        public static IMessageProcessing CreateMessageProcessor(EntityEventSns entityEvent, IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(entityEvent);
            ArgumentNullException.ThrowIfNull(serviceProvider);

            switch (entityEvent.EventType)
            {
                case RepairsEventTypes.WorkOrderUpdatedEvent:
                    return serviceProvider.GetService<IUpdateExistingWorkOrderPayElements>();


                default:
                    throw new ArgumentException($"Unknown event type: {entityEvent.EventType}");
            }


        }
    }
}
