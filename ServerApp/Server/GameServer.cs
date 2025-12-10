using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ServerApp.Database;

namespace ServerApp.Server;

/// <summary>
/// Serveur TCP principal qui accepte les connexions clients
/// et orchestre le jeu.
/// </summary>
public class GameServer
{
    private readonly int _port;
    private TcpListener? _tcpListener;
    private readonly List<ClientHandler> _clients = new();
    private CancellationTokenSource? _cts;
    private readonly object _lock = new();
    private readonly DbContextOptions<DatabaseContext> _dbOptions;

    public GameServer(int port, DbContextOptions<DatabaseContext> dbOptions)
    {
        _port = port;
        _dbOptions = dbOptions;
    }

    /// <summary>
    /// Démarre le serveur et commence à accepter les connexions.
    /// </summary>
    public void Start()
    {
        if (_tcpListener != null)
            return;

        _cts = new CancellationTokenSource();
        _tcpListener = new TcpListener(IPAddress.Any, _port);
        _tcpListener.Start();

        Console.WriteLine($"[Server] Listening on port {_port}...");
        _ = Task.Run(AcceptClientsAsync);
    }

    /// <summary>
    /// Boucle d'acceptation des clients.
    /// </summary>
    private async Task AcceptClientsAsync()
    {
        if (_tcpListener == null || _cts == null)
            return;

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var tcpClient = await _tcpListener.AcceptTcpClientAsync(_cts.Token);
                var handler = new ClientHandler(tcpClient, this);

                lock (_lock)
                {
                    _clients.Add(handler);
                }

                Console.WriteLine($"[Server] Client connected ({tcpClient.Client.RemoteEndPoint})");
                _ = handler.HandleClientAsync();
            }
            catch (OperationCanceledException)
            {
                // Arrêt demandé, sortir proprement
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Accept error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Envoie un message JSON à tous les clients connectés.
    /// </summary>
    public async Task BroadcastMessageAsync(string message)
    {
        List<ClientHandler> snapshot;
        lock (_lock)
        {
            snapshot = _clients.ToList();
        }

        foreach (var client in snapshot)
        {
            await client.SendMessageAsync(message);
        }
    }

    /// <summary>
    /// Supprime un client de la liste (déconnexion).
    /// </summary>
    internal void RemoveClient(ClientHandler client)
    {
        lock (_lock)
        {
            _clients.Remove(client);
        }
    }

    /// <summary>
    /// Arrête le serveur et ferme toutes les connexions.
    /// </summary>
    public void Stop()
    {
        _cts?.Cancel();
        _cts = null;

        lock (_lock)
        {
            foreach (var client in _clients.ToList())
            {
                client.Disconnect();
            }
            _clients.Clear();
        }

        _tcpListener?.Stop();
        _tcpListener = null;
        Console.WriteLine("[Server] Stopped.");
    }
}

