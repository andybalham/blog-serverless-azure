using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace WebhookTests
{
    internal class MockHttpResponseData : HttpResponseData
    {
        public MockHttpResponseData() : base(new MockFunctionContext())
        {
            Body = new MemoryStream();
        }

        public override HttpStatusCode StatusCode { get; set; }
        public override HttpHeadersCollection Headers { get; set; } = new HttpHeadersCollection();
        public override Stream Body { get; set; }
        public override HttpCookies Cookies => throw new NotImplementedException();
    }
}