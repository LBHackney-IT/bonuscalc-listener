using BonusCalcListener.Gateway;
using BonusCalcListener.Gateway.Interfaces;
using BonusCalcListener.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Frameworks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BonusCalcListener.Tests.Gateway
{
    public class OperativesGatewayTests : DatabaseTests
    {
        private Mock<ILogger<OperativesGateway>> _loggerMock;
        private OperativesGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<OperativesGateway>>();
            _classUnderTest = new OperativesGateway(BonusCalcContext, _loggerMock.Object);
        }

        [Test]
        public async Task ActivatesOperativeIfArchivedOperativeFound()
        {
            // Arrange
            var initialState = BonusCalcContext.Operatives.FirstOrDefault().IsArchived = true;

            // Act
            await _classUnderTest.ActivateOperative("123456").ConfigureAwait(false);

            // Assert
            var result = BonusCalcContext.Operatives.FirstOrDefault();

            result.EmailAddress.Should().Be("test@test.gov.uk");
            result.Name.Should().Be("An Operative");
            result.IsArchived.Should().Be(false);
        }

        [Test]
        public async Task DoNothingIfOperativeNotFound()
        {
            // Arrange
            var initialState = BonusCalcContext.Operatives.FirstOrDefault().IsArchived = true;

            // Act
            await _classUnderTest.ActivateOperative("MISSING").ConfigureAwait(false);

            // Assert
            BonusCalcContext.Operatives.FirstOrDefault().IsArchived.Should().Be(true);

            _loggerMock.Verify(logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == $"We recieved a request to unarchive operative MISSING who doesn't exist here"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public void ThrowExceptionIfNullOperative()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => _classUnderTest.ActivateOperative(null));
        }
    }
}
