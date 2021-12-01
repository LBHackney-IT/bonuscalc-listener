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
            await SeedData();

            // Act
            var closedDate = new DateTime(2021, 10, 19, 11, 0, 0, DateTimeKind.Utc);
            var result = await _classUnderTest.GetCurrentTimeSheetForOperative("123456", closedDate);

            // Assert
            result.Id.Should().Be("123456/2021-10-18");
        }

        [Test]
        public async Task RetrievesCorrectTimesheetIfClosedInClosedWeek()
        {
            // Arrange
            await SeedData();

            // Act
            var closedDate = new DateTime(2021, 10, 5, 11, 0, 0, DateTimeKind.Utc);
            var result = await _classUnderTest.GetCurrentTimeSheetForOperative("123456", closedDate);

            // Assert
            result.Id.Should().Be("123456/2021-10-11");
        }

        private async Task SeedData()
        {
            var trade = new Trade
            {
                Id = "EL",
                Description = "Electrician"
            };

            var scheme = new Scheme
            {
                Id = 1,
                Type = "SMV",
                Description = "Reactive",
                ConversionFactor = 1.0M
            };

            var operative = new Operative
            {
                Id = "123456",
                Name = "An Operative",
                Trade = trade,
                Scheme = scheme,
                Section = "H3007",
                SalaryBand = 5,
                FixedBand = false,
                IsArchived = false
            };

            var bonusPeriod = new BonusPeriod
            {
                Id = "2021-08-02",
                StartAt = new DateTime(2021, 8, 1, 23, 0, 0, DateTimeKind.Utc),
                Year = 2020,
                Number = 3,
                ClosedAt = null,
                Weeks = new List<Week>()
                {
                    new Week
                    {
                        Id = "2021-10-04",
                        StartAt = new DateTime(2021, 10, 3, 23, 0, 0, DateTimeKind.Utc),
                        Number = 10,
                        ClosedAt = new DateTime(2021, 10, 11, 16, 0, 0, DateTimeKind.Utc)
                    },
                    new Week
                    {
                        Id = "2021-10-11",
                        StartAt = new DateTime(2021, 10, 10, 23, 0, 0, DateTimeKind.Utc),
                        Number = 11,
                        ClosedAt = null
                    },
                    new Week
                    {
                        Id = "2021-10-18",
                        StartAt = new DateTime(2021, 10, 17, 23, 0, 0, DateTimeKind.Utc),
                        Number = 12,
                        ClosedAt = null
                    },
                    new Week
                    {
                        Id = "2021-10-25",
                        StartAt = new DateTime(2021, 10, 24, 23, 0, 0, DateTimeKind.Utc),
                        Number = 13,
                        ClosedAt = null
                    }
                }
            };

            var timesheets = new List<Timesheet>()
            {
                new Timesheet
                {
                    Id = "123456/2021-10-04",
                    OperativeId = "123456",
                    WeekId = "2021-10-04"
                },
                new Timesheet
                {
                    Id = "123456/2021-10-11",
                    OperativeId = "123456",
                    WeekId = "2021-10-11"
                },
                new Timesheet
                {
                    Id = "123456/2021-10-18",
                    OperativeId = "123456",
                    WeekId = "2021-10-18"
                },
                new Timesheet
                {
                    Id = "123456/2021-10-25",
                    OperativeId = "123456",
                    WeekId = "2021-10-25"
                },
            };

            await BonusCalcContext.Trades.AddAsync(trade);
            await BonusCalcContext.Schemes.AddAsync(scheme);
            await BonusCalcContext.Operatives.AddAsync(operative);
            await BonusCalcContext.BonusPeriods.AddAsync(bonusPeriod);
            await BonusCalcContext.Timesheets.AddRangeAsync(timesheets);
            await BonusCalcContext.SaveChangesAsync();
        }
    }
}
