using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;
using System.Net;
using System.Text;
using WebhookFunctionApp.Functions;
using WebhookFunctionApp.Models;
using WebhookFunctionApp.Services.RequestStore;
using WebhookFunctionApp.Services.RequestValidation;
using WebhookFunctionApp.Utilities;
using WebhookTests.Mocks;

namespace WebhookTests;

public class ValidateAndStoreFunctionTests
{
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;
    private readonly Mock<IRequestValidator> _mockRequestValidator;

    public ValidateAndStoreFunctionTests()
    {
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockLoggerFactory.Setup(lf => lf
            .CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);

        _mockRequestValidator = new Mock<IRequestValidator>();
    }

    [Fact]
    public async Task ValidRequest_CreatedResponse_StoredAsValid()
    {
        // Arrange

        const string ExpectedContractId = "contractId";
        const string ExpectedSenderId = "senderId";
        const string ExpectedTenantId = "tenantId";

        var requestValidationResult = new RequestValidationResult { IsValid = true };

        _mockRequestValidator.Setup(rv => rv
            .Validate(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(requestValidationResult);

        var mockRequestStore = new Mock<IPayloadStore>();

        var validateAndStoreSUT = new ValidateAndStoreFunction(
            _mockLoggerFactory.Object, _mockRequestValidator.Object, mockRequestStore.Object);

        // Act

        var response = 
            await validateAndStoreSUT.Run(
                new MockHttpRequestData(new { }), ExpectedContractId, ExpectedSenderId, ExpectedTenantId);

        // Assert

        response.Should().NotBeNull();

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        mockRequestStore.Verify(rs => 
            rs.AddAcceptedPayloadAsync(
                It.Is<string>(p => p == ExpectedTenantId),
                It.Is<string>(p => p == ExpectedSenderId),
                It.Is<string>(p => p == ExpectedContractId),
                It.Is<string>(p => Guid.Parse(p) != Guid.Empty),
                It.IsAny<IEnumerable<Tuple<string, string>>>(),
                It.IsAny<string>()));
    }

    [Fact]
    public async Task InvalidRequest_BadRequestResponseWithErrors_StoredAsInvalid()
    {
        // Arrange

        const string ExpectedContractId = "contractId";
        const string ExpectedSenderId = "senderId";
        const string ExpectedTenantId = "tenantId";

        var requestValidationResult = 
            new RequestValidationResult 
            { 
                IsValid = false,
                ErrorMessages = ["Missing properties"]
            };

        _mockRequestValidator.Setup(rv => rv
            .Validate(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(requestValidationResult);

        var mockRequestStore = new Mock<IPayloadStore>();

        var validateAndStoreSUT = new ValidateAndStoreFunction(
            _mockLoggerFactory.Object, _mockRequestValidator.Object, mockRequestStore.Object);

        // Act

        var response =
            await validateAndStoreSUT.Run(
                new MockHttpRequestData(new { }), ExpectedContractId, ExpectedSenderId, ExpectedTenantId);

        // Assert

        response.Should().NotBeNull();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var expectedErrors =
            JsonConvert.DeserializeObject(
                JsonConvert.SerializeObject(requestValidationResult.ErrorMessages));

        var actualErrors = JsonConvert.DeserializeObject(response.ReadBodyString());

        actualErrors.Should().BeEquivalentTo(expectedErrors);

        mockRequestStore.Verify(rs =>
            rs.AddRejectedPayloadAsync(
                It.Is<string>(p => p == ExpectedTenantId),
                It.Is<string>(p => p == ExpectedSenderId),
                It.Is<string>(p => p == ExpectedContractId),
                It.Is<string>(p => Guid.Parse(p) != Guid.Empty),
                It.IsAny<IEnumerable<Tuple<string, string>>>(),
                It.IsAny<string>(),
                It.Is<IList<string>>(p => p == requestValidationResult.ErrorMessages)));
    }
}