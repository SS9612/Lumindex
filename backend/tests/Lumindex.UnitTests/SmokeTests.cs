using FluentAssertions;

namespace Lumindex.UnitTests;

public class SmokeTests
{
    [Fact]
    public void TestHarness_Boots()
    {
        true.Should().BeTrue();
    }
}
