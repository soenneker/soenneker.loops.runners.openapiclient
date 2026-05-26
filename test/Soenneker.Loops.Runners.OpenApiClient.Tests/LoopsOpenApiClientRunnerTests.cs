using Soenneker.Tests.HostedUnit;

namespace Soenneker.Loops.Runners.OpenApiClient.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class LoopsOpenApiClientRunnerTests : HostedUnitTest
{

    public LoopsOpenApiClientRunnerTests(Host host) : base(host)
    {
    }

    [Test]
    public void Default()
    {

    }
}
