# 🎮 Guide de Démarrage - Échec-Pong

## ✅ Problèmes Résolus

### 1. Erreur Entity Framework (ServingPlayer)
**Problème :** `Unable to determine the relationship represented by navigation 'DbMatch.ServingPlayer'`

**Solution :** Ajout de la configuration explicite des relations dans `DatabaseContext.OnModelCreating()` :
```csharp
entity.HasOne(e => e.ServingPlayer)
      .WithMany()
      .HasForeignKey(e => e.ServingPlayerId)
      .OnDelete(DeleteBehavior.Restrict);
      
entity.HasOne(e => e.Winner)
      .WithMany()
      .HasForeignKey(e => e.WinnerId)
      .OnDelete(DeleteBehavior.Restrict);
```

### 2. Connexion Client-Serveur
**Problème :** Client ne pouvait pas se connecter au serveur

**Solution :** Le serveur fonctionne maintenant correctement avec le port 7777 et accepte les connexions TCP.

## 🆕 Nouvelles Fonctionnalités

### 🔍 Découverte Réseau Automatique (UDP Broadcast)

Le système peut maintenant découvrir automatiquement les serveurs sur le réseau local !

#### Côté Serveur
- Service UDP sur port 7778 (port de jeu + 1)
- Répond aux broadcasts "ECHEC_PONG_DISCOVERY"
- Envoie les informations : port + nom de la machine

#### Côté Client
- Recherche automatique des serveurs sur le réseau local
- Menu interactif pour choisir le mode de connexion :
  1. Recherche automatique (réseau local)
  2. Connexion manuelle (localhost)
  3. Connexion manuelle (IP personnalisée)

## 🚀 Démarrage

### Étape 1 : Démarrer le Serveur

```powershell
cd ServerApp
dotnet run
```

**Sortie attendue :**
```
✅ Connexion à PostgreSQL réussie!
✅ Tables de la base de données existantes
📊 Joueurs dans la base: X
🔍 Service de découverte UDP démarré sur le port 7778
[Server] Listening on port 7777...
🎮 Serveur de jeu démarré sur le port 7777
📡 Nom du serveur: VOTRE-PC
🌐 Les clients peuvent se connecter via découverte réseau ou directement
Appuyez sur 'Q' pour quitter...
```

### Étape 2 : Lancer les Clients

#### Option A : Recherche Automatique (Recommandé)

```powershell
cd ClientApp
dotnet run
```

Puis choisir l'option **1** pour la recherche automatique.

**Sortie attendue :**
```
╔════════════════════════════════════════╗
║       ÉCHEC-PONG - CLIENT              ║
╚════════════════════════════════════════╝

Comment voulez-vous vous connecter ?
1. Recherche automatique (réseau local)
2. Connexion manuelle (localhost)
3. Connexion manuelle (IP personnalisée)

Votre choix (1-3): 1
🔍 Recherche de serveurs sur le réseau local...
✅ Serveur trouvé: VOTRE-PC (192.168.1.10:7777)
✅ Connexion au serveur: VOTRE-PC (192.168.1.10:7777)

🔗 Connexion au serveur 192.168.1.10:7777...
✅ Connecté au serveur !
Entrez votre nom: Alice
```

#### Option B : Connexion Manuelle (Localhost)

Choisir l'option **2** pour se connecter en local.

#### Option C : Connexion Manuelle (IP Personnalisée)

Choisir l'option **3** pour saisir une adresse IP spécifique.

### Étape 3 : Jouer à Plusieurs

Pour jouer à 2 joueurs :

1. **Serveur sur PC 1** (qui héberge le jeu)
2. **Client 1 sur PC 1** (joueur hébergeur)
3. **Client 2 sur PC 2** (joueur invité via WiFi)

Les deux PC doivent être sur le **même réseau WiFi**.

## 🌐 Connexion Multi-PC (WiFi)

### Configuration Réseau

#### Sur le PC Serveur :
1. Vérifier votre adresse IP :
   ```powershell
   ipconfig
   ```
   Chercher l'adresse IPv4 (ex: 192.168.1.10)

2. Autoriser le port dans le pare-feu Windows :
   ```powershell
   # En tant qu'administrateur
   New-NetFirewallRule -DisplayName "Echec-Pong TCP" -Direction Inbound -Protocol TCP -LocalPort 7777 -Action Allow
   New-NetFirewallRule -DisplayName "Echec-Pong UDP" -Direction Inbound -Protocol UDP -LocalPort 7778 -Action Allow
   ```

3. Démarrer le serveur :
   ```powershell
   dotnet run
   ```

#### Sur le PC Client :
1. Lancer le client :
   ```powershell
   dotnet run
   ```

