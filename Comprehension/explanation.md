# 📖 Explication du Projet - Échec-Pong Hybride

Ce document explique le fonctionnement du projet, l'architecture et le rôle de chaque fichier.

---

## 🎯 Concept du Jeu

**Échec-Pong Hybride** combine deux jeux classiques :

1. **Ping-Pong** : Deux joueurs s'affrontent avec une balle
2. **Échecs** : Chaque joueur a un échiquier derrière lui avec des pièces ayant des vies

**Mécanique principale :**
- La balle est frappée **automatiquement** quand elle croise la raquette du joueur
- Les joueurs déplacent leur raquette (rectangle) avec les flèches ← →
- Si un joueur rate la balle, celle-ci sort du terrain par une **colonne** (0-7)
- La balle touche alors la pièce d'échecs de l'adversaire dans cette colonne
- Chaque pièce a des **vies** (Roi: 3♥, Reine: 2♥, etc.)
- Si une pièce perd toutes ses vies, elle est éliminée
- **Victoire** : Capturer le Roi adverse OU atteindre 11 points au Ping-Pong

**Interface Multi-Page :**
1. **Page 1** : Saisie des noms des joueurs (chaque fenêtre)
2. **Page 2** : Configuration du jeu - Le Joueur 1 choisit le nombre de colonnes (2-8)
3. **Page 3** : Jeu en cours avec interface complète

---

## 🏗️ Architecture Générale

Le projet est divisé en **2 applications indépendantes** :

```
Echec____Pong/
├── ServerApp/          # Application serveur (autoritaire)
├── ClientApp/          # Application client (affichage + entrées)
└── database/           # Scripts SQL
```

### Principe de Fonctionnement

```
┌─────────────┐         TCP/IP          ┌─────────────┐
│  ClientApp  │ ◄──────────────────► │  ServerApp  │
│  (Joueur 1) │    Messages JSON      │  (Autorité) │
└─────────────┘                        └─────────────┘
                                              │
                                              │ Entity Framework
                                              ▼
                                        ┌─────────────┐
                                        │ PostgreSQL  │
                                        │  Database   │
                                        └─────────────┘
```

**Le serveur est l'autorité :**
- Détient la logique de jeu
- Valide tous les mouvements
- Gère la physique de la balle
- Synchronise l'état avec tous les clients
- Sauvegarde dans la base de données

**Les clients sont des terminaux :**
- Envoient les entrées clavier
- Reçoivent l'état du jeu
- Affichent l'interface utilisateur

---

## 📁 Structure des Dossiers

### ServerApp/

#### `Program.cs`
**Rôle :** Point d'entrée de l'application serveur

**Fonctions :**
- Initialise la connexion PostgreSQL
- Vérifie/crée les tables
- Démarre le serveur TCP
- Boucle principale d'attente

**Flux :**
```
1. Charger appsettings.json
2. Créer DatabaseContext
3. Tester connexion DB
4. Créer GameServer(port)
5. Démarrer écoute TCP
6. Attendre 'Q' pour quitter
```

---

#### `Server/GameServer.cs`
**Rôle :** Serveur TCP principal

**Responsabilités :**
- Accepter les connexions clients (max 2)
- Créer un `ClientHandler` pour chaque client
- Gérer la liste des clients connectés
- Broadcast des messages à tous les clients
- Créer un match quand 2 joueurs sont prêts

**État :**
- ⚠️ **À implémenter** : Actuellement contient seulement des TODOs

---

#### `Server/ClientHandler.cs`
**Rôle :** Gère la communication avec un client spécifique

**Responsabilités :**
- Lire les messages du client (asynchrone)
- Parser les messages JSON
- Transmettre les messages au `GameEngine`
- Envoyer les réponses au client
- Gérer la déconnexion

**État :**
- ⚠️ **À implémenter** : Actuellement contient seulement des TODOs

---

#### `Database/DatabaseContext.cs`
**Rôle :** Contexte Entity Framework Core pour PostgreSQL

**Fonctions :**
- Définit les `DbSet` pour chaque table
- Configure les relations entre entités
- Définit les contraintes et index
- Gère les migrations (si utilisées)

**Tables mappées :**
- `Matches` → `match_hybride`
- `Players` → `joueurs`
- `Pieces` → `piece_echecs`
- `Balls` → `balle`
- `Shots` → `coup_pingpong`
- `Collisions` → `collision_precise`
- `DefenseStats` → `statistiques_defense`

---

#### `Database/GameRepository.cs`
**Rôle :** Pattern Repository pour l'accès aux données

