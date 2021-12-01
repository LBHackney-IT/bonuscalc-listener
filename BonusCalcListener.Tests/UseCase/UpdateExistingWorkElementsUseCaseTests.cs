using BonusCalcListener.Boundary;
using BonusCalcListener.Gateway;
using BonusCalcListener.Gateway.Interfaces;
using BonusCalcListener.Infrastructure;
using BonusCalcListener.UseCase;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BonusCalcListener.Tests.UseCase
{
    [TestFixture]
    public class UpdateExistingWorkElementsUseCaseTests
    {
        private UpdateExistingWorkOrderPayElementsUseCase _sut;
        private Mock<ITimesheetGateway> _mockTimesheetGateway;
        private Mock<IMapPayElements> _mockPayElementsMapper;
        private Mock<IDbSaver> _mockDbSaver;
        private Mock<PayElement> _mockPayElement;
        private ILogger<UpdateExistingWorkOrderPayElementsUseCase> _logger;

        [SetUp]
        public void Setup()
        {
            _mockTimesheetGateway = new Mock<ITimesheetGateway>();
            _mockPayElementsMapper = new Mock<IMapPayElements>();
            _mockDbSaver = new Mock<IDbSaver>();
            _mockPayElement = new Mock<PayElement>();
            _logger = new NullLogger<UpdateExistingWorkOrderPayElementsUseCase>();

            _sut = new UpdateExistingWorkOrderPayElementsUseCase(
                _mockTimesheetGateway.Object,
                _mockPayElementsMapper.Object,
                _mockDbSaver.Object,
                _logger
                );
        }

        [Test]
        public void ShouldAddPayElementWithValidRequest()
        {
            var timesheet = BonusCalcTestDataFactory.ValidTimesheet();
            var payElements = timesheet.PayElements;
            var productivePayElement = payElements.First(pe => pe.PayElementType.Id == 301);
            var nonProductivePayElement = payElements.First(pe => pe.PayElementType.Id == 101);

            _mockTimesheetGateway.Setup(tsg => tsg.GetCurrentTimeSheetForOperative(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult<Timesheet>(timesheet));

            _mockPayElementsMapper.Setup(pem => pem.BuildPayElement(It.IsAny<WorkOrderOperativeSmvData>()))
                .Returns(_mockPayElement.Object);

            _mockDbSaver.Setup(db => db.SaveChangesAsync())
                .Verifiable();

            // Act
            Assert.DoesNotThrowAsync(() => _sut.ProcessMessageAsync(BonusCalcTestDataFactory.ValidMessage()));

            // Assert
            Assert.That(payElements, Contains.Item(_mockPayElement.Object));
            Assert.That(payElements, Contains.Item(nonProductivePayElement));
            Assert.That(payElements, Does.Not.Contain(productivePayElement));
            _mockDbSaver.Verify(db => db.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void ShouldAddPayElementWithNoAccessRequest()
        {
            var timesheet = BonusCalcTestDataFactory.ValidTimesheet();
            var payElements = timesheet.PayElements;
            var productivePayElement = payElements.First(pe => pe.PayElementType.Id == 301);
            var nonProductivePayElement = payElements.First(pe => pe.PayElementType.Id == 101);

            _mockTimesheetGateway.Setup(tsg => tsg.GetCurrentTimeSheetForOperative(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult<Timesheet>(timesheet));

            _mockPayElementsMapper.Setup(pem => pem.BuildPayElement(It.IsAny<WorkOrderOperativeSmvData>()))
                .Returns(_mockPayElement.Object);

            _mockDbSaver.Setup(db => db.SaveChangesAsync())
                .Verifiable();

            // Act
            Assert.DoesNotThrowAsync(() => _sut.ProcessMessageAsync(BonusCalcTestDataFactory.NoAccessMessage()));

            // Assert
            Assert.That(payElements, Contains.Item(_mockPayElement.Object));
            Assert.That(payElements, Contains.Item(nonProductivePayElement));
            Assert.That(payElements, Does.Not.Contain(productivePayElement));
            _mockDbSaver.Verify(db => db.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void ShouldNotAddPayElementWithCancelledRequest()
        {
            var timesheet = BonusCalcTestDataFactory.ValidTimesheet();
            var payElements = timesheet.PayElements;
            var productivePayElement = payElements.First(pe => pe.PayElementType.Id == 301);
            var nonProductivePayElement = payElements.First(pe => pe.PayElementType.Id == 101);

            _mockTimesheetGateway.Setup(tsg => tsg.GetCurrentTimeSheetForOperative(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult<Timesheet>(timesheet));

            _mockPayElementsMapper.Setup(pem => pem.BuildPayElement(It.IsAny<WorkOrderOperativeSmvData>()))
                .Returns(_mockPayElement.Object);

            _mockDbSaver.Setup(db => db.SaveChangesAsync())
                .Verifiable();

            // Act
            Assert.DoesNotThrowAsync(() => _sut.ProcessMessageAsync(BonusCalcTestDataFactory.CancelledMessage()));

            // Assert
            Assert.That(payElements, Does.Not.Contain(_mockPayElement.Object));
            Assert.That(payElements, Contains.Item(nonProductivePayElement));
            Assert.That(payElements, Does.Not.Contain(productivePayElement));
            _mockDbSaver.Verify(db => db.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void ShouldNotAddPayElementWithUnknownRequest()
        {
            var timesheet = BonusCalcTestDataFactory.ValidTimesheet();
            var payElements = timesheet.PayElements;
            var productivePayElement = payElements.First(pe => pe.PayElementType.Id == 301);
            var nonProductivePayElement = payElements.First(pe => pe.PayElementType.Id == 101);

            _mockTimesheetGateway.Setup(tsg => tsg.GetCurrentTimeSheetForOperative(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult<Timesheet>(timesheet));

            _mockPayElementsMapper.Setup(pem => pem.BuildPayElement(It.IsAny<WorkOrderOperativeSmvData>()))
                .Returns(_mockPayElement.Object);

            _mockDbSaver.Setup(db => db.SaveChangesAsync())
                .Verifiable();

            // Act
            Assert.DoesNotThrowAsync(() => _sut.ProcessMessageAsync(BonusCalcTestDataFactory.UnknownMessage()));

            // Assert
            Assert.That(payElements, Does.Not.Contain(_mockPayElement.Object));
            Assert.That(payElements, Contains.Item(nonProductivePayElement));
            Assert.That(payElements, Does.Not.Contain(productivePayElement));
            _mockDbSaver.Verify(db => db.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void ShouldNotAddAnyPayElementsIfTimesheetCannotBeFound()
        {
            // Arrange
            _mockTimesheetGateway.Setup(tsg => tsg.GetCurrentTimeSheetForOperative(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult<Timesheet>(null));

            _mockDbSaver.Setup(db => db.SaveChangesAsync())
                .Verifiable();

            // Act
            Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ProcessMessageAsync(BonusCalcTestDataFactory.ValidMessage()));

            // Assert
            _mockDbSaver.Verify(db => db.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public void ShouldThrowExceptionIfTimesheetCannotBeFound()
        {
            // Arrange
            _mockTimesheetGateway.Setup(tsg => tsg.GetCurrentTimeSheetForOperative(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult<Timesheet>(null));

            _mockDbSaver.Setup(db => db.SaveChangesAsync())
                .Verifiable();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ProcessMessageAsync(BonusCalcTestDataFactory.ValidMessage()));
        }
    }
}
