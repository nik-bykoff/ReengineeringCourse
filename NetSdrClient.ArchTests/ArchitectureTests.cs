using System.Reflection;
using NetArchTest.Rules;

namespace NetSdrClient.ArchTests;

public class ArchitectureTests
{
    private static readonly Assembly ProductionAssembly = typeof(NetSdrClientApp.NetSdrClient).Assembly;

    private const string MessagesNamespace = "NetSdrClientApp.Messages";
    private const string NetworkingNamespace = "NetSdrClientApp.Networking";

    [Test]
    public void Messages_ShouldNotDependOn_Networking()
    {
        var result = Types.InAssembly(ProductionAssembly)
            .That()
            .ResideInNamespace(MessagesNamespace)
            .ShouldNot()
            .HaveDependencyOn(NetworkingNamespace)
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            FormatFailure(result, $"Types in {MessagesNamespace} must not reference {NetworkingNamespace}."));
    }

    [Test]
    public void Networking_ShouldNotDependOn_Messages()
    {
        var result = Types.InAssembly(ProductionAssembly)
            .That()
            .ResideInNamespace(NetworkingNamespace)
            .ShouldNot()
            .HaveDependencyOn(MessagesNamespace)
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            FormatFailure(result, $"Types in {NetworkingNamespace} must stay transport-only and not pull {MessagesNamespace}."));
    }

    [Test]
    public void Interfaces_InNetworking_ShouldStartWithI()
    {
        var result = Types.InAssembly(ProductionAssembly)
            .That()
            .ResideInNamespace(NetworkingNamespace)
            .And()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            FormatFailure(result, "Interfaces in Networking namespace must follow the I-prefix naming convention."));
    }

    [Test]
    public void NetworkingWrappers_ShouldBeSealed()
    {
        var result = Types.InAssembly(ProductionAssembly)
            .That()
            .ResideInNamespace(NetworkingNamespace)
            .And()
            .HaveNameEndingWith("Wrapper")
            .Should()
            .BeSealed()
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            FormatFailure(result, "Wrapper classes in Networking namespace should be sealed to prevent unintended subclassing."));
    }

    private static string FormatFailure(TestResult result, string headline)
    {
        if (result.FailingTypeNames is null || result.FailingTypeNames.Count == 0)
        {
            return headline;
        }

        return headline + " Failing types: " + string.Join(", ", result.FailingTypeNames);
    }
}
