using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EchoTcpServer
{
    public static class EchoCore
    {
        public const int DefaultBufferSize = 8192;

        public static async Task<long> EchoLoopAsync(Stream stream, CancellationToken token, int bufferSize = DefaultBufferSize)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

            byte[] buffer = new byte[bufferSize];
            long total = 0;
            int bytesRead;

            while (!token.IsCancellationRequested
                   && (bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) > 0)
            {
                await stream.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(false);
                total += bytesRead;
            }

            return total;
        }
    }
}
