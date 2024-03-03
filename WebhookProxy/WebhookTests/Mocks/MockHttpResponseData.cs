using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Net;

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