# 🚀 Guide de Déploiement - Échec-Pong Hybride

Ce guide explique comment compiler et exécuter les projets **ServerApp** et **ClientApp**.

---

## 📋 Prérequis

### Logiciels Requis
- **.NET 9.0 SDK** (ou version supérieure)
  - Vérifier : `dotnet --version` (doit afficher 9.0.x ou supérieur)
- **PostgreSQL** (version 12 ou supérieure)
  - Vérifier : `psql --version`
- **Visual Studio Code** ou **Cursor** (éditeur recommandé)
- **Git** (optionnel, pour versionning)

### Configuration PostgreSQL
- PostgreSQL doit être démarré et accessible
- Un utilisateur avec droits de création de base de données
- Port par défaut : `5432`

---

## 🗄️ Étape 1 : Configuration de la Base de Données

### 1.1 Créer la base de données

```bash
# Se connecter à PostgreSQL
psql -U postgres

# Créer la base de données
CREATE DATABASE echec_pong;

# Se connecter à la nouvelle base
\c echec_pong
```

### 1.2 Exécuter le schéma SQL

```bash
# Depuis le répertoire racine du projet
psql -U postgres -d echec_pong -f database/schema.sql
```

**OU** depuis psql :
```sql
\i database/schema.sql
```

### 1.3 Vérifier la création

```sql
-- Vérifier les tables créées
\dt

-- Vérifier les vues
\dv

-- Vérifier les fonctions
\df
```

Vous devriez voir :
- **10 tables** : `joueurs`, `match_hybride`, `piece_echecs`, `balle`, etc.
- **4 vues** : `vue_matchs_paralleles`, `vue_statistiques_precision`, etc.
- **4 fonctions** : `initialiser_pieces_paralleles`, etc.

---

## 🖥️ Étape 2 : Configuration du Serveur

### 2.1 Naviguer vers le dossier serveur

```bash
cd ServerApp
```

### 2.2 Configurer la connexion à la base de données

Éditer `appsettings.json` :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=echec_pong;Username=postgres;Password=VOTRE_MOT_DE_PASSE;Port=5432"
  },
  "Server": {
    "Port": 7777,
    "MaxPlayers": 2,
    "GameUpdateInterval": 50
  }
}
```

**⚠️ Important :** Remplacer `VOTRE_MOT_DE_PASSE` par votre mot de passe PostgreSQL.

### 2.3 Restaurer les dépendances NuGet

```bash
dotnet restore
```

Cela installera :
- `Npgsql.EntityFrameworkCore.PostgreSQL` (PostgreSQL)
- `Microsoft.EntityFrameworkCore` (ORM)
- `Microsoft.Extensions.Configuration.Json` (Configuration)

### 2.4 Compiler le projet serveur

```bash
dotnet build
```

**Résultat attendu :**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 2.5 Exécuter le serveur

```bash
dotnet run
```

**Sortie attendue :**
```
✅ Connexion à PostgreSQL réussie!
✅ Tables de la base de données existantes
📊 Joueurs dans la base: X
🎮 Serveur de jeu démarré sur le port 7777
Appuyez sur 'Q' pour quitter...
```

**✅ Le serveur est maintenant en écoute sur le port 7777.**

---

## 🎮 Étape 3 : Configuration du Client

### 3.1 Ouvrir un nouveau terminal

**⚠️ Important :** Garder le serveur en cours d'exécution dans le premier terminal.

### 3.2 Naviguer vers le dossier client

```bash
cd ClientApp
```

### 3.3 Restaurer les dépendances NuGet

```bash
dotnet restore
```

Cela installera :
- `Newtonsoft.Json` (sérialisation JSON)

### 3.4 Compiler le projet client

```bash
dotnet build
```

**Résultat attendu :**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 3.5 Exécuter le client

```bash
dotnet run
```

**Sortie attendue :**
```
Connexion au serveur localhost:7777...
Connecté au serveur !
Entrez votre nom: 
```

**✅ Le client est maintenant connecté au serveur.**

---

## 🧪 Étape 4 : Test Local (2 Fenêtres)

Pour tester avec 2 clients sur la même machine :

### 4.1 Premier client
```bash
cd ClientApp
dotnet run
# Entrer le nom : Alice
```

### 4.2 Deuxième client (nouveau terminal)
```bash
cd ClientApp
dotnet run
# Entrer le nom : Bob
```

Les deux clients devraient se connecter et un match devrait démarrer automatiquement.

---

## 📦 Compilation pour Production

### Build Release Serveur

```bash
cd ServerApp
dotnet build -c Release
```

L'exécutable sera dans : `ServerApp/bin/Release/net9.0/ServerApp.exe`

### Build Release Client

```bash
cd ClientApp
dotnet build -c Release
```

L'exécutable sera dans : `ClientApp/bin/Release/net9.0/ClientApp.exe`

### Publier en fichier unique (Self-contained)

**Serveur :**
```bash
cd ServerApp
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

