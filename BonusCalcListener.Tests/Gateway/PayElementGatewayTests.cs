using BonusCalcListener.Gateway;
using BonusCalcListener.Infrastructure;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BonusCalcListener.Tests.Gateway
{
    public class PayElementGatewayTests : DatabaseTests
    {
        private PayElementGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new PayElementGateway(BonusCalcContext);
        }

        [Test]
        public async Task RetrievesCorrectPayElementsOnWorkOrderId()
        {
            // Arrange
            var operative = await AddOperative();
            var week = await AddWeek();
            await AddTimesheet(operative, week);

            // Act
            var result = await _classUnderTest.GetPayElementsByWorkOrderId("12824", 301);

            // Assert
            result.Sum(x => x.Wednesday).Should().Be(7.0m);
        }

        [Test]
        public async Task DoesNotRetrieveIncorrectPayElemnentTypesByWorkOrderId()
        {
            // Arrange
            var operative = await AddOperative();
            var week = await AddWeek();
            await AddTimesheet(operative, week);

            // Act
            var result = await _classUnderTest.GetPayElementsByWorkOrderId("12824", 302);

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public async Task ShouldNotRetrieveNonExistantPayElements()
        {
            // Act
            var result = await _classUnderTest.GetPayElementsByWorkOrderId("Non-existant work order", 301);

            // Assert
            result.Should().BeEmpty();
        }

        private async Task<Operative> AddOperative()
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

            await BonusCalcContext.Trades.AddAsync(trade);
            await BonusCalcContext.Schemes.AddAsync(scheme);
            await BonusCalcContext.Operatives.AddAsync(operative);
            await BonusCalcContext.SaveChangesAsync();

            return operative;
        }

        private async Task<Week> AddWeek()
        {
            var bonusPeriod = new BonusPeriod
            {
                Id = "2021-08-02",
                StartAt = new DateTime(2021, 8, 1, 23, 0, 0, DateTimeKind.Utc),
                Year = 2020,
                Number = 3,
                ClosedAt = null
            };

            var week = new Week
            {
                Id = "2021-10-18",
                BonusPeriod = bonusPeriod,
                StartAt = new DateTime(2021, 10, 17, 23, 0, 0, DateTimeKind.Utc),
                Number = 12,
                ClosedAt = null
            };

            await BonusCalcContext.BonusPeriods.AddAsync(bonusPeriod);
            await BonusCalcContext.Weeks.AddAsync(week);
            await BonusCalcContext.SaveChangesAsync();

            return week;
        }

        private async Task<Timesheet> AddTimesheet(Operative operative, Week week)
        {
            var nonProductivePayElementType = new PayElementType
            {
                Id = 101,
                Description = "Dayworks",
                PayAtBand = false,
                Paid = true,
                Adjustment = false,
                Productive = false,
                NonProductive = true
            };

            var productivePayElementType = new PayElementType
            {
                Id = 301,
                Description = "Reactive",
                PayAtBand = true,
                Paid = true,
                Adjustment = false,
                Productive = true,
                NonProductive = false
            };

            var timesheet = new Timesheet
            {
                Id = "123456/2021-10-18",
                Week = week,
                Operative = operative,
                PayElements = new List<PayElement>()
                {
                    new PayElement
                    {
                        PayElementType = nonProductivePayElementType,
                        Monday = 1.0M,
                        Tuesday = 1.0M,
                        Wednesday = 1.0M,
                        Thursday = 1.0M,
                        Friday = 0.0M,
                        Duration = 4.0M,
                        Value = 348.0M,
                        ReadOnly = false,
                        WorkOrder = "12821"
                    },
                    new PayElement
                    {
                        PayElementType = productivePayElementType,
                        Monday = 0.0M,
                        Tuesday = 7.0M,
                        Wednesday = 0.0M,
                        Thursday = 0.0M,
                        Friday = 0.0M,
                        Duration = 7.0M,
                        Value = 0.0M,
                        ReadOnly = true,
                        WorkOrder = "12824"
                    },
                    new PayElement
                    {
                        PayElementType = productivePayElementType,
                        Monday = 0.0M,
                        Tuesday = 0.0M,
                        Wednesday = 7.0M,
                        Thursday = 0.0M,
                        Friday = 0.0M,
                        Duration = 7.0M,
                        Value = 0.0M,
                        ReadOnly = true,
                        WorkOrder = "12824"
                    },
                    new PayElement
                    {
                        PayElementType = productivePayElementType,
                        Monday = 0.0M,
                        Tuesday = 0.0M,
                        Wednesday = 7.0M,
                        Thursday = 0.0M,
                        Friday = 0.0M,
                        Duration = 7.0M,
                        Value = 0.0M,
                        ReadOnly = true,
                        WorkOrder = "OthrW"
                    }
                }
            };

            await BonusCalcContext.PayElementTypes.AddAsync(nonProductivePayElementType);
            await BonusCalcContext.PayElementTypes.AddAsync(productivePayElementType);
            await BonusCalcContext.Timesheets.AddAsync(timesheet);
            await BonusCalcContext.SaveChangesAsync();

            return timesheet;
        }
    }
}
