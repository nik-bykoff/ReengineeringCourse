using NetSdrClientApp.Networking;

namespace NetSdrClientAppTests;

public class WrapperDisposeTests
{
    [Test]
    public void TcpClientWrapper_Dispose_OnFreshInstance_DoesNotThrow()
    {
        var wrapper = new TcpClientWrapper("127.0.0.1", 0);

        Assert.DoesNotThrow(() => wrapper.Dispose());
    }

    [Test]
    public void TcpClientWrapper_Dispose_TwiceInARow_DoesNotThrow()
    {
        var wrapper = new TcpClientWrapper("127.0.0.1", 0);

        Assert.DoesNotThrow(() => wrapper.Dispose());
        Assert.DoesNotThrow(() => wrapper.Dispose());
    }

    [Test]
    public void TcpClientWrapper_Disconnect_WhenNotConnected_DoesNotThrow()
    {
        var wrapper = new TcpClientWrapper("127.0.0.1", 0);

        Assert.DoesNotThrow(() => wrapper.Disconnect());
    }

    [Test]
    public void UdpClientWrapper_Dispose_OnFreshInstance_DoesNotThrow()
    {
        var wrapper = new UdpClientWrapper(0);

        Assert.DoesNotThrow(() => wrapper.Dispose());
    }

    [Test]
    public void UdpClientWrapper_Dispose_TwiceInARow_DoesNotThrow()
    {
        var wrapper = new UdpClientWrapper(0);

        Assert.DoesNotThrow(() => wrapper.Dispose());
        Assert.DoesNotThrow(() => wrapper.Dispose());
    }

    [Test]
    public void UdpClientWrapper_StopListening_OnFreshInstance_DoesNotThrow()
    {
        var wrapper = new UdpClientWrapper(0);

        Assert.DoesNotThrow(() => wrapper.StopListening());
    }

    [Test]
    public void UdpClientWrapper_Exit_OnFreshInstance_DoesNotThrow()
    {
        var wrapper = new UdpClientWrapper(0);

        Assert.DoesNotThrow(() => wrapper.Exit());
    }
}
