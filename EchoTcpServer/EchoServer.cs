using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EchoTcpServer
{
    /// <summary>
    /// Local TCP echo server used during integration testing of <c>NetSdrClient</c>.
    /// Not intended for production use; introduced as a refactor of the original
    /// <c>Program.cs</c> to expose <see cref="IEchoServer"/> and to keep the echo
    /// loop testable through <see cref="EchoCore"/>.
    /// </summary>
    public sealed class EchoServer : IEchoServer
    {
        private readonly int _port;
        private TcpListener? _listener;
        private CancellationTokenSource? _cancellationTokenSource;

        public EchoServer(int port)
        {
            _port = port;
        }

        public async Task StartAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            Console.WriteLine($"Server started on port {_port}.");

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    Console.WriteLine("Client connected.");

                    _ = Task.Run(() => HandleClientAsync(client, _cancellationTokenSource.Token));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }

            Console.WriteLine("Server shutdown.");
        }

        private static async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            try
            {
                using NetworkStream stream = client.GetStream();
                long echoed = await EchoCore.EchoLoopAsync(stream, token).ConfigureAwait(false);
                Console.WriteLine($"Echoed {echoed} bytes total to the client.");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Client disconnected.");
            }
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _listener?.Stop();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            Console.WriteLine("Server stopped.");
        }
    }
}
