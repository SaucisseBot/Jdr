
using Jdr.Networking;

namespace Jdr
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Choisissez le mode :");
            Console.WriteLine("1 - Serveur");
            Console.WriteLine("2 - Client");
            Console.Write("Votre choix : ");
            string choix = Console.ReadLine();

            if (choix == "1")
            {
                Server server = new Server(5000);
                await server.StartAsync();
            }
            else if (choix == "2")
            {
                Console.Write("Entrez l'adresse IP du serveur : ");
                string ip = Console.ReadLine();
                Client client = new Client(ip, 5000);
                await client.StartAsync();
            }
            else
            {
                Console.WriteLine("Choix invalide.");
            }
        }
    }
}
