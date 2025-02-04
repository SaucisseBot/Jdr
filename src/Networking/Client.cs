using System.Net.Sockets;
using System.Text;

namespace Jdr.Networking
{
    public class Client
    {
        private readonly string _serverIp;
        private readonly int _port;
        private TcpClient _client;

        public Client(string serverIp, int port)
        {
            _serverIp = serverIp;
            _port = port;
        }

        public async Task StartAsync()
        {
            _client = new TcpClient();
            try
            {
                await _client.ConnectAsync(_serverIp, _port);
                Console.WriteLine("Connecté au serveur.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur de connexion : " + ex.Message);
                return;
            }

            _ = Task.Run(ReceiveAsync);
            NetworkStream stream = _client.GetStream();
            Console.WriteLine("Tapez vos messages :");
            while (true)
            {
                string message = Console.ReadLine();
                if (string.IsNullOrEmpty(message))
                    continue;
                string fullMessage = $"[Client] {message}";
                byte[] data = Encoding.UTF8.GetBytes(fullMessage);
                try
                {
                    await stream.WriteAsync(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur lors de l'envoi : " + ex.Message);
                    break;
                }
            }
            _client.Close();
        }

        private async Task ReceiveAsync()
        {
            NetworkStream stream = _client.GetStream();
            byte[] buffer = new byte[1024];
            try
            {
                while (true)
                {
                    int byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (byteCount == 0)
                        break;
                    string message = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    Console.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de la réception : " + ex.Message);
            }
        }
    }
}
