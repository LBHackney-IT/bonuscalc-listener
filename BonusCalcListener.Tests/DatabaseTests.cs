using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using BonusCalcListener.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace BonusCalcListener.Tests
{
    [TestFixture]
    public class DatabaseTests
    {
        private IDbContextTransaction _transaction;
        protected BonusCalcContext BonusCalcContext { get; private set; }

        [SetUp]
        public async Task RunBeforeAnyTests()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseNpgsql(ConnectionString.TestDatabase())
                .UseSnakeCaseNamingConvention();
            BonusCalcContext = new BonusCalcContext(builder.Options);

            await BonusCalcContext.Database.MigrateAsync();
            _transaction = BonusCalcContext.Database.BeginTransaction();

            await SeedData();
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
            _transaction.Rollback();
            _transaction.Dispose();
        }

        protected async Task SeedData()
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
                IsArchived = false,
                EmailAddress = "test@test.gov.uk"
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

            // Empty trades table for tests
            BonusCalcContext.Trades.RemoveRange(BonusCalcContext.Trades);

            BonusCalcContext.Trades.Add(trade);
            BonusCalcContext.Schemes.Add(scheme);
            BonusCalcContext.Operatives.Add(operative);
            BonusCalcContext.BonusPeriods.Add(bonusPeriod);
            BonusCalcContext.Timesheets.AddRange(timesheets);

            await BonusCalcContext.SaveChangesAsync();
        }
    }
}
