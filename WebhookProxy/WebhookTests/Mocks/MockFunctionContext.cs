using Microsoft.Azure.Functions.Worker;

namespace WebhookTests.Mocks;

internal class MockFunctionContext : FunctionContext
{
    public override string InvocationId => throw new NotImplementedException();

    public override string FunctionId => throw new NotImplementedException();

    public override TraceContext TraceContext => throw new NotImplementedException();

    public override BindingContext BindingContext => throw new NotImplementedException();

    public override RetryContext RetryContext => throw new NotImplementedException();

    public override IServiceProvider InstanceServices { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override FunctionDefinition FunctionDefinition => throw new NotImplementedException();

    public override IDictionary<object, object> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override IInvocationFeatures Features => throw new NotImplementedException();
}