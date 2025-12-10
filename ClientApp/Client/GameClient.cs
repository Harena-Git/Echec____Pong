using System.Net.Sockets;
using System.Text;
using ClientApp.Network;

namespace ClientApp.Client;

public class GameClient
{
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private CancellationTokenSource? _cancellationTokenSource;
    
    public bool IsConnected => _tcpClient?.Connected ?? false;
    
    public event Action<GameMessage>? OnMessageReceived;
    public event Action? OnConnected;
    public event Action<string>? OnDisconnected;
    
    public async Task<bool> ConnectAsync(string ipAddress, int port)
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(ipAddress, port);
            
            _stream = _tcpClient.GetStream();
            _reader = new StreamReader(_stream, Encoding.UTF8);
            _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
            
            _cancellationTokenSource = new CancellationTokenSource();
            
            // Démarrer la réception de messages
            _ = Task.Run(() => ReceiveMessagesAsync(_cancellationTokenSource.Token));
            
            OnConnected?.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            OnDisconnected?.Invoke($"Connection failed: {ex.Message}");
            return false;
        }
    }
    
    public async Task SendMessageAsync(GameMessage message)
    {
        if (_writer == null) return;
        
        try
        {
            string json = message.ToJson();
            await _writer.WriteLineAsync(json);
        }
        catch (Exception ex)
        {
            OnDisconnected?.Invoke($"Send failed: {ex.Message}");
            Disconnect();
        }
    }
    
    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        if (_reader == null) return;
        
        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                string? json = await _reader.ReadLineAsync();
                if (string.IsNullOrEmpty(json)) continue;
                
                var message = GameMessage.FromJson(json);
                if (message != null)
                {
                    OnMessageReceived?.Invoke(message);
                }
            }
        }
        catch (Exception ex)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                OnDisconnected?.Invoke($"Connection lost: {ex.Message}");
                Disconnect();
            }
        }
    }
    
    public void Disconnect()
    {
        _cancellationTokenSource?.Cancel();
        
        _reader?.Close();
        _writer?.Close();
        _stream?.Close();
        _tcpClient?.Close();
        
        _reader = null;
        _writer = null;
        _stream = null;
        _tcpClient = null;
    }
}