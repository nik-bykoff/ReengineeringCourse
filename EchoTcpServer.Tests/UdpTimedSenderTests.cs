using System;
using EchoTcpServer;
using NUnit.Framework;

namespace EchoTcpServerTests
{
    public class UdpTimedSenderTests
    {
        [Test]
        public void StartSending_TwiceWithoutStop_Throws()
        {
            using var sender = new UdpTimedSender("127.0.0.1", 60001);
            sender.StartSending(60_000);

            try
            {
                Assert.Throws<InvalidOperationException>(() => sender.StartSending(60_000));
            }
            finally
            {
                sender.StopSending();
            }
        }

        [Test]
        public void StopSending_AfterStart_AllowsSubsequentStart()
        {
            using var sender = new UdpTimedSender("127.0.0.1", 60002);

            sender.StartSending(60_000);
            sender.StopSending();

            Assert.DoesNotThrow(() => sender.StartSending(60_000));
            sender.StopSending();
        }

        [Test]
        public void StartSending_WithNonPositiveInterval_ThrowsArgumentOutOfRange()
        {
            using var sender = new UdpTimedSender("127.0.0.1", 60003);
            Assert.Throws<ArgumentOutOfRangeException>(() => sender.StartSending(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => sender.StartSending(-1));
        }

        [Test]
        public void Constructor_NullHost_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new UdpTimedSender(null!, 60004));
        }

        [Test]
        public void IsRunning_ReflectsTimerLifecycle()
        {
            using var sender = new UdpTimedSender("127.0.0.1", 60005);
            Assert.That(sender.IsRunning, Is.False);

            sender.StartSending(60_000);
            Assert.That(sender.IsRunning, Is.True);

            sender.StopSending();
            Assert.That(sender.IsRunning, Is.False);
        }
    }
}
