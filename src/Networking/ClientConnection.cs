using System.Net.Sockets;

namespace Jdr.Networking
{
    public class ClientConnection
    {
        public TcpClient TcpClient { get; }
        public int Id { get; }

        public ClientConnection(TcpClient tcpClient, int id)
        {
            TcpClient = tcpClient;
            Id = id;
        }
    }
}
