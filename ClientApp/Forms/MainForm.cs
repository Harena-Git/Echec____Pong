using System.Drawing;
using System.Windows.Forms;
using ClientApp.Network;
using ClientApp.Game;
using ClientApp.Client;

namespace ClientApp.Forms;

public class MainForm : Form
{
    private GameClient? _gameClient;
    private GameManager? _gameManager;
    private LocalGameMode? _localGameMode;
    private bool _isLocalMode = false;
    private string? _playerName;
    private int? _playerId;
    private string? _playerSide;
    private int _numberOfColumns = 8;
    
    // Panels pour différentes pages
    private Panel? _nameInputPanel;
    private Panel? _configPanel;
    private Panel? _gamePanel;
    
    // Contrôles pour Page 1 (Saisie du nom)
    private TextBox? _nameTextBox;
    private Button? _nameSubmitButton;
    private Label? _statusLabel;
    
    // Contrôles pour Page 2 (Configuration)
    private NumericUpDown? _columnsSelector;
    private Button? _startGameButton;
    private PictureBox? _previewPictureBox;
    
    // Contrôles pour Page 3 (Jeu)
    private PictureBox? _gameCanvas;
    private Label? _scoreLabel;
    private Label? _targetingLabel;
    private System.Windows.Forms.Timer? _renderTimer;
    
    // État du clavier
    private bool _leftPressed = false;
    private bool _rightPressed = false;
    
    // Constructeur pour mode réseau
    public MainForm(GameClient gameClient, GameManager gameManager, string playerName)
    {
        _gameClient = gameClient;
        _gameManager = gameManager;
        _playerName = playerName;
        _isLocalMode = false;
        
        InitializeComponent();
        SetupEventHandlers();
        ShowNameInputPage();
    }
    
    // Constructeur pour mode local (sans serveur)
    public MainForm()
    {
        _isLocalMode = true;
        _localGameMode = new LocalGameMode();
        _playerName = "Joueur Local";
        
        InitializeComponent();
        SetupLocalModeEventHandlers();
        ShowGamePage(); // Aller directement au jeu
    }
    
    private void InitializeComponent()
    {
        // Configuration de la fenêtre principale (taille réduite pour petits écrans)
        Text = "Échec-Pong - Client";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.FromArgb(45, 45, 48);
        
        // Créer les panels
        _nameInputPanel = CreateNameInputPanel();
        _configPanel = CreateConfigPanel();
        _gamePanel = CreateGamePanel();
        
        Controls.Add(_nameInputPanel);
        Controls.Add(_configPanel);
        Controls.Add(_gamePanel);
        
        // Masquer tous les panels au départ
        _nameInputPanel.Visible = false;
        _configPanel.Visible = false;
        _gamePanel.Visible = false;
    }
    
