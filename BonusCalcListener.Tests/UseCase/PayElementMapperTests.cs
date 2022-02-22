using AutoFixture;
using BonusCalcListener.Boundary;
using BonusCalcListener.Domain;
using BonusCalcListener.Infrastructure;
using BonusCalcListener.UseCase;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BonusCalcListener.Tests.UseCase
{
    [TestFixture]
    public class PayElementMapperTests
    {
        private PayElementMapper _sut;
        private readonly Fixture _fixture = new Fixture();

        [Test]
        public void ShouldThrowWithInvalidArguments_ClosedTime()
        {
            // Arrange
            var evtData = _fixture.Build<WorkOrderOperativeSmvData>()
                .Without(e => e.ClosedTime)
                .Create();

            // Act + Assert
            _sut.Invoking(s => s.BuildPayElement(evtData))
                .Should().Throw<ArgumentException>()
                .WithMessage($"Cannot create a pay element with null closed time, WorkOrder: {evtData.WorkOrderId}, operative: {evtData.OperativePrn}");
        }

        [Test]
        public void ShouldThrowWithInvalidArguments_JobPercentage()
        {
            // Arrange
            var evtData = _fixture.Build<WorkOrderOperativeSmvData>()
                .With(e => e.JobPercentage, -1d)
                .Create();

            // Act + Assert
            _sut.Invoking(s => s.BuildPayElement(evtData))
                .Should().Throw<ArgumentException>()
                .WithMessage($"Cannot have a job percentage of zero or less than zero, WorkOrder: {evtData.WorkOrderId}");

        }

        [Test]
        public void ShouldCreatePayElementWithValidArguments()
        {
            // Arrange
            var evtData = _fixture.Create<WorkOrderOperativeSmvData>();
            evtData.WorkOrderStatusCode = 50;
            evtData.StandardMinuteValue = 300;
            evtData.JobPercentage = 50;
            evtData.ClosedTime = new DateTime(2021, 11, 08, 14, 32, 01);
            evtData.IsOutOfHours = false;
            evtData.OperativeCost = 0;
            evtData.PaymentType = null;

            // Act
            var result = _sut.BuildPayElement(evtData);

            // Assert
            result.PayElementTypeId.Should().Be(PayElementTypeIds.Reactive);
            result.WorkOrder.Should().Be(evtData.WorkOrderId);
            result.Address.Should().Be(evtData.Address);
            result.Comment.Should().Be(evtData.Description);
            result.ClosedAt.Should().Be(evtData.ClosedTime);
            result.Monday.Should().Be(2.5m);
            result.Tuesday.Should().Be(0.0m);
            result.Wednesday.Should().Be(0.0m);
            result.Thursday.Should().Be(0.0m);
            result.Friday.Should().Be(0.0m);
            result.Saturday.Should().Be(0.0m);
            result.Sunday.Should().Be(0.0m);
            result.Duration.Should().Be(2.5m);
            result.Value.Should().Be(150.0m);
        }

        [Test]
        public void ShouldCreatePayElementWithZeroValue()
        {
            // Arrange
            var evtData = _fixture.Create<WorkOrderOperativeSmvData>();
            evtData.WorkOrderStatusCode = 1000;
            evtData.StandardMinuteValue = 300;
            evtData.JobPercentage = 50;
            evtData.ClosedTime = new DateTime(2021, 11, 08, 14, 32, 01);
            evtData.IsOutOfHours = false;
            evtData.OperativeCost = 0;
            evtData.PaymentType = null;

            // Act
            var result = _sut.BuildPayElement(evtData);

            // Assert
            result.PayElementTypeId.Should().Be(PayElementTypeIds.Reactive);
            result.WorkOrder.Should().Be(evtData.WorkOrderId);
            result.Address.Should().Be(evtData.Address);
            result.Comment.Should().Be(evtData.Description);
            result.ClosedAt.Should().Be(evtData.ClosedTime);
            result.Monday.Should().Be(0.0m);
            result.Tuesday.Should().Be(0.0m);
            result.Wednesday.Should().Be(0.0m);
            result.Thursday.Should().Be(0.0m);
            result.Friday.Should().Be(0.0m);
            result.Saturday.Should().Be(0.0m);
            result.Sunday.Should().Be(0.0m);
            result.Duration.Should().Be(0.0m);
            result.Value.Should().Be(0.0m);
        }

        [Test]
        public void ShouldCreateOutOfHoursPayElementWithValidArguments()
        {
            // Arrange
            var evtData = _fixture.Create<WorkOrderOperativeSmvData>();
            evtData.WorkOrderStatusCode = 50;
            evtData.StandardMinuteValue = 0;
            evtData.JobPercentage = 50;
            evtData.ClosedTime = new DateTime(2021, 11, 08, 14, 32, 01);
            evtData.IsOutOfHours = true;
            evtData.OperativeCost = 68;
            evtData.PaymentType = null;

            // Act
            var result = _sut.BuildPayElement(evtData);

            // Assert
            result.PayElementTypeId.Should().Be(PayElementTypeIds.OutOfHours);
            result.WorkOrder.Should().Be(evtData.WorkOrderId);
            result.Address.Should().Be(evtData.Address);
            result.Comment.Should().Be(evtData.Description);
            result.ClosedAt.Should().Be(evtData.ClosedTime);
            result.Monday.Should().Be(0.0m);
            result.Tuesday.Should().Be(0.0m);
            result.Wednesday.Should().Be(0.0m);
            result.Thursday.Should().Be(0.0m);
            result.Friday.Should().Be(0.0m);
            result.Saturday.Should().Be(0.0m);
            result.Sunday.Should().Be(0.0m);
            result.Duration.Should().Be(0.0m);
            result.Value.Should().Be(34.0m);
        }

        [Test]
        public void ShouldCreateOutOfHoursPayElementWithZeroValue()
        {
            // Arrange
            var evtData = _fixture.Create<WorkOrderOperativeSmvData>();
            evtData.WorkOrderStatusCode = 1000;
            evtData.StandardMinuteValue = 0;
            evtData.JobPercentage = 50;
            evtData.ClosedTime = new DateTime(2021, 11, 08, 14, 32, 01);
            evtData.IsOutOfHours = true;
            evtData.OperativeCost = 68;
            evtData.PaymentType = null;

            // Act
            var result = _sut.BuildPayElement(evtData);

            // Assert
            result.PayElementTypeId.Should().Be(PayElementTypeIds.OutOfHours);
            result.WorkOrder.Should().Be(evtData.WorkOrderId);
            result.Address.Should().Be(evtData.Address);
            result.Comment.Should().Be(evtData.Description);
            result.ClosedAt.Should().Be(evtData.ClosedTime);
            result.Monday.Should().Be(0.0m);
            result.Tuesday.Should().Be(0.0m);
            result.Wednesday.Should().Be(0.0m);
            result.Thursday.Should().Be(0.0m);
            result.Friday.Should().Be(0.0m);
            result.Saturday.Should().Be(0.0m);
            result.Sunday.Should().Be(0.0m);
            result.Duration.Should().Be(0.0m);
            result.Value.Should().Be(0.0m);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WhenPaymentTypeIsOvertime_ShouldCreateOvertimePayElementWithValidArguments(bool isOvertime)
        {
            // Arrange
            var evtData = _fixture.Create<WorkOrderOperativeSmvData>();
            evtData.WorkOrderStatusCode = 50;
            evtData.StandardMinuteValue = 0;
            evtData.JobPercentage = 50;
            evtData.ClosedTime = new DateTime(2021, 11, 08, 14, 32, 01);
            evtData.IsOutOfHours = false;
            evtData.OperativeCost = 0;
            evtData.PaymentType = PaymentType.Overtime;

            // Act
            var result = _sut.BuildPayElement(evtData);

            // Assert
            result.PayElementTypeId.Should().Be(PayElementTypeIds.Overtime);
            result.WorkOrder.Should().Be(evtData.WorkOrderId);
            result.Address.Should().Be(evtData.Address);
            result.Comment.Should().Be(evtData.Description);
            result.ClosedAt.Should().Be(evtData.ClosedTime);
            result.Monday.Should().Be(0.0m);
            result.Tuesday.Should().Be(0.0m);
            result.Wednesday.Should().Be(0.0m);
            result.Thursday.Should().Be(0.0m);
            result.Friday.Should().Be(0.0m);
            result.Saturday.Should().Be(0.0m);
            result.Sunday.Should().Be(0.0m);
            result.Duration.Should().Be(0.0m);
            result.Value.Should().Be(21.6m);
        }



        [TestCase(true)]
        [TestCase(false)]
        public void WhenPaymentTypeIsOvertime_AndNoAccess_ShouldCreateOvertimePayElementWithZeroValue(bool isOvertime)
        {
            // Arrange
            var evtData = _fixture.Create<WorkOrderOperativeSmvData>();
            evtData.WorkOrderStatusCode = 1000;
            evtData.StandardMinuteValue = 0;
            evtData.JobPercentage = 50;
            evtData.ClosedTime = new DateTime(2021, 11, 08, 14, 32, 01);
            evtData.IsOutOfHours = false;
            evtData.OperativeCost = 0;
            evtData.PaymentType = PaymentType.Overtime;

            // Act
            var result = _sut.BuildPayElement(evtData);

            // Assert
            result.PayElementTypeId.Should().Be(PayElementTypeIds.Overtime);
            result.WorkOrder.Should().Be(evtData.WorkOrderId);
            result.Address.Should().Be(evtData.Address);
            result.Comment.Should().Be(evtData.Description);
            result.ClosedAt.Should().Be(evtData.ClosedTime);
            result.Monday.Should().Be(0.0m);
            result.Tuesday.Should().Be(0.0m);
            result.Wednesday.Should().Be(0.0m);
            result.Thursday.Should().Be(0.0m);
            result.Friday.Should().Be(0.0m);
            result.Saturday.Should().Be(0.0m);
            result.Sunday.Should().Be(0.0m);
            result.Duration.Should().Be(0.0m);
            result.Value.Should().Be(0.0m);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WhenPaymentTypeIsNotOvertime_ShouldCreateReactivePayElement(bool isOvertime)
        {
            // Arrange
            var evtData = _fixture.Create<WorkOrderOperativeSmvData>();
            evtData.WorkOrderStatusCode = 50;
            evtData.StandardMinuteValue = 0;
            evtData.JobPercentage = 50;
            evtData.ClosedTime = new DateTime(2021, 11, 08, 14, 32, 01);
            evtData.IsOutOfHours = false;
            evtData.OperativeCost = 0;
            evtData.PaymentType = PaymentType.Bonus;

            // Act
            var result = _sut.BuildPayElement(evtData);

            // Assert
            result.PayElementTypeId.Should().Be(PayElementTypeIds.Reactive);
        }


        [SetUp]
        public void Setup()
        {
            // Allow circular refs during object generation
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new PayElementMapper();
        }
    }
}
