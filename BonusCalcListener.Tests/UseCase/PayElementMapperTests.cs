using AutoFixture;
using BonusCalcListener.Boundary;
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
        public void ShouldThrowWithInvalidArguments_Closedtime()
        {
            // Arrange
            var timesheet = _fixture.Create<Timesheet>();
            var evtData = _fixture.Build<WorkOrderOperativeSmvData>()
                .Without(e => e.ClosedTime)
                .Create();

            // Act + Assert
            _sut.Invoking(s => s.BuildPayElement(evtData, timesheet))
                .Should().Throw<ArgumentException>()
                .WithMessage($"Cannot create a pay element with null closed time, WorkOrder: {evtData.WorkOrderId}, operative: {evtData.OperativePrn}");
        }

        [Test]
        public void ShouldThrowWithInvalidArguments_JobPercentage()
        {
            // Arrange
            var timesheet = _fixture.Create<Timesheet>();
            var evtData = _fixture.Build<WorkOrderOperativeSmvData>()
                .With(e => e.JobPercentage, -1d)
                .Create();

            // Act + Assert
            _sut.Invoking(s => s.BuildPayElement(evtData, timesheet))
                .Should().Throw<ArgumentException>()
                .WithMessage($"Cannot have a job percentage of zero or less than zero, WorkOrder: {evtData.WorkOrderId}");

        }

        [Test]
        public void ShouldCreatePayElementWithValidArguments()
        {
            // Arrange
            var timesheet = _fixture.Create<Timesheet>();
            var evtData = _fixture.Create<WorkOrderOperativeSmvData>();
            evtData.ClosedTime = new DateTime(2021, 11, 08, 14, 32, 01);

            // Act
            var result = _sut.BuildPayElement(evtData, timesheet);

            // Assert
            result.TimesheetId.Should().Be(timesheet.Id);
            result.Monday.Should().Be((decimal) (evtData.StandardMinuteValue * evtData.JobPercentage) / 60);
            result.Tuesday.Should().Be(0);
            result.Wednesday.Should().Be(0);
            result.Thursday.Should().Be(0);
            result.Friday.Should().Be(0);
            result.Saturday.Should().Be(0);
            result.Sunday.Should().Be(0);
            result.Duration.Should().Be((decimal) (evtData.StandardMinuteValue * evtData.JobPercentage) / 60);
            result.ClosedAt.Should().Be(evtData.ClosedTime);
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
