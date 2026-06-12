using FluentAssertions;

namespace Lumindex.IntegrationTests;

public class SmokeTests
{
    [Fact]
    public void IntegrationHarness_Boots()
    {
        true.Should().BeTrue();
    }
}
