using System.Net.Sockets;
using System.Text;
using ServerApp.Network;

namespace ServerApp.Server;

/// <summary>
/// Gère la communication avec un client spécifique.
/// </summary>
public class ClientHandler
{
    private readonly TcpClient _tcpClient;
    private readonly GameServer _server;
    private readonly NetworkStream _stream;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private readonly CancellationTokenSource _cts = new();

    public ClientHandler(TcpClient client, GameServer server)
    {
        _tcpClient = client;
        _server = server;
        _stream = client.GetStream();
        _reader = new StreamReader(_stream, Encoding.UTF8);
        _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
    }
    
    /// <summary>
    /// Boucle principale de gestion du client.
    /// </summary>
    public async Task HandleClientAsync()
    {
        try
        {
            while (!_cts.IsCancellationRequested && _tcpClient.Connected)
            {
                var line = await _reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var message = GameMessage.FromJson(line);
                if (message == null)
                    continue;

                // Pour l'instant : echo/broadcast des messages reçus
                await _server.BroadcastMessageAsync(line);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ClientHandler] Error: {ex.Message}");
        }
        finally
        {
            Disconnect();
            _server.RemoveClient(this);
        }
    }
    
    /// <summary>
    /// Envoie un message JSON au client.
    /// </summary>
    public async Task SendMessageAsync(string message)
    {
        try
        {
            await _writer.WriteLineAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ClientHandler] Send error: {ex.Message}");
            Disconnect();
        }
    }
    
    /// <summary>
    /// Ferme la connexion avec le client.
    /// </summary>
    public void Disconnect()
    {
        _cts.Cancel();
        try { _reader.Close(); } catch { }
        try { _writer.Close(); } catch { }
        try { _stream.Close(); } catch { }
        try { _tcpClient.Close(); } catch { }
    }
}

