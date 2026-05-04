using NetSdrClientApp.Messages;

namespace NetSdrClientAppTests
{
    public class NetSdrMessageHelperTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetControlItemMessageTest()
        {
            //Arrange
            var type = NetSdrMessageHelper.MsgTypes.Ack;
            var code = NetSdrMessageHelper.ControlItemCodes.ReceiverState;
            int parametersLength = 7500;

            //Act
            byte[] msg = NetSdrMessageHelper.GetControlItemMessage(type, code, new byte[parametersLength]);

            var headerBytes = msg.Take(2);
            var codeBytes = msg.Skip(2).Take(2);
            var parametersBytes = msg.Skip(4);

            var num = BitConverter.ToUInt16(headerBytes.ToArray());
            var actualType = (NetSdrMessageHelper.MsgTypes)(num >> 13);
            var actualLength = num - ((int)actualType << 13);
            var actualCode = BitConverter.ToInt16(codeBytes.ToArray());

            //Assert
            Assert.That(headerBytes.Count(), Is.EqualTo(2));
            Assert.That(msg.Length, Is.EqualTo(actualLength));
            Assert.That(type, Is.EqualTo(actualType));

            Assert.That(actualCode, Is.EqualTo((short)code));

            Assert.That(parametersBytes.Count(), Is.EqualTo(parametersLength));
        }

        [Test]
        public void GetDataItemMessageTest()
        {
            //Arrange
            var type = NetSdrMessageHelper.MsgTypes.DataItem2;
            int parametersLength = 7500;

            //Act
            byte[] msg = NetSdrMessageHelper.GetDataItemMessage(type, new byte[parametersLength]);

            var headerBytes = msg.Take(2);
            var parametersBytes = msg.Skip(2);

            var num = BitConverter.ToUInt16(headerBytes.ToArray());
            var actualType = (NetSdrMessageHelper.MsgTypes)(num >> 13);
            var actualLength = num - ((int)actualType << 13);

            //Assert
            Assert.That(headerBytes.Count(), Is.EqualTo(2));
            Assert.That(msg.Length, Is.EqualTo(actualLength));
            Assert.That(type, Is.EqualTo(actualType));

            Assert.That(parametersBytes.Count(), Is.EqualTo(parametersLength));
        }

        [Test]
        public void TranslateMessage_ControlItemRoundtrip_PreservesTypeCodeAndBody()
        {
            var type = NetSdrMessageHelper.MsgTypes.SetControlItem;
            var code = NetSdrMessageHelper.ControlItemCodes.ReceiverFrequency;
            var body = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 };

            var raw = NetSdrMessageHelper.GetControlItemMessage(type, code, body);

            var success = NetSdrMessageHelper.TranslateMessage(
                raw,
                out var actualType,
                out var actualCode,
                out var sequenceNumber,
                out var actualBody);

            Assert.Multiple(() =>
            {
                Assert.That(success, Is.True);
                Assert.That(actualType, Is.EqualTo(type));
                Assert.That(actualCode, Is.EqualTo(code));
                Assert.That(sequenceNumber, Is.EqualTo(0));
                Assert.That(actualBody, Is.EqualTo(body));
            });
        }

        [Test]
        public void TranslateMessage_DataItemRoundtrip_ExtractsSequenceNumber()
        {
            var type = NetSdrMessageHelper.MsgTypes.DataItem1;
            var body = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD };
            var raw = NetSdrMessageHelper.GetDataItemMessage(type, body);

            NetSdrMessageHelper.TranslateMessage(
                raw,
                out var actualType,
                out var actualCode,
                out var sequenceNumber,
                out var actualBody);

            Assert.Multiple(() =>
            {
                Assert.That(actualType, Is.EqualTo(type));
                Assert.That(actualCode, Is.EqualTo(NetSdrMessageHelper.ControlItemCodes.None));
                Assert.That(actualBody.Length, Is.EqualTo(body.Length - 2),
                    "First two bytes of data item body are interpreted as sequence number.");
            });

            Assert.That(sequenceNumber, Is.EqualTo(BitConverter.ToUInt16(body.AsSpan(0, 2))));
        }

        [Test]
        public void GetSamples_With16BitWidth_YieldsExpectedCount()
        {
            var body = new byte[]
            {
                0x01, 0x00,
                0x02, 0x00,
                0x03, 0x00,
                0xFF, 0x7F
            };

            var samples = NetSdrMessageHelper.GetSamples(16, body).ToArray();

            Assert.That(samples, Is.EqualTo(new[] { 1, 2, 3, 0x7FFF }));
        }

        [Test]
        public void GetSamples_With8BitWidth_TruncatesIncompleteTail()
        {
            var body = new byte[] { 0x10, 0x20, 0x30 };

            var samples = NetSdrMessageHelper.GetSamples(8, body).ToArray();

            Assert.That(samples, Is.EqualTo(new[] { 0x10, 0x20, 0x30 }));
        }

        [Test]
        public void GetSamples_OnEmptyBody_ReturnsEmptySequenceWithoutThrowing()
        {
            var samples = NetSdrMessageHelper.GetSamples(16, Array.Empty<byte>()).ToArray();

            Assert.That(samples, Is.Empty);
        }

        [Test]
        public void GetSamples_With40BitWidth_ThrowsArgumentOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => NetSdrMessageHelper.GetSamples(40, new byte[8]).ToArray());
        }
    }
}