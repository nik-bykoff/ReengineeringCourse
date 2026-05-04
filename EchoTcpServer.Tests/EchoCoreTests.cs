using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EchoTcpServer;
using NUnit.Framework;

namespace EchoTcpServerTests
{
    public class EchoCoreTests
    {
        [Test]
        public async Task EchoLoopAsync_OnPlainPayload_WritesIdenticalBytesBack()
        {
            var inbound = Encoding.UTF8.GetBytes("hello world");
            var input = new MemoryStream(inbound);
            var output = new MemoryStream();
            using var bridge = new BridgeStream(input, output);

            long total = await EchoCore.EchoLoopAsync(bridge, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(total, Is.EqualTo(inbound.Length));
                Assert.That(output.ToArray(), Is.EqualTo(inbound));
            });
        }

        [Test]
        public async Task EchoLoopAsync_StopsWhenStreamClosed()
        {
            var input = new MemoryStream(Array.Empty<byte>());
            var output = new MemoryStream();
            using var bridge = new BridgeStream(input, output);

            long total = await EchoCore.EchoLoopAsync(bridge, CancellationToken.None);

            Assert.That(total, Is.EqualTo(0));
            Assert.That(output.Length, Is.EqualTo(0));
        }

        [Test]
        public void EchoLoopAsync_RespectsCancellationToken()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            var input = new MemoryStream(new byte[] { 1, 2, 3 });
            var output = new MemoryStream();
            using var bridge = new BridgeStream(input, output);

            Assert.DoesNotThrowAsync(async () =>
            {
                await EchoCore.EchoLoopAsync(bridge, cts.Token);
            });

            Assert.That(output.Length, Is.EqualTo(0),
                "Pre-cancelled token must short-circuit the loop before any echo happens.");
        }

        [Test]
        public void EchoLoopAsync_NullStream_Throws()
        {
            Assert.ThrowsAsync<ArgumentNullException>(
                async () => await EchoCore.EchoLoopAsync(null!, CancellationToken.None));
        }

        [Test]
        public void EchoLoopAsync_NonPositiveBufferSize_Throws()
        {
            using var input = new MemoryStream();
            using var output = new MemoryStream();
            using var bridge = new BridgeStream(input, output);

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await EchoCore.EchoLoopAsync(bridge, CancellationToken.None, bufferSize: 0));
        }

        /// <summary>
        /// Couples a read-only input stream with a write-only output stream so the
        /// echo loop can be exercised without real sockets.
        /// </summary>
        private sealed class BridgeStream : Stream
        {
            private readonly Stream _input;
            private readonly Stream _output;

            public BridgeStream(Stream input, Stream output)
            {
                _input = input;
                _output = output;
            }

            public override bool CanRead => _input.CanRead;
            public override bool CanSeek => false;
            public override bool CanWrite => _output.CanWrite;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => 0; set => throw new NotSupportedException(); }

            public override void Flush() => _output.Flush();

            public override int Read(byte[] buffer, int offset, int count) =>
                _input.Read(buffer, offset, count);

            public override long Seek(long offset, SeekOrigin origin) =>
                throw new NotSupportedException();

            public override void SetLength(long value) =>
                throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count) =>
                _output.Write(buffer, offset, count);

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _input.Dispose();
                    _output.Dispose();
                }

                base.Dispose(disposing);
            }
        }
    }
}
