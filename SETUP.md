# Guide de Démarrage Rapide

## Prérequis
- .NET 9.0 SDK (déjà installé : version 9.0.306)
- PostgreSQL (déjà installé)
- VS Code ou Cursor

## Installation

### 1. Créer la base de données PostgreSQL

```sql
-- Se connecter à PostgreSQL
psql -U postgres

-- Créer la base de données
CREATE DATABASE echec_pong;

-- Se connecter à la base
\c echec_pong

-- Exécuter le script de schéma
\i database/schema.sql
```

Ou exécuter directement :
```bash
psql -U postgres -d echec_pong -f database/schema.sql
```

### 2. Restaurer les packages NuGet

```bash
# Dans le dossier ServerApp
cd ServerApp
dotnet restore

# Dans le dossier ClientApp
cd ../ClientApp
dotnet restore

# Dans le dossier Shared
cd ../Shared
dotnet restore
```

### 3. Configurer la chaîne de connexion

Modifier `ServerApp/Program.cs` :
```csharp
string dbConnectionString = "Host=localhost;Database=echec_pong;Username=postgres;Password=VOTRE_MOT_DE_PASSE";
```

## Exécution

### Démarrer le serveur
```bash
cd ServerApp
dotnet run
```

### Démarrer un client (dans un autre terminal)
```bash
cd ClientApp
dotnet run
```

### Le serveur peut aussi devenir client
Le serveur peut lancer une instance client en interne pour tester en local avec 2 fenêtres.

## Structure des Projets

- **ServerApp** : Application serveur (TCP + PostgreSQL)
- **ClientApp** : Application client (TCP + Interface)
- **Shared** : Code partagé (protocole de communication)

## Prochaines Étapes

1. Implémenter les méthodes TODO dans les classes
2. Tester la connexion réseau TCP
3. Tester la connexion PostgreSQL
4. Implémenter la logique de jeu (Échecs + Ping-Pong)
5. Implémenter l'affichage