    private Panel CreateNameInputPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 45, 48)
        };
        
        var titleLabel = new Label
        {
            Text = "ÉCHEC-PONG - HYBRIDE",
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(250, 100)
        };
        
        var subtitleLabel = new Label
        {
            Text = "PAGE 1: SAISIE DU NOM",
            Font = new Font("Segoe UI", 14),
            ForeColor = Color.LightGray,
            AutoSize = true,
            Location = new Point(310, 150)
        };
        
        var nameLabel = new Label
        {
            Text = "Entrez votre nom:",
            Font = new Font("Segoe UI", 12),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(300, 250)
        };
        
        _nameTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 14),
            Location = new Point(300, 280),
            Size = new Size(300, 35),
            Text = _playerName ?? ""
        };
        
        _nameSubmitButton = new Button
        {
            Text = "REJOINDRE",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Location = new Point(350, 340),
            Size = new Size(200, 45),
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _nameSubmitButton.FlatAppearance.BorderSize = 0;
        
        _statusLabel = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.Yellow,
            AutoSize = true,
            Location = new Point(300, 400)
        };
        
        panel.Controls.Add(titleLabel);
        panel.Controls.Add(subtitleLabel);
        panel.Controls.Add(nameLabel);
        panel.Controls.Add(_nameTextBox);
        panel.Controls.Add(_nameSubmitButton);
        panel.Controls.Add(_statusLabel);
        
        return panel;
    }
    
    private Panel CreateConfigPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 45, 48)
        };
        
        var titleLabel = new Label
        {
            Text = "CONFIGURATION DU JEU",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(280, 50)
        };
        
        var columnsLabel = new Label
        {
            Text = "Nombre de colonnes de pions (2-8):",
            Font = new Font("Segoe UI", 12),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(100, 120)
        };
        
        _columnsSelector = new NumericUpDown
        {
            Font = new Font("Segoe UI", 14),
            Location = new Point(400, 115),
            Size = new Size(80, 35),
            Minimum = 2,
            Maximum = 8,
            Value = 8
        };
        
        _previewPictureBox = new PictureBox
        {
            Location = new Point(100, 180),
            Size = new Size(600, 250),
            BackColor = Color.FromArgb(30, 30, 30),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        _startGameButton = new Button
        {
            Text = "LANCER LA PARTIE",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Location = new Point(300, 560),
            Size = new Size(300, 50),
            BackColor = Color.FromArgb(16, 124, 16),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _startGameButton.FlatAppearance.BorderSize = 0;
        
        panel.Controls.Add(titleLabel);
        panel.Controls.Add(columnsLabel);
        panel.Controls.Add(_columnsSelector);
        panel.Controls.Add(_previewPictureBox);
        panel.Controls.Add(_startGameButton);
        
        return panel;
    }
    
    private Panel CreateGamePanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 45, 48)
        };
        
        _scoreLabel = new Label
        {
            Text = "Nord: 0  |  Sud: 0",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(350, 10)
        };
        
        _gameCanvas = new PictureBox
        {
            Location = new Point(40, 50),
            Size = new Size(680, 420),
            BackColor = Color.Black,
            BorderStyle = BorderStyle.FixedSingle
        };
        
        _targetingLabel = new Label
        {
            Text = "Ciblage: --",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.Yellow,
            AutoSize = true,
            Location = new Point(50, 610)
        };
        
        var controlsLabel = new Label
        {
            Text = "← → : Déplacer raquette | FRAPPE AUTOMATIQUE sur collision | Q : Quitter",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.LightGray,
            AutoSize = true,
            Location = new Point(50, 640)
        };
        
        panel.Controls.Add(_scoreLabel);
        panel.Controls.Add(_gameCanvas);
        panel.Controls.Add(_targetingLabel);
        panel.Controls.Add(controlsLabel);
        
        return panel;
    }
    
    private void SetupEventHandlers()
    {
        if (_nameSubmitButton != null)
        {
            _nameSubmitButton.Click += NameSubmitButton_Click;
        }
        
        if (_startGameButton != null)
        {
            _startGameButton.Click += StartGameButton_Click;
        }
        
        if (_columnsSelector != null)
        {
            _columnsSelector.ValueChanged += ColumnsSelector_ValueChanged;
        }
        
        if (_gameClient != null)
        {
            _gameClient.OnMessageReceived += OnMessageReceived;
        }
        
        if (_gameManager != null)
        {
            _gameManager.OnGameStateUpdated += OnGameStateUpdated;
            _gameManager.OnGameEvent += (msg) =>
            {
                if (_statusLabel == null) return;
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        _statusLabel.Text = msg;
                        _statusLabel.ForeColor = Color.Yellow;
                    }));
                }
                else
                {
                    _statusLabel.Text = msg;
                    _statusLabel.ForeColor = Color.Yellow;
                }
            };
        }
        
        // Gestion du clavier
        KeyPreview = true;
        KeyDown += MainForm_KeyDown;
        KeyUp += MainForm_KeyUp;
    }
    
    private void SetupLocalModeEventHandlers()
    {
        if (_localGameMode != null)
        {
            _localGameMode.OnGameStateUpdated += (state) =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdateLocalGameUI(state)));
                }
                else
                {
                    UpdateLocalGameUI(state);
                }
            };
        }
        
        // Gestion du clavier
        KeyPreview = true;
        KeyDown += MainForm_KeyDown;
        KeyUp += MainForm_KeyUp;
    }
    
    private void UpdateLocalGameUI(GameState state)
    {
        // Mettre à jour les labels de score
        if (_scoreLabel != null)
        {
            _scoreLabel.Text = $"Nord: {state.Match.ScoreNorth}  |  Sud: {state.Match.ScoreSouth}";
        }
        
        // Vérifier fin de partie
        if (state.Match.Status == "finished")
        {
            string winner = state.Match.ScoreNorth > state.Match.ScoreSouth ? "Nord" : "Sud";
            MessageBox.Show($"Partie terminée ! Gagnant: {winner}", "Fin de partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    
    private void ShowNameInputPage()
    {
        if (_nameInputPanel != null) _nameInputPanel.Visible = true;
        if (_configPanel != null) _configPanel.Visible = false;
        if (_gamePanel != null) _gamePanel.Visible = false;
    }
    
    private void ShowConfigPage()
    {
        if (_nameInputPanel != null) _nameInputPanel.Visible = false;
        if (_configPanel != null) _configPanel.Visible = true;
        if (_gamePanel != null) _gamePanel.Visible = false;
        
        RenderChessboardPreview();
    }
    
    private void ShowGamePage()
    {
        if (_nameInputPanel != null) _nameInputPanel.Visible = false;
        if (_configPanel != null) _configPanel.Visible = false;
        if (_gamePanel != null) _gamePanel.Visible = true;
        
        // Démarrer le timer de rendu
        _renderTimer = new System.Windows.Forms.Timer { Interval = 50 }; // 20 FPS
        _renderTimer.Tick += RenderTimer_Tick;
        _renderTimer.Start();
    }
    
    private async void NameSubmitButton_Click(object? sender, EventArgs e)
    {
        if (_nameTextBox == null || _gameClient == null || _statusLabel == null || _nameSubmitButton == null) return;
        _nameSubmitButton.Enabled = false;

        _playerName = _nameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(_playerName))
        {
            _playerName = "Player" + Random.Shared.Next(1000, 9999);
            _nameTextBox.Text = _playerName;
        }

        _statusLabel.Text = "⏳ Envoi de la demande de connexion...";
        _statusLabel.ForeColor = Color.Yellow;

        var joinRequest = new JoinRequestMessage { PlayerName = _playerName };
        try
        {
            await _gameClient.SendMessageAsync(joinRequest);
            _statusLabel.Text = "⏳ En attente de la réponse du serveur...";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"❌ Erreur en envoyant la demande: {ex.Message}";
            _statusLabel.ForeColor = Color.Red;
            _nameSubmitButton.Enabled = true;
        }
    }
    
    private async void StartGameButton_Click(object? sender, EventArgs e)
    {
        if (_gameClient == null || _playerId == null) return;
        
        var configMessage = new GameConfigMessage
        {
            PlayerId = _playerId.Value,
            NumberOfColumns = _numberOfColumns
        };
        
        await _gameClient.SendMessageAsync(configMessage);
    }
    
    private void ColumnsSelector_ValueChanged(object? sender, EventArgs e)
    {
        if (_columnsSelector != null)
        {
            _numberOfColumns = (int)_columnsSelector.Value;
            RenderChessboardPreview();
        }
    }
    
    private void OnMessageReceived(GameMessage message)
    {
        // Invoke sur le thread UI
        if (InvokeRequired)
        {
            Invoke(new Action<GameMessage>(OnMessageReceived), message);
            return;
        }
        
        if (message is JoinResponseMessage joinResponse)
        {
            HandleJoinResponse(joinResponse);
        }
        else if (message is GameStateUpdateMessage stateUpdate)
        {
            // Le GameManager gère déjà cela
            if (stateUpdate.GameState.Match.Status == "playing" && _gamePanel != null && !_gamePanel.Visible)
            {
                ShowGamePage();
            }
        }
    }
    
    private void HandleJoinResponse(JoinResponseMessage response)
    {
        if (_statusLabel == null) return;
        if (response.Success)
        {
            _playerId = response.PlayerId;
            _playerSide = response.Side;

            _statusLabel.Text = $"✅ Connecté en tant que {response.PlayerName} ({response.Side})";
            _statusLabel.ForeColor = Color.LimeGreen;

            // Désactiver le bouton de soumission pour éviter les doubles envois
            if (_nameSubmitButton != null) _nameSubmitButton.Enabled = false;

            // Attendre 1 seconde puis afficher la page appropriée
            var timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                timer.Dispose();

                if (response.Side == "north")
                {
                    ShowConfigPage();
                }
                else
                {
                    _statusLabel.Text = "⏳ En attente de la configuration par le Joueur 1...";
                }
            };
            timer.Start();
        }
        else
        {
            _statusLabel.Text = $"❌ {response.ErrorMessage ?? "Connexion refusée"}";
            _statusLabel.ForeColor = Color.Red;
            if (_nameSubmitButton != null) _nameSubmitButton.Enabled = true;
        }
    }
    
    private void OnGameStateUpdated(GameState state)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<GameState>(OnGameStateUpdated), state);
            return;
        }
        
        // Mettre à jour les labels de score
        if (_scoreLabel != null)
        {
            _scoreLabel.Text = $"Nord: {state.Match.ScoreNorth}  |  Sud: {state.Match.ScoreSouth}";
        }
    }
    
    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Left)
        {
            _leftPressed = true;
        }
        else if (e.KeyCode == Keys.Right)
        {
            _rightPressed = true;
        }
        else if (e.KeyCode == Keys.Q)
        {
            Close();
        }
    }
    
    private void MainForm_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Left)
        {
            _leftPressed = false;
        }
        else if (e.KeyCode == Keys.Right)
        {
            _rightPressed = false;
        }
    }
    
    private void RenderTimer_Tick(object? sender, EventArgs e)
    {
        // Mode local
        if (_isLocalMode && _localGameMode != null)
        {
            // Gérer le mouvement de la raquette
            var northPlayer = _localGameMode.CurrentState.Players.FirstOrDefault(p => p.Side == "north");
            if (northPlayer != null)
            {
                if (_leftPressed)
                {
                    float newX = Math.Clamp(northPlayer.PositionX - 0.02f, 0f, 1f);
                    _localGameMode.UpdatePlayerPosition(newX);
                }
                else if (_rightPressed)
                {
                    float newX = Math.Clamp(northPlayer.PositionX + 0.02f, 0f, 1f);
                    _localGameMode.UpdatePlayerPosition(newX);
                }
            }
            
            // Mettre à jour la physique du jeu
            _localGameMode.Update();
            
            // Redessiner
            RenderGameLocal(_localGameMode.CurrentState);
            return;
        }
        
        // Mode réseau
        if (_gameManager?.LocalPlayer != null)
        {
            if (_leftPressed)
            {
                float newX = Math.Clamp(_gameManager.LocalPlayer.PositionX - 0.02f, 0f, 1f);
                _gameManager.UpdatePlayerPosition(newX, 0f);
            }
            else if (_rightPressed)
            {
                float newX = Math.Clamp(_gameManager.LocalPlayer.PositionX + 0.02f, 0f, 1f);
                _gameManager.UpdatePlayerPosition(newX, 0f);
            }
        }
        
        // Redessiner le jeu
        RenderGame();
    }
    
    private void RenderChessboardPreview()
    {
        if (_previewPictureBox == null) return;
        
        var bmp = new Bitmap(_previewPictureBox.Width, _previewPictureBox.Height);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.FromArgb(30, 30, 30));
            
            string[] pieces = { "♜", "♞", "♝", "♛", "♚", "♝", "♞", "♜" };
            string pawn = "♟";
            
            int cellWidth = 80;
            int startX = 50;
            int startY = 100;
            
            var font = new Font("Segoe UI", 32);
            var brush = new SolidBrush(Color.White);
            
            // Rangée arrière
            for (int i = 0; i < _numberOfColumns; i++)
            {
                g.DrawString(pieces[i], font, brush, startX + i * cellWidth, startY);
            }
            
            // Rangée de pions
            for (int i = 0; i < _numberOfColumns; i++)
            {
                g.DrawString(pawn, font, brush, startX + i * cellWidth, startY + 80);
            }
            
            // Légende
            var legendFont = new Font("Segoe UI", 10);
            g.DrawString($"Configuration: {_numberOfColumns} colonnes", legendFont, brush, startX, startY + 180);
        }
        
        _previewPictureBox.Image?.Dispose();
        _previewPictureBox.Image = bmp;
    }
    
    private void RenderGameLocal(GameState state)
    {
        if (_gameCanvas == null || state == null) return;
        
        var bmp = new Bitmap(_gameCanvas.Width, _gameCanvas.Height);
        
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.FromArgb(20, 20, 20));
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            int width = _gameCanvas.Width;
            int height = _gameCanvas.Height;
            int numCols = state.NumberOfColumns;
            
            // Zone Nord (haut)
            DrawChessboard(g, state.PiecesNorth, 20, 20, width - 40, 120, numCols, false);
            
            // Zone de jeu (milieu)
            int gameZoneY = 160;
            int gameZoneHeight = 180;
            
            // Raquette Nord
            var northPlayer = state.Players.FirstOrDefault(p => p.Side == "north");
            if (northPlayer != null)
            {
                DrawPaddle(g, northPlayer, gameZoneY, width, numCols, Color.Cyan);
            }
            
            // Filet
            using (var pen = new Pen(Color.White, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
            {
                g.DrawLine(pen, 0, gameZoneY + gameZoneHeight / 2, width, gameZoneY + gameZoneHeight / 2);
            }
            
            // Balle
            if (state.Ball != null && state.Ball.State == "moving")
            {
                int ballX = (int)(state.Ball.PositionX * width);
                int ballY = gameZoneY + (int)(state.Ball.PositionY * gameZoneHeight);
                g.FillEllipse(Brushes.Yellow, ballX - 8, ballY - 8, 16, 16);
            }
            
            // Raquette Sud
            var southPlayer = state.Players.FirstOrDefault(p => p.Side == "south");
            if (southPlayer != null)
            {
                DrawPaddle(g, southPlayer, gameZoneY + gameZoneHeight - 15, width, numCols, Color.Orange);
            }
            
            // Zone Sud (bas)
            DrawChessboard(g, state.PiecesSouth, 20, height - 140, width - 40, 120, numCols, true);
        }
        
        _gameCanvas.Image?.Dispose();
        _gameCanvas.Image = bmp;
    }
    
    private void RenderGame()
    {
        if (_gameCanvas == null || _gameManager?.CurrentState == null) return;
        
        var state = _gameManager.CurrentState;
        var bmp = new Bitmap(_gameCanvas.Width, _gameCanvas.Height);
        
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.FromArgb(20, 20, 20));
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            int width = _gameCanvas.Width;
            int height = _gameCanvas.Height;
            int numCols = state.NumberOfColumns;
            
            // Zone Nord (haut)
            DrawChessboard(g, state.PiecesNorth, 20, 20, width - 40, 150, numCols, false);
            
            // Zone de jeu (milieu)
            int gameZoneY = 190;
            int gameZoneHeight = 200;
            
            // Raquette Nord
            var northPlayer = state.Players.FirstOrDefault(p => p.Side == "north");
            if (northPlayer != null)
            {
                DrawPaddle(g, northPlayer, gameZoneY, width, numCols, Color.Cyan);
            }
            
            // Filet
            using (var pen = new Pen(Color.White, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
            {
                g.DrawLine(pen, 0, gameZoneY + gameZoneHeight / 2, width, gameZoneY + gameZoneHeight / 2);
            }
            
            // Balle
            if (state.Ball != null && state.Ball.State == "moving")
            {
                int ballX = (int)(state.Ball.PositionX * width);
                int ballY = gameZoneY + (int)(state.Ball.PositionY * gameZoneHeight);
                g.FillEllipse(Brushes.Yellow, ballX - 8, ballY - 8, 16, 16);
            }
            
            // Raquette Sud
            var southPlayer = state.Players.FirstOrDefault(p => p.Side == "south");
            if (southPlayer != null)
            {
                DrawPaddle(g, southPlayer, gameZoneY + gameZoneHeight - 15, width, numCols, Color.Orange);
            }
            
            // Zone Sud (bas)
            DrawChessboard(g, state.PiecesSouth, 20, height - 170, width - 40, 150, numCols, true);
        }
        
        _gameCanvas.Image?.Dispose();
        _gameCanvas.Image = bmp;
    }
    
    private void DrawChessboard(Graphics g, List<PieceState> pieces, int x, int y, int width, int height, int numCols, bool isSouth)
    {
        var font = new Font("Segoe UI", 24);
        int cellWidth = width / numCols;
        
        // Dessiner les pièces
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < numCols; col++)
            {
                var piece = pieces.FirstOrDefault(p => p.Column == col && p.Row == row && p.IsAlive);
                if (piece != null)
                {
                    string symbol = GetPieceSymbol(piece.Type, row == 1);
                    var brush = piece.CurrentHealth > 0 ? Brushes.White : Brushes.Gray;
                    
                    int pieceX = x + col * cellWidth + cellWidth / 4;
                    int pieceY = y + (isSouth ? row : (1 - row)) * (height / 2) + 10;
                    
                    g.DrawString(symbol, font, brush, pieceX, pieceY);
                    
                    // Afficher les vies
                    if (piece.CurrentHealth > 0)
                    {
                        var smallFont = new Font("Segoe UI", 10);
                        string hearts = new string('♥', piece.CurrentHealth);
                        g.DrawString(hearts, smallFont, Brushes.Red, pieceX + 30, pieceY + 5);
                    }
                }
            }
        }
    }
    
    private void DrawPaddle(Graphics g, PlayerState player, int y, int totalWidth, int numCols, Color color)
    {
        int paddleWidth = (int)(player.PaddleWidth * totalWidth);
        int paddleX = (int)(player.PositionX * totalWidth) - paddleWidth / 2;
        
        using (var brush = new SolidBrush(color))
        {
            g.FillRectangle(brush, paddleX, y, paddleWidth, 10);
        }
    }
    
    private string GetPieceSymbol(string type, bool isBackRow)
    {
        return type switch
        {
            "king" => isBackRow ? "♔" : "♚",
            "queen" => isBackRow ? "♕" : "♛",
            "rook" => isBackRow ? "♖" : "♜",
            "bishop" => isBackRow ? "♗" : "♝",
            "knight" => isBackRow ? "♘" : "♞",
            "pawn" => isBackRow ? "♙" : "♟",
            _ => "?"
        };
    }
    
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _renderTimer?.Stop();
        _renderTimer?.Dispose();
        _gameClient?.Disconnect();
        base.OnFormClosing(e);
    }
}
