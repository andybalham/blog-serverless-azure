using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Net;
using System.Runtime.CompilerServices;

namespace WebhookTests.Mocks;

internal class MockHttpResponseData : HttpResponseData
{
    public MockHttpResponseData() : base(new Mock<FunctionContext>().Object)
    {
        Body = new MemoryStream();
    }

    public override HttpStatusCode StatusCode { get; set; }
    public override HttpHeadersCollection Headers { get; set; } = new HttpHeadersCollection();
    public override Stream Body { get; set; }
    public override HttpCookies Cookies => throw new NotImplementedException();
}

internal static class MockHttpResponseDataExtensions
{
    public static string ReadBodyString(this HttpResponseData responseData)
    {
        responseData.Body.Position = 0;
        var body = new StreamReader(responseData.Body).ReadToEnd();
        return body;
    }
}