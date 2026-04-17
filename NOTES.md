# Notes de Configuration

## Structure du Projet C# - MonoGame

### Organisation des Fichiers

```
AnimationGame/
├── src/
│   ├── Animation.cs       - Gère les animations avec frames PNG
│   ├── Player.cs          - Joueur + physique + input
│   ├── GameWindow.cs      - Fenêtre de jeu + boucle principale
│   └── Program.cs         - Point d'entrée
├── Content/
│   └── Sprites/
│       ├── Idle/          - Images PNG pour idle
│       ├── Run/           - Images PNG pour run
│       ├── Jump/          - Images PNG pour jump
│       └── Death/         - Images PNG pour death
└── AnimationGame.csproj   - Configuration du projet
```

### Classes Principales

#### `Animation.cs`
- **Classe**: `Animation`
- Gère les frames et l'avancement
- Supporte les animations en boucle ou non
- `Update()` - Met à jour la frame actuelle
- `GetCurrentFrame()` - Obtient la texture actuelle
- `Reset()` - Redémarre l'animation

#### `Player.cs`
- **Classe**: `Player`
- Gestion complète du joueur
- Physique 2D (gravité, velocité)
- Gestion des états (idle, run, jump, death)
- Chargement automatique des PNG depuis les dossiers
- `Update()` - Met à jour physique + animations
- `Draw()` - Affiche le joueur
- `Die()` - Lance l'animation de mort

#### `GameWindow.cs`
- **Classe**: `GameWindow : Game`
- Hérite de la classe Game de MonoGame
- Boucle principale du jeu
- Rendu (Draw), Logique (Update)
- Gestion des collisions basiques

#### `Program.cs`
- Point d'entrée du programme
- Lance la fenêtre de jeu

### Configuration MonoGame

Le `.csproj` configure:
- Framework: .NET 6.0 Windows
- Plateforme: WinDX (DirectX pour Windows)
- Les fichiers du dossier `Content` sont copiés à la compilation

### Chargement des Sprites

Les images PNG sont chargées comme suit:

1. **Demarrage**: `Player.LoadAnimations(graphicsDevice)` est appelé
2. **Parcours**: Pour chaque dossier (Idle, Run, Jump, Death)
3. **Tri**: Les fichiers `.png` sont triés alphabétiquement
4. **Chargement**: Chaque PNG est chargé avec `Texture2D.FromStream()`
5. **Animation**: Les textures sont groupées dans une `List<Texture2D>`

### Ordre de Chargement

```
Content/Sprites/Idle/  → lecture alphabétique → Animation Idle
Content/Sprites/Run/   → lecture alphabétique → Animation Run
Content/Sprites/Jump/  → lecture alphabétique → Animation Jump
Content/Sprites/Death/ → lecture alphabétique → Animation Death
```

### Boucle de Jeu

```
Game.Run()
    └─ Initialize()      - Première initialisation
    └─ LoadContent()     - Charge les ressources
    └─ BOUCLE PRINCIPALE
        ├─ Update()      - Logique du jeu
        │   └─ Player.Update()
        └─ Draw()        - Rendu
            └─ Player.Draw()
```

### Entrées Clavier

```
Keyboard.GetState()  → Obtient l'état actuel
keys.IsKeyDown()     → Teste si une touche est enfoncée
```

### Physique Simple

- **Gravité**: Accelere vers le bas
- **Jump**: Applique une velocité négative (vers le haut)
- **Limite**: MaxFallSpeed empêche une chute trop rapide
- **Collision**: Vérification simple avec le sol

---

**Notes de développement:**
- MonoGame gère la fenêtre et le contexte graphique
- DirectX est utilisé en arrière-plan sur Windows
- Les sprites sont retournés avec `SpriteEffects.FlipHorizontally`
- L'ordre alphabétique des fichiers détermine l'ordre des frames
