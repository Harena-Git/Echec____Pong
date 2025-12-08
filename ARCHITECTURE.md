# Architecture du Projet - Jeu Échecs/Ping-Pong en Réseau

## Vue d'ensemble

**Technologies :**
- C# (.NET 9.0)
- PostgreSQL (via Npgsql)
- TCP/IP pour la communication réseau
- Console ou WPF pour l'interface

---

## Structure du Projet

```
Echec____Pong/
│
├── ServerApp/                    # Application Serveur
│   ├── Program.cs               # Point d'entrée serveur
│   ├── Server/
│   │   ├── GameServer.cs        # Serveur TCP principal
│   │   ├── ClientHandler.cs     # Gestion de chaque client
│   │   └── ServerState.cs       # État du serveur (peut devenir client)
│   │
│   ├── GameLogic/
│   │   ├── GameEngine.cs        # Moteur de jeu principal
│   │   ├── GameState.cs         # État actuel du jeu
│   │   ├── Player.cs            # Représentation d'un joueur
│   │   └── GameRules.cs         # Règles du jeu (Échecs + Ping-Pong)
│   │
│   ├── Database/
│   │   ├── DbConnection.cs      # Connexion PostgreSQL
│   │   ├── GameRepository.cs    # Accès aux données de jeu
│   │   └── PlayerRepository.cs  # Accès aux données joueurs
│   │
│   └── Models/
│       ├── GameData.cs          # Modèle de données jeu
│       └── PlayerData.cs        # Modèle de données joueur
│
├── ClientApp/                    # Application Client
│   ├── Program.cs               # Point d'entrée client
│   ├── Client/
│   │   ├── GameClient.cs        # Client TCP principal
│   │   └── NetworkManager.cs    # Gestion communication réseau
│   │
│   ├── Input/
│   │   └── KeyboardHandler.cs   # Gestion des entrées clavier
│   │
│   ├── Render/
│   │   └── GameRenderer.cs      # Affichage du jeu
│   │
│   └── Models/
│       └── GameState.cs         # État du jeu côté client
│
└── Shared/                       # Code partagé (optionnel)
    ├── Protocol/
    │   └── MessageProtocol.cs   # Protocole de communication
    └── Models/
        └── NetworkMessage.cs    # Messages réseau
```

---

## Architecture Orientée Objet

### 1. **ServeurApp**

#### `GameServer.cs`
```csharp
- Propriétés : TcpListener, List<ClientHandler>, GameEngine
- Méthodes : Start(), AcceptClients(), BroadcastMessage()
- Responsabilité : Accepter connexions, gérer clients, orchestrer le jeu
```

#### `ClientHandler.cs`
```csharp
- Propriétés : TcpClient, NetworkStream, Player
- Méthodes : HandleClient(), SendMessage(), ReceiveMessage()
- Responsabilité : Communication avec un client spécifique
```

#### `GameEngine.cs`
```csharp
- Propriétés : GameState, List<Player>, GameRules
- Méthodes : UpdateGame(), ProcessMove(), CheckWinCondition()
- Responsabilité : Logique métier du jeu
```

#### `DbConnection.cs`
```csharp
- Propriétés : NpgsqlConnection
- Méthodes : Connect(), Disconnect(), ExecuteQuery()
- Responsabilité : Connexion et gestion PostgreSQL
```

#### `GameRepository.cs`
```csharp
- Propriétés : DbConnection
- Méthodes : SaveGameState(), GetGameState(), UpdateGameData()
- Responsabilité : Persistance des données de jeu
```

---

### 2. **ClientApp**

#### `GameClient.cs`
```csharp
- Propriétés : TcpClient, NetworkStream, GameState
- Méthodes : Connect(), SendInput(), ReceiveUpdate(), Disconnect()
- Responsabilité : Communication avec le serveur
```

#### `KeyboardHandler.cs`
```csharp
- Propriétés : Dictionary<ConsoleKey, Action>
- Méthodes : ListenInput(), ProcessKeyPress()
- Responsabilité : Capture et traitement des entrées clavier
```

