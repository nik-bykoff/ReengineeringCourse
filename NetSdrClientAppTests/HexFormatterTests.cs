using NetSdrClientApp.Networking;

namespace NetSdrClientAppTests;

public class HexFormatterTests
{
    [Test]
    public void ToSpaceSeparatedHex_OnNullInput_ReturnsEmptyString()
    {
        Assert.That(HexFormatter.ToSpaceSeparatedHex(null!), Is.EqualTo(string.Empty));
    }

    [Test]
    public void ToSpaceSeparatedHex_OnEmptyArray_ReturnsEmptyString()
    {
        Assert.That(HexFormatter.ToSpaceSeparatedHex(System.Array.Empty<byte>()), Is.EqualTo(string.Empty));
    }

    [Test]
    public void ToSpaceSeparatedHex_OnSingleByte_ReturnsSingleHexToken()
    {
        Assert.That(HexFormatter.ToSpaceSeparatedHex(new byte[] { 0x0F }), Is.EqualTo("f"));
    }

    [Test]
    public void ToSpaceSeparatedHex_OnMultipleBytes_JoinsThemWithSingleSpace()
    {
        var formatted = HexFormatter.ToSpaceSeparatedHex(new byte[] { 0x00, 0xAB, 0x10, 0xFF });

        Assert.That(formatted, Is.EqualTo("0 ab 10 ff"));
    }
}
