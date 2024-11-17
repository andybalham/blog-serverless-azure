using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Security.Claims;
using System.Text.Json;

namespace WebhookTests.Mocks;

internal class MockHttpRequestData(object bodyObject) : HttpRequestData(new Mock<FunctionContext>().Object)
{
    private readonly string _bodyJson = JsonSerializer.Serialize(bodyObject);

    public override Stream Body => GetStringAsStream(_bodyJson);

    public override HttpHeadersCollection Headers => new HttpHeadersCollection();

    public override IReadOnlyCollection<IHttpCookie> Cookies => throw new NotImplementedException();

    public override Uri Url => throw new NotImplementedException();

    public override IEnumerable<ClaimsIdentity> Identities => throw new NotImplementedException();

    public override string Method => throw new NotImplementedException();

    public override HttpResponseData CreateResponse()
    {
        return new MockHttpResponseData();
    }

    private static Stream GetStringAsStream(string input)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(input);
        writer.Flush();
        stream.Position = 0; // Reset stream position to the beginning
        return stream;
    }

}