**Méthodes principales :**
- `GetPlayerByIdAsync()` : Récupérer un joueur
- `CreatePlayerAsync()` : Créer un joueur
- `CreateMatchAsync()` : Créer un match
- `InitializePiecesForMatchAsync()` : Initialiser les 32 pièces d'un match
- `UpdateMatchAsync()` : Mettre à jour un match

**Logique importante :**
- `CreatePiecesForPlayer()` : Crée les pièces avec les bonnes rangées
  - Nord : Rangée arrière = 1, Pions = 0
  - Sud : Rangée arrière = 0, Pions = 1

---

#### `GameLogic/GameEngine.cs`
**Rôle :** Moteur de jeu principal (logique métier)

**Responsabilités :**
- Gérer l'état du jeu (`GameState`)
- Traiter les mouvements des joueurs
- Gérer les frappes de balle
- Calculer les collisions balle-pièces
- Détecter les conditions de victoire
- Synchroniser avec la base de données

**Méthodes clés :**
- `InitializeMatch()` : Initialise un nouveau match
- `ProcessPlayerMove()` : Met à jour la position d'un joueur
- `ProcessBallHit()` : Lance la balle avec angle/puissance
- `UpdatePhysics()` : Met à jour la physique (appelé toutes les 50ms)
  - Utilise `PhysicsEngine` pour calculer la trajectoire
  - Détecte si la balle sort du terrain
  - Vérifie si l'adversaire peut défendre
  - Applique les dégâts aux pièces si collision

**Logique de collision :**
```
1. Balle lancée par joueur Nord → Sud
2. Balle sort par colonne X (0-7)
3. Vérifier si joueur Sud est en colonne X (défense)
4. Si NON défendu → Toucher pièce Sud colonne X
5. Appliquer dégâts (1 point)
6. Si pièce = Roi et vies = 0 → Victoire Nord
```

---

#### `GameLogic/PhysicsEngine.cs`
**Rôle :** Calculs physiques de la balle

**Méthodes :**
- `UpdateBallPosition()` : Calcule nouvelle position avec gravité
- `CalculateImpactColumn()` : Prédit la colonne de sortie
- `CalculateHitPower()` : Calcule la puissance d'un coup

**Physique :**
- Gravité : 9.81 m/s²
- Rebond sur sol : coefficient 0.8
- Limites : X ∈ [0, 1], Y ≥ 0

---

#### `Models/` (Tous les fichiers Db*.cs)
**Rôle :** Modèles de données Entity Framework

Chaque fichier représente une table PostgreSQL :

- **`DbPlayer.cs`** : Table `joueurs`
  - Pseudo, classement, statistiques
  
- **`DbMatch.cs`** : Table `match_hybride`
  - Scores, état des rois, statut, timestamps
  
- **`DbPieceEchecs.cs`** : Table `piece_echecs`
  - Type, position (colonne/rangée), vies, statut
  
- **`DbBall.cs`** : Table `balle`
  - Position, vitesse, état, prédiction colonne
  
- **`DbCoupPingPong.cs`** : Table `coup_pingpong`
  - Type de coup, puissance, précision, colonne visée/atteinte
  
- **`DbCollisionPrecise.cs`** : Table `collision_precise`
  - Détails d'une collision balle-pièce
  
- **`DbDefenseStat.cs`** : Table `statistiques_defense`
  - Statistiques de défense par colonne
  
- **`DbTerrain.cs`** : Table `terrains`
  - Configuration du terrain de jeu
  
- **`DbPlayerPreference.cs`** : Table `preferences_joueur`
  - Préférences de jeu du joueur

---

#### `Network/GameMessage.cs`
**Rôle :** Protocole de communication réseau

**Classes de messages :**
- `JoinRequestMessage` : Client demande à rejoindre
- `JoinResponseMessage` : Serveur répond (succès/échec)
- `PlayerMoveMessage` : Client envoie mouvement raquette
- `BallHitMessage` : Client frappe la balle
- `GameStateUpdateMessage` : Serveur envoie état complet
- `PieceDamagedMessage` : Notification de dégâts
- `MatchEndMessage` : Fin de match
- `ChatMessage` : Message chat
- `PingMessage` : Ping/pong pour latence
- `TargetingUpdateMessage` : Mise à jour ciblage

**Format :** JSON avec propriété `messageType`

---

#### `Network/GameState.cs`
**Rôle :** État du jeu synchronisé entre serveur et clients

