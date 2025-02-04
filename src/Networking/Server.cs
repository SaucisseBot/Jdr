using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Jdr.Networking
{
    public class Server
    {
        private readonly int _port;
        private TcpListener _listener;
        private readonly List<ClientConnection> _clients = new List<ClientConnection>();
        private readonly object _clientsLock = new object();
        private int _nextId = 1;

        public Server(int port)
        {
            _port = port;
        }

        public async Task StartAsync()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            Console.WriteLine($"Serveur lancé sur le port {_port}.");
            _ = Task.Run(AcceptClientsAsync);
            Console.WriteLine("Tapez vos messages :");
            while (true)
            {
                string message = Console.ReadLine();
                if (!string.IsNullOrEmpty(message))
                {
                    string fullMessage = $"[Serveur] {message}";
                    Console.WriteLine(fullMessage);
                    await BroadcastAsync(fullMessage, null);
                }
            }
        }

        private async Task AcceptClientsAsync()
        {
            while (true)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                int id;
                lock (_clientsLock)
                {
                    id = _nextId++;
                    _clients.Add(new ClientConnection(client, id));
                }
                Console.WriteLine($"Client {id} connecté.");
                _ = Task.Run(() => HandleClientAsync(client, id));
            }
        }

        private async Task HandleClientAsync(TcpClient client, int id)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (byteCount == 0)
                        break;
                    string message = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    string fullMessage = $"[Client {id}] {message}";
                    Console.WriteLine(fullMessage);
                    await BroadcastAsync(fullMessage, client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
            finally
            {
                lock (_clientsLock)
                {
                    _clients.RemoveAll(c => c.TcpClient == client);
                }
                client.Close();
                Console.WriteLine($"Client {id} déconnecté.");
            }
        }

        public async Task BroadcastAsync(string message, TcpClient sender)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            List<ClientConnection> clientsCopy;
            lock (_clientsLock)
            {
                clientsCopy = new List<ClientConnection>(_clients);
            }
            foreach (ClientConnection connection in clientsCopy)
            {
                if (connection.TcpClient != sender)
                {
                    try
                    {
                        await connection.TcpClient.GetStream().WriteAsync(data, 0, data.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Erreur d'envoi : " + ex.Message);
                    }
                }
            }
        }
    }
}
