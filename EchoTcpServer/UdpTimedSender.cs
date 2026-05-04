using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EchoTcpServer
{
    public sealed class UdpTimedSender : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private readonly UdpClient _udpClient;
        private Timer? _timer;
        private ushort _sequence;

        public UdpTimedSender(string host, int port)
            : this(host, port, new UdpClient())
        {
        }

        internal UdpTimedSender(string host, int port, UdpClient udpClient)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _port = port;
            _udpClient = udpClient ?? throw new ArgumentNullException(nameof(udpClient));
        }

        public bool IsRunning => _timer is not null;

        public void StartSending(int intervalMilliseconds)
        {
            if (intervalMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(intervalMilliseconds));
            }

            if (_timer is not null)
            {
                throw new InvalidOperationException("Sender is already running.");
            }

            _timer = new Timer(SendMessageCallback, null, 0, intervalMilliseconds);
        }

        private void SendMessageCallback(object? state)
        {
            try
            {
                var rnd = new Random();
                byte[] samples = new byte[1024];
                rnd.NextBytes(samples);
                _sequence++;

                byte[] msg = new byte[] { 0x04, 0x84 }
                    .Concat(BitConverter.GetBytes(_sequence))
                    .Concat(samples)
                    .ToArray();
                var endpoint = new IPEndPoint(IPAddress.Parse(_host), _port);

                _udpClient.Send(msg, msg.Length, endpoint);
                Console.WriteLine($"Message sent to {_host}:{_port} ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        public void StopSending()
        {
            _timer?.Dispose();
            _timer = null;
        }

        public void Dispose()
        {
            StopSending();
            _udpClient.Dispose();
        }
    }
}
