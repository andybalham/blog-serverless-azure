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
        private Mock<ILoggerFactory> _mockLoggerFactory;
        private Mock<IRequestValidator> _mockRequestValidator;

        public ValidateAndStoreFunctionTests()
        {
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLoggerFactory.Setup(lf => lf
                .CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            _mockRequestValidator = new Mock<IRequestValidator>();
        }

        [Fact]
        public void ValidRequest_CreatedResponse_StoredAsValid()
        {
            // Arrange

            const string ExpectedContractId = "contractId";
            const string ExpectedSenderId = "senderId";
            const string ExpectedTenantId = "tenantId";

            var requestValidationResult = new RequestValidationResult { IsValid = true };

            _mockRequestValidator.Setup(rv => rv
                .Validate(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(requestValidationResult);

            var mockRequestStore = new Mock<IRequestStore>();

            var validateAndStoreSUT = new ValidateAndStoreFunction(
                _mockLoggerFactory.Object, _mockRequestValidator.Object, mockRequestStore.Object);

            // Act

            var response = 
                validateAndStoreSUT.Run(new MockHttpRequestData(new { }), ExpectedContractId, ExpectedSenderId, ExpectedTenantId);

            // Assert

            response.Should().NotBeNull();

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            mockRequestStore.Verify(rs => 
                rs.PutValidRequest(
                    It.IsAny<HttpRequestData>(), 
                    It.Is<string>(p => p == ExpectedContractId), 
                    It.Is<string>(p => p == ExpectedSenderId), 
                    It.Is<string>(p => p == ExpectedTenantId)));
        }

        [Fact]
        public void InvalidRequest_BadRequestResponseWithErrors_StoredAsInvalid()
        {
            // Arrange

            const string ExpectedContractId = "contractId";
            const string ExpectedSenderId = "senderId";
            const string ExpectedTenantId = "tenantId";

            var requestValidationResult = 
                new RequestValidationResult 
                { 
                    IsValid = false,
                    ErrorMessages = new List<string> { "Missing properties" }
                };

            _mockRequestValidator.Setup(rv => rv
                .Validate(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(requestValidationResult);

            var mockRequestStore = new Mock<IRequestStore>();

            var validateAndStoreSUT = new ValidateAndStoreFunction(
                _mockLoggerFactory.Object, _mockRequestValidator.Object, mockRequestStore.Object);

            // Act

            var response =
                validateAndStoreSUT.Run(new MockHttpRequestData(new { }), ExpectedContractId, ExpectedSenderId, ExpectedTenantId);

            // Assert

            response.Should().NotBeNull();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            mockRequestStore.Verify(rs =>
                rs.PutInvalidRequest(
                    It.IsAny<HttpRequestData>(),
                    It.Is<string>(p => p == ExpectedContractId),
                    It.Is<string>(p => p == ExpectedSenderId),
                    It.Is<string>(p => p == ExpectedTenantId),
                    It.Is<IList<string>>(p => p == requestValidationResult.ErrorMessages)));
        }
    }
}