**Classes :**
- `GameState` : État complet
  - `Players` : Liste des joueurs
  - `Ball` : État de la balle
  - `PiecesNorth` / `PiecesSouth` : Pièces de chaque côté
  - `Match` : Informations du match
  
- `PlayerState` : État d'un joueur
  - Position X, côté (north/south), score
  
- `BallState` : État de la balle
  - Position (X, Y), vitesse (VX, VY), état
  
- `PieceState` : État d'une pièce
  - Type, colonne, rangée, vies, vivant
  
- `MatchInfo` : Informations match
  - Scores, statut, vainqueur, raison victoire

---

### ClientApp/

#### `Program.cs`
**Rôle :** Point d'entrée de l'application client

**Flux :**
```
1. Créer GameClient, GameManager, GameRenderer, KeyboardHandler
2. Connecter au serveur (localhost:7777)
3. Demander nom du joueur
4. Envoyer JoinRequestMessage
5. Configurer événements clavier
6. Boucle principale (attendre déconnexion)
```

---

#### `Client/GameClient.cs`
**Rôle :** Client TCP pour communication avec serveur

**Fonctions :**
- `ConnectAsync()` : Connexion au serveur
- `SendMessageAsync()` : Envoi message JSON
- `ReceiveMessagesAsync()` : Réception asynchrone (thread séparé)
- `Disconnect()` : Fermeture connexion

**Événements :**
- `OnMessageReceived` : Message reçu du serveur
- `OnConnected` : Connexion établie
- `OnDisconnected` : Déconnexion

---

#### `Game/GameManager.cs`
**Rôle :** Gestionnaire de l'état local du jeu

**Responsabilités :**
- Maintenir l'état local (`_currentState`)
- Traiter les messages réseau
- Mettre à jour l'état quand message reçu
- Calculer la prédiction de ciblage
- Envoyer les actions du joueur au serveur

**Méthodes :**
- `UpdatePlayerPosition()` : Envoie mouvement au serveur
- `SendBallHit()` : Envoie frappe au serveur
- `SendChat()` : Envoie message chat
- `UpdateTargetingPrediction()` : Calcule colonne ciblée

**Événements :**
- `OnGameStateUpdated` : État mis à jour
- `OnChatMessage` : Message chat reçu
- `OnGameEvent` : Événement de jeu (dégâts, fin match)

---

#### `Input/KeyboardHandler.cs`
**Rôle :** Gestion des entrées clavier

**Contrôles :**
- `←` / `→` : Déplacer raquette gauche/droite
- `ESPACE` : Frapper la balle
- `A` / `Z` : Augmenter/diminuer angle (0-90°)
- `E` / `R` : Augmenter/diminuer puissance (0.5-3.0)
- `C` : Ouvrir chat
- `Q` : Quitter

**Fonctionnement :**
- Thread séparé qui écoute `Console.KeyAvailable`
- Appelle les événements correspondants
- Affiche prévisualisation angle/puissance

---

#### `Render/GameRenderer.cs`
**Rôle :** Affichage de l'interface utilisateur

**Sections affichées :**
1. **En-tête** : Scores, noms joueurs
2. **Zone Nord** : Échiquier Nord + raquette
3. **Zone de jeu** : Terrain Ping-Pong avec colonnes (0-7)
4. **Zone Sud** : Échiquier Sud + raquette
5. **Ciblage** : Colonne ciblée, défense possible
6. **Commandes** : Liste des touches

**Méthodes :**
- `Render()` : Affiche tout l'écran
- `RenderChessRow()` : Affiche une rangée d'échiquier
- `RenderGameZone()` : Affiche zone Ping-Pong
- `RenderTargetingInfo()` : Affiche info ciblage

**Symboles pièces :**
- ♔ ♕ ♖ ♗ ♘ ♙ (blancs)
- ♚ ♛ ♜ ♝ ♞ ♟ (noirs)

---

#### `Network/GameMessage.cs` et `Network/GameState.cs`
**Rôle :** Identiques au serveur pour compatibilité

Les mêmes classes que dans `ServerApp/Network/` pour assurer la compatibilité de sérialisation JSON.

---

## 🔄 Flux de Données

### Connexion d'un Client

```
Client                    Serveur                  Database
  │                         │                         │
  │─── ConnectAsync() ─────►│                         │
  │                         │                         │
  │─── JoinRequest ─────────►│                         │
  │                         │─── CreatePlayer ───────►│
  │                         │◄── Player Created ──────│
  │◄── JoinResponse ─────────│                         │
  │  (PlayerId, Side)       │                         │
```

### Pendant le Jeu

