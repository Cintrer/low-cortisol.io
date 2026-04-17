# 🎮 Animation Personnage - C# Simple

Projet C# simple avec MonoGame pour créer un personnage animé avec sprites PNG.

## 📁 Structure

```
📦 low-cortisol.io
├── 📁 src/
│   ├── Animation.cs          # Gestionnaire d'animations
│   ├── Player.cs             # Classe du joueur
│   ├── GameWindow.cs         # Fenêtre principale du jeu
│   └── Program.cs            # Point d'entrée
├── 📁 Content/Sprites/
│   ├── Idle/                 # Au repos
│   ├── Run/                  # Course
│   ├── Jump/                 # Saut
│   └── Death/                # Mort
├── AnimationGame.csproj      # Projet C#
└── README.md                 # Ce fichier
```

## 🎯 Fonctionnalités

✅ **Animations basées sur PNG** - Charge automatiquement les images  
✅ **Mouvement gauche/droite** - Avec retournement du sprite  
✅ **Saut avec gravité** - Physique 2D simple  
✅ **États du joueur** - idle, run, jump, death  
✅ **Gestion des entrées** - Clavier flèches/ZQSD

## ⌨️ Contrôles

| Touche | Action |
|--------|--------|
| **Flèches Gauche/Droite** ou **Q/D** | Mouvement |
| **Espace** / **Haut** / **Z** | Saut |
| **M** | Tester l'animation de mort |
| **R** | Réinitialiser le joueur |
| **Tab** | Afficher le débogage |
| **Échap** | Quitter |

## 📸 Ajouter tes Sprites

### 1️⃣ Images pour **Idle** (au repos)
- Dossier: `Content/Sprites/Idle/`
- Ajoute: `image1.png`, `image2.png`, etc.
- Nombre: 1 à N images
- Format: PNG avec transparence
- Taille: 80x100 pixels (configurable)

### 2️⃣ Images pour **Run** (course)
- Dossier: `Content/Sprites/Run/`
- Ajoute: `Wraith_03_Moving Forward_000.png`, etc.
- Format: PNG avec transparence
- Les images seront triées alphabétiquement

### 3️⃣ Images pour **Jump** (saut)
- Dossier: `Content/Sprites/Jump/`
- Progression du saut

### 4️⃣ Images pour **Death** (mort)
- Dossier: `Content/Sprites/Death/`
- Animation d'environ 15 frames recommandé

## 🚀 Comment Lancer

### Avec .NET CLI
```bash
cd d:\Ymmerssion\low-cortisol.io
dotnet build
dotnet run
```

### Avec Visual Studio
1. Ouvre `AnimationGame.csproj` dans Visual Studio
2. Appuie sur **F5** pour lancer

## ⚙️ Configuration

Modifie les valeurs dans `src/Player.cs`:

```csharp
public float Gravity = 980f;           // Force de gravité
public float MaxFallSpeed = 500f;      // Vitesse de chute max
public float MoveSpeed = 250f;         // Vitesse de mouvement
public float JumpForce = -500f;        // Force du saut
```

## 🎬 Durée des Animations

Dans `src/Player.cs`, méthode `LoadAnimations()`:

```csharp
_idleAnim = LoadAnimationFromFolder(..., 0.08f, true);   // 0.08s par frame
_runAnim = LoadAnimationFromFolder(..., 0.06f, true);    // 0.06s par frame
_jumpAnim = LoadAnimationFromFolder(..., 0.1f, false);   // 0.1s par frame
_deathAnim = LoadAnimationFromFolder(..., 0.08f, false); // 0.08s par frame
```

## 📝 Fichiers Importants

- [src/Animation.cs](src/Animation.cs) - Système d'animation
- [src/Player.cs](src/Player.cs) - Joueur, physique et input
- [src/GameWindow.cs](src/GameWindow.cs) - Boucle de jeu principale

## 🔧 Dépendances

- **.NET 6.0+**
- **MonoGame 3.8.1**

Les dépendances sont automatiquement téléchargées via NuGet.

## 💡 Conseils

- Les images PNG doivent avoir leur canal alpha (transparence)
- Les fichiers sont chargés dans l'ordre alphabétique
- Utilise les touches [Tab] pour voir les infos de débogage
- La console affiche les logs de chargement des animations

---

**Bon développement!** 🎮✨
