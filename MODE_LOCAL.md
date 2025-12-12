# 🎮 MODE LOCAL - Solution de Contournement

## 🚨 Problème Résolu

L'application restait bloquée sur la page de saisie du nom car le serveur n'implémentait pas la logique de traitement des messages réseau (`JoinRequest`, etc.).

## ✅ Solution : Mode Local (Démo)

Un **mode de jeu local** a été ajouté qui permet de jouer immédiatement **sans serveur**.

### Caractéristiques du Mode Local

- ✅ Pas besoin de serveur
- ✅ Jeu fonctionnel avec physique de la balle
- ✅ IA simple pour le joueur adverse (Sud)
- ✅ Toutes les pièces d'échecs affichées
- ✅ Système de score
- ✅ Détection de fin de partie

---

## 🚀 Lancement Rapide

### Option 1 : Script PowerShell (Recommandé)
```powershell
.\start-demo-game.ps1
```

### Option 2 : Manuellement
```powershell
cd ClientApp
dotnet run
```

Puis cliquez sur **"MODE LOCAL (DÉMO)"** dans la fenêtre de connexion.

---

## 🎯 Contrôles du Jeu

- **← →** : Déplacer la raquette
- **Q** : Quitter

### Règles

1. Protégez vos pièces avec votre raquette
2. Si vous ratez la balle, elle touche une pièce adverse (ils gagnent un point)
3. Si l'adversaire rate, il touche une de vos pièces (vous gagnez un point)
4. Les pièces ont des points de vie (♥)
5. Le roi a 3 vies - s'il meurt, c'est la fin !

---

## 📁 Fichiers Créés/Modifiés

### Nouveaux fichiers
- `ClientApp/Game/LocalGameMode.cs` : Moteur de jeu local
- `start-demo-game.ps1` : Script de lancement rapide

### Fichiers modifiés
- `ClientApp/Forms/ConnectionForm.cs` : Bouton "MODE LOCAL" ajouté
- `ClientApp/Forms/MainForm.cs` : Support du mode local + réseau
  - Taille de fenêtre réduite (800x600)
  - Deux constructeurs (réseau/local)
  - Rendu optimisé pour petits écrans

---

## 🔧 Mode Réseau (Pour Plus Tard)

Le mode réseau existe toujours mais nécessite que le **serveur soit complété** avec :

1. Logique de traitement des messages
2. Gestion de l'état du jeu côté serveur
3. Synchronisation des joueurs
4. Moteur physique côté serveur

Pour l'instant, utilisez le **MODE LOCAL** pour tester l'interface et le gameplay !

---

## 📸 Aperçu

```
┌─────────────────────────────────────────┐
│  Échec-Pong - Connexion                 │
├─────────────────────────────────────────┤
│                                         │
│  ○ Découverte automatique (UDP)         │
│  ○ Serveur local (127.0.0.1)            │
│  ○ Adresse IP personnalisée: _______    │
│                                         │
│     [ SE CONNECTER ]                    │
│                                         │
│     [ MODE LOCAL (DÉMO) ] ← Cliquez!    │
│                                         │
└─────────────────────────────────────────┘
```

---

## 🐛 Dépannage

### Le jeu ne se lance pas
```powershell
# Vérifier .NET
dotnet --version

# Nettoyer et recompiler
cd ClientApp
dotnet clean
dotnet build
dotnet run
```

### Les pièces ne s'affichent pas bien
- La police "Segoe UI" doit supporter les symboles Unicode
- Windows 10/11 : normalement OK
- Vérifiez que votre système affiche : ♔ ♕ ♖ ♗ ♘ ♙

### La fenêtre est trop grande
- Déjà réduite à 800x600
- Pour réduire encore, modifiez [MainForm.cs](ClientApp/Forms/MainForm.cs#L73) :
  ```csharp
  Size = new Size(700, 500); // Au lieu de 800x600
  ```

---

**Bon jeu ! 🎮**