2. Choisir l'option **1** (recherche automatique)
   - Le serveur sera détecté automatiquement sur le réseau local

   **OU**

   Choisir l'option **3** et entrer manuellement l'IP du serveur
   - Exemple : 192.168.1.10 (l'IP du PC serveur)

## 🎯 Affichage Multi-Joueurs

### Fenêtres Séparées

Chaque joueur a sa propre fenêtre console avec :
- Vue de son propre échiquier
- Position de sa raquette
- État du jeu en temps réel
- Indicateur de ciblage
- Commandes disponibles

### Synchronisation

Le serveur envoie l'état complet du jeu à tous les clients connectés :
- Position de la balle
- Positions des raquettes
- État des pièces d'échecs
- Scores
- Tours de jeu

## 📋 Configuration du Pare-feu (Windows)

Si la découverte automatique ne fonctionne pas, ouvrez les ports manuellement :

### Via l'Interface Graphique :
1. Ouvrir **Pare-feu Windows Defender**
2. Cliquer sur **Paramètres avancés**
3. **Règles de trafic entrant** → **Nouvelle règle**
4. Type : **Port**
5. Protocole : **TCP**, Port : **7777**
6. Action : **Autoriser**
7. Nom : **Echec-Pong TCP**
8. Répéter pour UDP port **7778**

### Via PowerShell (Administrateur) :
```powershell
New-NetFirewallRule -DisplayName "Echec-Pong TCP" -Direction Inbound -Protocol TCP -LocalPort 7777 -Action Allow
New-NetFirewallRule -DisplayName "Echec-Pong UDP" -Direction Inbound -Protocol UDP -LocalPort 7778 -Action Allow
```

## 🔧 Dépannage

### Problème : "Aucun serveur trouvé"
**Solutions :**
1. Vérifier que le serveur est démarré
2. Vérifier que les deux PC sont sur le même réseau WiFi
3. Désactiver temporairement le pare-feu pour tester
4. Utiliser la connexion manuelle (option 3) avec l'IP du serveur

### Problème : "Connexion refusée"
**Solutions :**
1. Vérifier que le port 7777 est ouvert dans le pare-feu
2. Vérifier que le serveur écoute sur `IPAddress.Any` (0.0.0.0)
3. Tester avec `telnet IP_SERVEUR 7777`

### Problème : "Base de données inaccessible"
**Solutions :**
1. Vérifier que PostgreSQL est démarré :
   ```powershell
   Get-Service postgresql*
   ```
2. Vérifier la chaîne de connexion dans `appsettings.json`
3. Créer la base de données si nécessaire :
   ```sql
   CREATE DATABASE pingpong_chess;
   ```

## 📊 Architecture Réseau

```
┌─────────────────┐
│   PC Serveur    │
│                 │
│  ┌──────────┐   │         WiFi/LAN
│  │PostgreSQL│   │         ┌──────────────────┐
│  └────┬─────┘   │         │                  │
│       │         │◄────────┤  PC Client 1     │
│  ┌────▼──────┐  │         │  (Découverte     │
│  │ ServerApp │  │         │   automatique)   │
│  │ Port 7777 │  │         └──────────────────┘
│  │ Port 7778 │  │
│  └───────────┘  │         ┌──────────────────┐
│       ▲         │◄────────┤  PC Client 2     │
└───────┼─────────┘         │  (Découverte     │
        │                   │   automatique)   │
        │                   └──────────────────┘
        │
   ┌────▼─────┐
   │ ClientApp│  (Local sur serveur)
   └──────────┘
```

## 🎮 Commandes de Jeu

- **←/→** : Déplacer la raquette
- **ESPACE** : Frapper la balle
- **A/Z** : Ajuster l'angle de tir
- **E/R** : Ajuster la puissance
- **C** : Ouvrir le chat
- **Q** : Quitter

## ✨ Résumé des Modifications

### Fichiers Ajoutés :
- `ServerApp/Network/ServerDiscovery.cs` - Service UDP pour découverte
- `ClientApp/Network/ServerDiscovery.cs` - Client de découverte réseau

### Fichiers Modifiés :
- `ServerApp/Database/DatabaseContext.cs` - Configuration relations EF
- `ServerApp/Server/GameServer.cs` - Support DbContext
- `ServerApp/Program.cs` - Intégration service découverte
- `ClientApp/Program.cs` - Menu de connexion interactif

### Corrections :
- ✅ Relations Entity Framework (ServingPlayer, Winner)
- ✅ Connexion TCP fonctionnelle
- ✅ Découverte réseau UDP
- ✅ Support multi-PC via WiFi
- ✅ Interface utilisateur améliorée

---

**Dernière mise à jour :** 10 décembre 2025
