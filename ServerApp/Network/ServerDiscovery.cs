using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerApp.Network;

/// <summary>
/// Service de découverte réseau UDP pour permettre aux clients
/// de trouver automatiquement le serveur sur le réseau local
/// </summary>
public class ServerDiscovery
{
    private readonly int _discoveryPort;
    private readonly int _gameServerPort;
    private UdpClient? _udpListener;
    private CancellationTokenSource? _cts;
    private bool _isRunning;
    
    public ServerDiscovery(int discoveryPort = 7778, int gameServerPort = 7777)
    {
        _discoveryPort = discoveryPort;
        _gameServerPort = gameServerPort;
    }
    
    /// <summary>
    /// Démarre le service de découverte
    /// </summary>
    public void Start()
    {
        if (_isRunning) return;
        
        _isRunning = true;
        _cts = new CancellationTokenSource();
        _udpListener = new UdpClient(_discoveryPort);
        
        Console.WriteLine($"🔍 Service de découverte UDP démarré sur le port {_discoveryPort}");
        _ = Task.Run(() => ListenForDiscoveryRequests(_cts.Token));
    }
    
    /// <summary>
    /// Arrête le service de découverte
    /// </summary>
    public void Stop()
    {
        if (!_isRunning) return;
        
        _isRunning = false;
        _cts?.Cancel();
        _udpListener?.Close();
        Console.WriteLine("🔍 Service de découverte arrêté");
    }
    
    /// <summary>
    /// Écoute les requêtes de découverte et répond avec les informations du serveur
    /// </summary>
    private async Task ListenForDiscoveryRequests(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _udpListener != null)
        {
            try
            {
                var result = await _udpListener.ReceiveAsync(cancellationToken);
                var message = Encoding.UTF8.GetString(result.Buffer);
                
                if (message == "ECHEC_PONG_DISCOVERY")
                {
                    // Répondre avec les informations du serveur
                    var serverInfo = $"ECHEC_PONG_SERVER:{_gameServerPort}:{Environment.MachineName}";
                    var responseData = Encoding.UTF8.GetBytes(serverInfo);
                    
                    await _udpListener.SendAsync(responseData, result.RemoteEndPoint, cancellationToken);
                    Console.WriteLine($"🔍 Réponse de découverte envoyée à {result.RemoteEndPoint}");
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine($"⚠️ Erreur dans le service de découverte: {ex.Message}");
                }
            }
        }
    }
}