```
Client                    Serveur                  Database
  │                         │                         │
  │─── PlayerMove ──────────►│                         │
  │                         │─── ProcessMove()        │
  │                         │─── UpdatePhysics()      │
  │                         │─── CheckCollisions()    │
  │                         │─── SaveToDB ────────────►│
  │◄── GameStateUpdate ────│                         │
  │  (État complet)         │                         │
```

### Frappe de Balle

```
Client                    Serveur                  Database
  │                         │                         │
  │─── BallHit ─────────────►│                         │
  │  (power, angle)         │─── ProcessBallHit()      │
  │                         │  Set ball velocity      │
  │                         │                         │
  │◄── GameStateUpdate ────│                         │
  │  (Ball moving)          │                         │
  │                         │                         │
  │  [Boucle 50ms]          │─── UpdatePhysics()      │
  │                         │  Calculate trajectory   │
  │                         │  Check exit column      │
  │                         │  Check defense          │
  │                         │  Apply damage ──────────►│
  │◄── GameStateUpdate ────│                         │
  │◄── PieceDamaged ────────│                         │
```

---

## 🎮 Logique de Jeu Détaillée

### Initialisation d'un Match

1. **2 joueurs connectés** → Serveur crée un `DbMatch`
2. **Serveur appelle** `InitializePiecesForMatchAsync()`
3. **Pour chaque joueur** :
   - Crée 8 pièces arrière (Roi, Reine, Tours, etc.)
   - Crée 8 pions
   - Positionne selon côté (Nord/Sud)
4. **Match démarre** → Statut = "en_cours"

### Système de Colonnes

Le terrain est divisé en **8 colonnes** (0-7) :

```
Colonnes:  0   1   2   3   4   5   6   7
           │   │   │   │   │   │   │   │
Nord:      ♟   ♟   ♟   ♟   ♟   ♟   ♟   ♟
           │   │   │   │   │   │   │   │
           ────────────────────────────────
           │   │   │   │   │   │   │   │
Sud:       ♙   ♙   ♙   ♙   ♙   ♙   ♙   ♙
```

**Défense :** Si un joueur est en colonne X, il protège toutes les pièces de cette colonne.

### Calcul de Collision

1. **Balle lancée** avec angle et puissance
2. **Physique calcule** trajectoire avec gravité
3. **Quand balle sort** (X < 0 ou X > 1) :
   - Calculer colonne de sortie : `(int)(X * 8)`
4. **Vérifier défense** :
   - Position adversaire en colonne X ? → Défense réussie
5. **Si non défendu** :
   - Trouver pièce en colonne X (rangée avant d'abord)
   - Appliquer 1 dégât
   - Si vies = 0 → Pièce éliminée
   - Si Roi et vies = 0 → Victoire

### Conditions de Victoire

1. **Roi capturé** : `WinReason = "king_captured"`
2. **Score atteint** : 11 points avec écart de 2
   - `WinReason = "score_reached"`
3. **Abandon** : Joueur se déconnecte
   - `WinReason = "abandon"`

---

## 🔧 Configuration

### `appsettings.json` (Serveur)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=echec_pong;..."
  },
  "Server": {
    "Port": 7777,
    "MaxPlayers": 2,
    "GameUpdateInterval": 50  // ms
  }
}
```

### Ports et Adresses

- **Serveur TCP** : Port 7777 (configurable)
- **PostgreSQL** : Port 5432 (par défaut)
- **Client** : Se connecte à `localhost:7777` (modifiable dans `Program.cs`)

---

## 📊 Base de Données

### Tables Principales

- **`joueurs`** : Informations joueurs
- **`match_hybride`** : Matchs en cours/terminés
- **`piece_echecs`** : Toutes les pièces de tous les matchs
- **`balle`** : État de la balle par match
- **`coup_pingpong`** : Historique des coups
- **`collision_precise`** : Historique des collisions

### Vues Utiles

- **`vue_matchs_paralleles`** : Matchs en cours avec stats
- **`vue_statistiques_precision`** : Précision des joueurs
- **`vue_defenses_colonnes`** : Statistiques de défense
- **`vue_pieces_vulnerables`** : Pièces non protégées

---

## 🚀 Prochaines Étapes

Voir `avancement.md` pour la liste complète des fonctionnalités à implémenter.

**Priorité immédiate :**
1. Implémenter `GameServer.cs` et `ClientHandler.cs`
2. Finaliser `Program.cs` client
3. Tester connexion serveur-client
4. Tester match complet localement

---

**Dernière mise à jour :** Décembre 2024

