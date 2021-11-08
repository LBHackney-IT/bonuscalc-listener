using AutoFixture;
using BonusCalcListener.Boundary;
using BonusCalcListener.Factories;
using BonusCalcListener.UseCase.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace BonusCalcListener.Tests.Factories
{
    [TestFixture]
    public class MessageProcessorFactoryTests
    {
        private readonly Fixture _fixture = new Fixture();
        private EntityEventSns _evt;
        private Mock<IServiceProvider> _mockServiceProvider;

        [SetUp]
        public void SetupTests()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();

            _evt = ConstructEvent(RepairsEventTypes.WorkOrderCompletedEvent);
        }

        [Test]
        public void FactoryArgumentsAreCorrectlyValidated_ServiceProvider()
        {
            Action createdWithNullSp = () => MessageProcessorFactory.CreateMessageProcessor(_evt, null);
            createdWithNullSp.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void FactoryArgumentsAreCorrectlyValidated_Event()
        {
            Action createdWithNullEvent = () => MessageProcessorFactory.CreateMessageProcessor(null, _mockServiceProvider.Object);
            createdWithNullEvent.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void FactoryThrowsForUnknownEventType()
        {
            _evt = ConstructEvent("UnknownEvent");

            Action act = () => MessageProcessorFactory.CreateMessageProcessor(_evt, _mockServiceProvider.Object);
            act.Should().Throw<ArgumentException>().WithMessage($"Unknown event type: {_evt.EventType}");
            _mockServiceProvider.Verify(x => x.GetService(typeof(IAddNewWorkOrderPayElements)), Times.Never);
        }

        [Test]
        public void FactoryChoosesCorrectServiceForValidCompletedMessage()
        {
            _evt = ConstructEvent(RepairsEventTypes.WorkOrderCompletedEvent);
            TestMessageProcessingCreation<IAddNewWorkOrderPayElements>(_evt);
        }

        [Test]
        public void FactoryChoosesCorrectServiceForValidUpdatedMessage()
        {
            _evt = ConstructEvent(RepairsEventTypes.WorkOrderUpdatedEvent);
            TestMessageProcessingCreation<IUpdateExistingWorkOrderPayElements>(_evt);
        }

        private EntityEventSns ConstructEvent(string evtType, string version = RepairsEventVersions.V1)
        {
            return _fixture.Build<EntityEventSns>()
                .With(e => e.EventType, evtType)
                .With(e => e.Version, version)
                .Create();
        }

        private void TestMessageProcessingCreation<T>(EntityEventSns eventObj) where T : class, IMessageProcessing
        {
            var mockProcessor = new Mock<T>();
            _mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(mockProcessor.Object);

            var result = MessageProcessorFactory.CreateMessageProcessor(eventObj, _mockServiceProvider.Object);
            result.Should().NotBeNull();
            _mockServiceProvider.Verify(x => x.GetService(typeof(T)), Times.Once);
        }

    }
}
