using BonusCalcListener.Boundary;
using BonusCalcListener.Domain;
using BonusCalcListener.Gateway.Interfaces;
using BonusCalcListener.Infrastructure;
using BonusCalcListener.UseCase;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
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
        private Mock<ILogger<UpdateExistingWorkOrderPayElementsUseCase>> _logger;
        //private Mock<IOperativesGateway> _operativesGatewayMock;

        [SetUp]
        public void Setup()
        {
            _mockTimesheetGateway = new Mock<ITimesheetGateway>();
            _mockPayElementsMapper = new Mock<IMapPayElements>();
            _mockDbSaver = new Mock<IDbSaver>();
            _mockPayElement = new Mock<PayElement>();
            _logger = new Mock<ILogger<UpdateExistingWorkOrderPayElementsUseCase>>();
            //_operativesGatewayMock = new Mock<IOperativesGateway>();

            //_operativesGatewayMock.Setup(g => g.ActivateOperative(It.IsAny<string>()))
            //    .Verifiable();

            _sut = new UpdateExistingWorkOrderPayElementsUseCase(
                _mockTimesheetGateway.Object,
                _mockPayElementsMapper.Object,
                _mockDbSaver.Object,
                _logger.Object
                //_operativesGatewayMock.Object
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

        [TestCase(PaymentType.Overtime)]
        [TestCase(PaymentType.Bonus)]
        public void WhenPaymentTypeNotCloseToBaseShouldAddPayElementWithValidRequest(PaymentType paymentType)
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

            var message = BonusCalcTestDataFactory.ValidMessage();
            message.EventData.PaymentType = paymentType;

            // Act
            Assert.DoesNotThrowAsync(() => _sut.ProcessMessageAsync(message));

            // Assert
            _mockDbSaver.Verify(db => db.SaveChangesAsync(), Times.Once);

            //_operativesGatewayMock.Verify(m => m.ActivateOperative(It.IsAny<string>()), Times.Once);
        }

        //[TestCase("T1000")]
        //[TestCase("T0001")]
        //[TestCase("T1234")]
        //[TestCase("T")]
        //[TestCase("TTTTT")]
        //public void WhenOperativeIsAgencyOperativeDoesNothing(string payrollNumber)
        //{
        //    _mockDbSaver.Setup(db => db.SaveChangesAsync())
        //        .Verifiable();

        //    var message = BonusCalcTestDataFactory.ValidMessage();
        //    message.EventData.OperativePrn = payrollNumber;

        //    // Act
        //    Assert.DoesNotThrowAsync(() => _sut.ProcessMessageAsync(message));

        //    // Assert
        //    _mockDbSaver.Verify(db => db.SaveChangesAsync(), Times.Never);
        //    _logger.Verify(logger => logger.Log(
        //            It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
        //            It.IsAny<EventId>(),
        //            It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == $"WorkOrder {message.EventData.WorkOrderId} will not be recorded because the operative is agency: {payrollNumber}" && @type.Name == "FormattedLogValues"),
        //            It.IsAny<Exception>(),
        //            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        //        Times.Once);

        //    _operativesGatewayMock.Verify(m => m.ActivateOperative(It.IsAny<string>()), Times.Never);
        //}

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
        public void ShouldNotAddPayElementWithPaymentTypeCloseToBase()
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

            var message = BonusCalcTestDataFactory.ValidMessage();
            message.EventData.PaymentType = PaymentType.CloseToBase;

            // Act
            Assert.DoesNotThrowAsync(() => _sut.ProcessMessageAsync(message));

            // Assert
            _mockDbSaver.Verify(db => db.SaveChangesAsync(), Times.Never);
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
        public void ShouldNotAddPayElementWithOldRequest()
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
            Assert.DoesNotThrowAsync(() => _sut.ProcessMessageAsync(BonusCalcTestDataFactory.OldMessage()));

            // Assert
            Assert.That(payElements, Does.Not.Contain(_mockPayElement.Object));
            Assert.That(payElements, Contains.Item(nonProductivePayElement));
            Assert.That(payElements, Contains.Item(productivePayElement));
            _mockDbSaver.Verify(db => db.SaveChangesAsync(), Times.Never);
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
