using BonusCalcListener.Gateway;
using BonusCalcListener.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BonusCalcListener.Tests.Gateway
{
    public class TimesheetGatewayTests : DatabaseTests
    {
        private Mock<ILogger<TimesheetGateway>> _loggerMock;
        private TimesheetGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<TimesheetGateway>>();
            _classUnderTest = new TimesheetGateway(BonusCalcContext, _loggerMock.Object);
        }

        [Test]
        public async Task RetrievesCorrectTimesheetIfClosedInOpenWeek()
        {
            // Arrange
            // Data is now seeded during setup

            // Act
            var closedDate = new DateTime(2021, 10, 19, 11, 0, 0, DateTimeKind.Utc);
            var result = await _classUnderTest.GetCurrentTimeSheetForOperative("123456", closedDate).ConfigureAwait(false);

            // Assert
            result.Id.Should().Be("123456/2021-10-18");
        }

        [Test]
        public async Task RetrievesCorrectTimesheetIfClosedInClosedWeek()
        {
            // Arrange
            // Data is now seeded during setup

            // Act
            var closedDate = new DateTime(2021, 10, 5, 11, 0, 0, DateTimeKind.Utc);
            var result = await _classUnderTest.GetCurrentTimeSheetForOperative("123456", closedDate).ConfigureAwait(false);

            // Assert
            result.Id.Should().Be("123456/2021-10-11");
        }
    }
}