#### `GameRenderer.cs`
```csharp
- Propriétés : GameState
- Méthodes : Render(), UpdateDisplay(), ClearScreen()
- Responsabilité : Affichage du jeu à l'écran
```

---

## Communication Réseau

### Protocole de Messages

**Format JSON :**
```json
{
  "Type": "MOVE|GAME_STATE|INPUT|CONNECTION",
  "Data": { ... },
  "Timestamp": "2024-01-01T12:00:00"
}
```

**Types de messages :**
- `CONNECTION` : Connexion/déconnexion client
- `INPUT` : Entrée clavier du client
- `GAME_STATE` : État complet du jeu (serveur → clients)
- `MOVE` : Mouvement validé
- `ERROR` : Message d'erreur

---

## Base de Données PostgreSQL

### Tables (à créer manuellement)

```sql
-- Table des parties
CREATE TABLE games (
    id SERIAL PRIMARY KEY,
    created_at TIMESTAMP DEFAULT NOW(),
    status VARCHAR(50),  -- 'waiting', 'playing', 'finished'
    winner_id INTEGER,
    game_data JSONB      -- État du jeu en JSON
);

-- Table des joueurs
CREATE TABLE players (
    id SERIAL PRIMARY KEY,
    username VARCHAR(100),
    connected_at TIMESTAMP,
    game_id INTEGER REFERENCES games(id)
);

-- Table des mouvements
CREATE TABLE moves (
    id SERIAL PRIMARY KEY,
    game_id INTEGER REFERENCES games(id),
    player_id INTEGER REFERENCES players(id),
    move_data JSONB,
    timestamp TIMESTAMP DEFAULT NOW()
);
```

### Accès via Repository Pattern

- **GameRepository** : CRUD sur `games` et `moves`
- **PlayerRepository** : CRUD sur `players`
- Utilisation de **Npgsql** pour les requêtes

---

## Flux de Communication

```
1. Client se connecte → Serveur
   └─> Serveur crée ClientHandler
   └─> Serveur enregistre dans DB

2. Client envoie INPUT (clavier) → Serveur
   └─> GameEngine traite le mouvement
   └─> Serveur sauvegarde dans DB
   └─> Serveur envoie GAME_STATE → Tous les clients

3. Client reçoit GAME_STATE
   └─> GameRenderer met à jour l'affichage

4. Boucle continue jusqu'à fin de partie
```

---

## Points Importants

### Serveur peut devenir Client
- Le serveur peut lancer une instance `GameClient` en interne
- Utiliser 2 fenêtres distinctes (Console ou WPF)
- Même code client, connexion locale (localhost)

### Visualisation Live des Données
- Serveur peut exposer une API REST (optionnel) ou console
- Requêtes SQL directes sur PostgreSQL
- Affichage en temps réel via polling ou événements

### Gestion Multi-Clients
- Serveur maintient une `List<ClientHandler>`
- Broadcast des mises à jour à tous les clients connectés
- Synchronisation via GameState unique côté serveur

---

## Dépendances NuGet

### ServerApp
```
- Npgsql (PostgreSQL)
- Newtonsoft.Json (sérialisation messages)
```

### ClientApp
```
- Newtonsoft.Json (sérialisation messages)
```

---

## Points d'Entrée

### ServerApp/Program.cs
```csharp
- Initialise GameServer
- Configure connexion PostgreSQL
- Démarre écoute TCP
- Lance boucle principale
```

### ClientApp/Program.cs
```csharp
- Initialise GameClient
- Se connecte au serveur
- Lance KeyboardHandler et GameRenderer
- Boucle de jeu
```

---

## Prochaines Étapes

1. Créer la structure de dossiers
2. Implémenter les classes de base (sans logique métier)
3. Tester la connexion réseau TCP
4. Tester la connexion PostgreSQL
5. Implémenter la logique de jeu
6. Intégrer l'affichage

