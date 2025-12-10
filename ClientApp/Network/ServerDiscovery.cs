using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ClientApp.Network;

/// <summary>
/// Service de découverte réseau pour trouver automatiquement
/// les serveurs disponibles sur le réseau local
/// </summary>
public class ServerDiscovery
{
    private readonly int _discoveryPort;
    
    public ServerDiscovery(int discoveryPort = 7778)
    {
        _discoveryPort = discoveryPort;
    }
    
    /// <summary>
    /// Recherche les serveurs disponibles sur le réseau local
    /// </summary>
    /// <param name="timeout">Timeout en millisecondes</param>
    /// <returns>Liste des serveurs trouvés (IP, Port, Nom)</returns>
    public async Task<List<(string IpAddress, int Port, string ServerName)>> FindServersAsync(int timeout = 3000)
    {
        var servers = new List<(string, int, string)>();
        using var udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;
        
        try
        {
            Console.WriteLine("🔍 Recherche de serveurs sur le réseau local...");
            
            // Envoyer un broadcast UDP
            var requestData = Encoding.UTF8.GetBytes("ECHEC_PONG_DISCOVERY");
            var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, _discoveryPort);
            await udpClient.SendAsync(requestData, broadcastEndpoint);
            
            // Écouter les réponses avec timeout
            udpClient.Client.ReceiveTimeout = timeout;
            var startTime = DateTime.UtcNow;
            
            while ((DateTime.UtcNow - startTime).TotalMilliseconds < timeout)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync();
                    var response = Encoding.UTF8.GetString(result.Buffer);
                    
                    // Format: ECHEC_PONG_SERVER:PORT:SERVERNAME
                    if (response.StartsWith("ECHEC_PONG_SERVER:"))
                    {
                        var parts = response.Split(':');
                        if (parts.Length >= 3)
                        {
                            var ipAddress = result.RemoteEndPoint.Address.ToString();
                            var port = int.Parse(parts[1]);
                            var serverName = parts[2];
                            
                            servers.Add((ipAddress, port, serverName));
                            Console.WriteLine($"✅ Serveur trouvé: {serverName} ({ipAddress}:{port})");
                        }
                    }
                }
                catch (SocketException)
                {
                    // Timeout sur ReceiveAsync, continuer
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erreur lors de la découverte: {ex.Message}");
        }
        
        return servers;
    }
    
    /// <summary>
    /// Trouve le premier serveur disponible
    /// </summary>
    public async Task<(string IpAddress, int Port)?> FindFirstServerAsync(int timeout = 3000)
    {
        var servers = await FindServersAsync(timeout);
        if (servers.Count > 0)
        {
            return (servers[0].IpAddress, servers[0].Port);
        }
        return null;
    }
}