**Client :**
```bash
cd ClientApp
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Les fichiers seront dans : `ServerApp/bin/Release/net9.0/win-x64/publish/`

---

## 🔧 Dépannage

### Erreur : "Connexion à PostgreSQL échouée"

**Solutions :**
1. Vérifier que PostgreSQL est démarré :
   ```bash
   # Windows
   services.msc → Chercher "PostgreSQL"
   
   # Linux
   sudo systemctl status postgresql
   ```

2. Vérifier les identifiants dans `appsettings.json`

3. Tester la connexion manuellement :
   ```bash
   psql -U postgres -d echec_pong
   ```

### Erreur : "Port 7777 déjà utilisé"

**Solutions :**
1. Changer le port dans `appsettings.json` :
   ```json
   "Server": {
     "Port": 8888
   }
   ```

2. OU arrêter le processus utilisant le port :
   ```bash
   # Windows
   netstat -ano | findstr :7777
   taskkill /PID <PID> /F
   ```

### Erreur : "Tables non trouvées"

**Solution :**
Réexécuter le schéma SQL :
```bash
psql -U postgres -d echec_pong -f database/schema.sql
```

### Erreur : "Impossible de se connecter au serveur"

**Solutions :**
1. Vérifier que le serveur est démarré
2. Vérifier l'adresse IP dans `ClientApp/Program.cs` :
   ```csharp
   string serverIp = "localhost"; // ou l'IP du serveur
   int port = 7777; // doit correspondre au port serveur
   ```

3. Vérifier le firewall (si connexion distante)

---

## 📝 Commandes Rapides

### Serveur
```bash
cd ServerApp
dotnet restore && dotnet build && dotnet run
```

### Client
```bash
cd ClientApp
dotnet restore && dotnet build && dotnet run
```

### Base de données (réinitialiser)
```bash
psql -U postgres -d echec_pong -f database/drop.sql
psql -U postgres -d echec_pong -f database/schema.sql
```

---

## 🌐 Déploiement Réseau

### Serveur sur machine distante

1. **Compiler le serveur** (voir section "Compilation pour Production")

2. **Copier les fichiers** sur la machine serveur :
   - `ServerApp.exe` (ou fichiers publiés)
   - `appsettings.json`
   - `database/schema.sql` (pour création DB)

3. **Configurer `appsettings.json`** avec l'IP publique du serveur

4. **Démarrer le serveur** :
   ```bash
   ./ServerApp
   ```

### Client se connectant au serveur distant

1. **Modifier `ClientApp/Program.cs`** :
   ```csharp
   string serverIp = "192.168.1.100"; // IP du serveur
   int port = 7777;
   ```

2. **Compiler et exécuter** le client

---

## ✅ Checklist de Déploiement

- [ ] PostgreSQL installé et démarré
- [ ] Base de données `echec_pong` créée
- [ ] Schéma SQL exécuté avec succès
- [ ] `appsettings.json` configuré avec bons identifiants
- [ ] Serveur compile sans erreur
- [ ] Serveur démarre et se connecte à la DB
- [ ] Client compile sans erreur
- [ ] Client se connecte au serveur
- [ ] Test local avec 2 clients fonctionne

---

**Dernière mise à jour :** Décembre 2024

