using BonusCalcListener.Gateway;
using BonusCalcListener.Gateway.Interfaces;
using BonusCalcListener.Infrastructure;
using BonusCalcListener.UseCase;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace BonusCalcListener.Tests.UseCase
{
    [TestFixture]
    public class UpdateExistingWorkElementsUseCaseTests
    {
        private UpdateExistingWorkOrderPayElementsUseCase _sut;
        private Mock<IPayElementGateway> _mockPayElementGateway;
        private Mock<ITimesheetGateway> _mockTimesheetGateway;
        private Mock<IMapPayElements> _mockPayElementsMapper;
        private Mock<IDbSaver> _mockDbSaver;
        private ILogger<UpdateExistingWorkOrderPayElementsUseCase> _logger;

        [SetUp]
        public void Setup()
        {
            _mockPayElementGateway = new Mock<IPayElementGateway>();
            _mockTimesheetGateway = new Mock<ITimesheetGateway>();
            _mockPayElementsMapper = new Mock<IMapPayElements>();
            _mockDbSaver = new Mock<IDbSaver>();
            _logger = new NullLogger<UpdateExistingWorkOrderPayElementsUseCase>();

            _sut = new UpdateExistingWorkOrderPayElementsUseCase(
                _mockPayElementGateway.Object,
                _mockTimesheetGateway.Object,
                _mockPayElementsMapper.Object,
                _mockDbSaver.Object,
                _logger
                );
        }

        [Test]
        public void ShouldAddPayElementWithValidRequest()
        {
            _mockTimesheetGateway.Setup(tsg => tsg.GetCurrentTimeSheetForOperative(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult<Timesheet>(BonusCalcTestDataFactory.ValidTimesheet()));
        }

        [Test]
        public void ShouldNotAddAnyPayElementsIfTimesheetCannotBeFound()
        {
            // Arrange
            _mockTimesheetGateway.Setup(tsg => tsg.GetCurrentTimeSheetForOperative(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult<Timesheet>(null));

            _mockPayElementGateway.Setup(peg => peg.GetPayElementsByWorkOrderId(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(BonusCalcTestDataFactory.GeneratePayElements()));

            _mockDbSaver.Setup(db => db.SaveChangesAsync())
                .Verifiable();

            // Act
            TestDelegate act = async () => await _sut.ProcessMessageAsync(BonusCalcTestDataFactory.ValidMessage());

            // Assert
            _mockDbSaver.Verify(db => db.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public void ShouldThrowExceptionIfTimesheetCannotBeFound()
        {
            // Arrange
            _mockTimesheetGateway.Setup(tsg => tsg.GetCurrentTimeSheetForOperative(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult<Timesheet>(null));

            _mockPayElementGateway.Setup(peg => peg.GetPayElementsByWorkOrderId(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(BonusCalcTestDataFactory.GeneratePayElements()));

            _mockDbSaver.Setup(db => db.SaveChangesAsync())
                .Verifiable();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ProcessMessageAsync(BonusCalcTestDataFactory.ValidMessage()));
        }
    }
}
