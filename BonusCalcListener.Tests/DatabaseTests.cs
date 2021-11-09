using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using BonusCalcListener.Infrastructure;

namespace BonusCalcListener.Tests
{
    [TestFixture]
    public class DatabaseTests
    {
        private IDbContextTransaction _transaction;
        protected BonusCalcContext BonusCalcContext { get; private set; }

        [SetUp]
        public void RunBeforeAnyTests()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseNpgsql(ConnectionString.TestDatabase())
                .UseSnakeCaseNamingConvention();
            BonusCalcContext = new BonusCalcContext(builder.Options);

            BonusCalcContext.Database.Migrate();
            _transaction = BonusCalcContext.Database.BeginTransaction();

            // Empty trades table for tests
            BonusCalcContext.Trades.RemoveRange(BonusCalcContext.Trades);
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
            _transaction.Rollback();
            _transaction.Dispose();
        }
    }
}
