using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.Contracts;
using System.Net;
using WebhookFunctionApp.Functions;
using WebhookFunctionApp.Services.RequestStorage;
using WebhookFunctionApp.Services.RequestValidation;

namespace WebhookTests
{
    public class ValidateAndStoreFunctionTests
    {
        [Fact]
        public void ValidRequest_CreatedResponseAndRequestStored()
        {
            // Arrange

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(lf => lf
                .CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            var mockRequestValidator = new Mock<IRequestValidator>();
            mockRequestValidator.Setup(rv => rv
                .Validate(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new RequestValidationResult { IsValid = true });

            var mockRequestStore = new Mock<IRequestStore>();

            var validateAndStoreSUT = new ValidateAndStoreFunction(
                mockLoggerFactory.Object, mockRequestValidator.Object, mockRequestStore.Object);

            // Act

            var response = 
                validateAndStoreSUT.Run(new MockHttpRequestData(new { }), "contractId", "senderId", "tenantId");

            // Assert

            response.Should().NotBeNull();

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // TODO: Assert the response is stored
        }
    }
}