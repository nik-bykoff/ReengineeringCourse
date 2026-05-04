using System.Threading.Tasks;

namespace EchoTcpServer
{
    public interface IEchoServer
    {
        Task StartAsync();

        void Stop();
    }
}
