using FluentAssertions;

namespace DocuMind.UnitTests;

public class SmokeTests
{
    [Fact]
    public void TestHarness_Boots()
    {
        true.Should().BeTrue();
    }
}
