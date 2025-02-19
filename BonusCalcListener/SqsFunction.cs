using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.XRay.Recorder.Core.Internal.Entities;
using BonusCalcListener.Boundary;
using BonusCalcListener.Factories;
using BonusCalcListener.Gateway;
using BonusCalcListener.Gateway.Interfaces;
using BonusCalcListener.Infrastructure;
using BonusCalcListener.UseCase;
using BonusCalcListener.UseCase.Interfaces;
using Hackney.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BonusCalcListener
{
    /// <summary>
    /// Lambda function triggered by an SQS message
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SqsFunction : BaseFunction
    {
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public SqsFunction()
        { }

        /// <summary>
        /// Use this method to perform any DI configuration required
        /// </summary>
        /// <param name="services"></param>
        protected override void ConfigureServices(IServiceCollection services)
        {

            services.AddHttpClient();
            services.AddScoped<IUpdateExistingWorkOrderPayElements, UpdateExistingWorkOrderPayElementsUseCase>();
            // services.AddScoped<IOperativesGateway, OperativesGateway>();
            services.AddScoped<ITimesheetGateway, TimesheetGateway>();
            services.AddScoped<IMapPayElements, PayElementMapper>();
            services.AddScoped<IDbSaver, DbSaver>();


            // TODO: replace with Postgres services.AddScoped<IDbEntityGateway, DynamoDbEntityGateway>();

            base.ConfigureServices(services);
        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            // Do this in parallel???
            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Method called to process every distinct message received.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [LogCall(LogLevel.Information)]
        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"Processing message {message.MessageId}");
            var entityEvent = JsonSerializer.Deserialize<EntityEventSns>(message.Body, _jsonOptions);
            var traceUsingXray = message.Attributes.TryGetValue("AWSTraceHeader", out string traceHeader);

            using (Logger.BeginScope("CorrelationId: {CorrelationId}", traceUsingXray ? TraceHeader.FromString(traceHeader).RootTraceId : Guid.NewGuid().ToString()))
            {
                try
                {
                    var processor = MessageProcessorFactory.CreateMessageProcessor(entityEvent, ServiceProvider);

                    if (processor != null)
                    {
                        Logger.LogInformation($"Found a processor for message {message.MessageId}, type: {entityEvent.EventType}");
                        await processor.ProcessMessageAsync(entityEvent).ConfigureAwait(false);
                    }
                    else
                    {
                        Logger.LogWarning($"Could not find a processor for message {message.MessageId} (type: {entityEvent.EventType} version: {entityEvent.Version}) so it will be ignored");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Exception processing message id: {message.MessageId}; type: {entityEvent.EventType}; entity id: {entityEvent.EntityId}");
                    throw; // AWS will handle retry/moving to the dead letter queue
                }
            }
        }
    }
}
