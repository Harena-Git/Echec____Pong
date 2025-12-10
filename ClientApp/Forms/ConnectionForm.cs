using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using ClientApp.Client;
using ClientApp.Game;

namespace ClientApp.Forms;

public class ConnectionForm : Form
{
    private RadioButton? _discoveryRadio;
    private RadioButton? _localhostRadio;
    private RadioButton? _customRadio;
    private TextBox? _customIpTextBox;
    private Button? _connectButton;
    private Label? _statusLabel;
    
    public ConnectionForm()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        Text = "Échec-Pong - Connexion";
        Size = new Size(600, 500);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.FromArgb(45, 45, 48);
        
        var titleLabel = new Label
        {
            Text = "ÉCHEC-PONG",
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(150, 40)
        };
        
        var subtitleLabel = new Label
        {
            Text = "Connexion au serveur",
            Font = new Font("Segoe UI", 14),
            ForeColor = Color.LightGray,
            AutoSize = true,
            Location = new Point(200, 90)
        };
        
        var instructionLabel = new Label
        {
            Text = "Choisissez le type de connexion:",
            Font = new Font("Segoe UI", 12),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(50, 150)
        };
        
        _discoveryRadio = new RadioButton
        {
            Text = "Découverte automatique (UDP)",
            Font = new Font("Segoe UI", 11),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(80, 190),
            Checked = true
        };
        
        _localhostRadio = new RadioButton
        {
            Text = "Serveur local (127.0.0.1)",
            Font = new Font("Segoe UI", 11),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(80, 230)
        };
        
        _customRadio = new RadioButton
        {
            Text = "Adresse IP personnalisée:",
            Font = new Font("Segoe UI", 11),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(80, 270)
        };
        
        _customIpTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 11),
            Location = new Point(340, 268),
            Size = new Size(180, 30),
            Enabled = false
        };
        
        _connectButton = new Button
        {
            Text = "SE CONNECTER",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Location = new Point(175, 340),
            Size = new Size(250, 50),
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _connectButton.FlatAppearance.BorderSize = 0;
        _connectButton.Click += ConnectButton_Click;
        
        _statusLabel = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.Yellow,
            AutoSize = false,
            Size = new Size(500, 40),
            Location = new Point(50, 410),
            TextAlign = ContentAlignment.MiddleCenter
        };
        
        // Event handlers pour activer/désactiver la textbox
        _discoveryRadio.CheckedChanged += (s, e) =>
        {
            if (_customIpTextBox != null)
                _customIpTextBox.Enabled = false;
        };
        
        _localhostRadio.CheckedChanged += (s, e) =>
        {
            if (_customIpTextBox != null)
                _customIpTextBox.Enabled = false;
        };
        
        _customRadio.CheckedChanged += (s, e) =>
        {
            if (_customIpTextBox != null && _customRadio != null)
                _customIpTextBox.Enabled = _customRadio.Checked;
        };
        
        Controls.Add(titleLabel);
        Controls.Add(subtitleLabel);
        Controls.Add(instructionLabel);
        Controls.Add(_discoveryRadio);
        Controls.Add(_localhostRadio);
        Controls.Add(_customRadio);
        Controls.Add(_customIpTextBox);
        Controls.Add(_connectButton);
        Controls.Add(_statusLabel);
    }
    
    private async void ConnectButton_Click(object? sender, EventArgs e)
    {
        if (_connectButton == null || _statusLabel == null) return;
        
        _connectButton.Enabled = false;
        _statusLabel.Text = "🔍 Connexion en cours...";
        _statusLabel.ForeColor = Color.Yellow;
        
        string? serverIp = null;
        
        try
        {
            if (_discoveryRadio?.Checked == true)
            {
                _statusLabel.Text = "🔍 Recherche de serveurs sur le réseau local (UDP)...";
                serverIp = await DiscoverServerAsync();
                
                if (serverIp == null)
                {
                    _statusLabel.Text = "❌ Aucun serveur trouvé. Utilisez l'adresse IP manuelle.";
                    _statusLabel.ForeColor = Color.Red;
                    _connectButton.Enabled = true;
                    return;
                }
                
                _statusLabel.Text = $"✅ Serveur trouvé: {serverIp}";
            }
            else if (_localhostRadio?.Checked == true)
            {
                serverIp = "127.0.0.1";
            }
            else if (_customRadio?.Checked == true && _customIpTextBox != null)
            {
                serverIp = _customIpTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(serverIp))
                {
                    _statusLabel.Text = "❌ Veuillez saisir une adresse IP.";
                    _statusLabel.ForeColor = Color.Red;
                    _connectButton.Enabled = true;
                    return;
                }
            }
            
            if (serverIp == null)
            {
                _statusLabel.Text = "❌ Erreur: impossible de déterminer l'adresse du serveur.";
                _statusLabel.ForeColor = Color.Red;
                _connectButton.Enabled = true;
                return;
            }
            
            // Tenter la connexion TCP
            _statusLabel.Text = $"🔌 Connexion à {serverIp}:7777...";
            
            var gameClient = new GameClient();
            bool connected = await gameClient.ConnectAsync(serverIp, 7777);
            
            if (!connected)
            {
                _statusLabel.Text = $"❌ Impossible de se connecter à {serverIp}:7777";
                _statusLabel.ForeColor = Color.Red;
                _connectButton.Enabled = true;
                return;
            }
            
            _statusLabel.Text = "✅ Connecté! Ouverture de la fenêtre principale...";
            _statusLabel.ForeColor = Color.LimeGreen;
            
            // Créer le GameManager et l'initialiser
            var gameManager = new GameManager();
            gameManager.Initialize(gameClient);
            
            // Attendre un peu puis ouvrir la fenêtre principale
            await Task.Delay(500);
            
            var mainForm = new MainForm(gameClient, gameManager, "");
            mainForm.FormClosed += (s, args) => Close();
            mainForm.Show();
            Hide();
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"❌ Erreur: {ex.Message}";
            _statusLabel.ForeColor = Color.Red;
            _connectButton.Enabled = true;
        }
    }
    
    private async Task<string?> DiscoverServerAsync()
    {
        try
        {
            using var udpClient = new UdpClient();
            udpClient.Client.ReceiveTimeout = 5000; // 5 secondes timeout
            
            var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, 7778);
            var discoveryMessage = System.Text.Encoding.UTF8.GetBytes("DISCOVER_SERVER");
            
            await udpClient.SendAsync(discoveryMessage, discoveryMessage.Length, broadcastEndpoint);
            
            var receiveTask = udpClient.ReceiveAsync();
            if (await Task.WhenAny(receiveTask, Task.Delay(5000)) == receiveTask)
            {
                var result = await receiveTask;
                string response = System.Text.Encoding.UTF8.GetString(result.Buffer);
                
                if (response.StartsWith("SERVER_HERE:"))
                {
                    return result.RemoteEndPoint.Address.ToString();
                }
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }
}
