# 🌀 Maze Game — Setup Guide

## Setup (2 minutes)

1. Open the project in Unity.
2. In the **Hierarchy**, right-click → **Create Empty** → name it `Maze`.
3. Select it and in the **Inspector** click **Add Component** → type `Maze Generator`.
4. Press **Play**. That's it!

On Play, everything is built automatically:
- 🧱 A random maze (different every run!)
- 🔥 Flickering torches mounted on the walls
- 🌋 Lava tiles scattered through the corridors (they set you on fire)
- 👹 Patrolling enemies that chase you on sight (red light = they spotted you)
- 💚 A spinning green exit portal in the far corner — touch it to **win**
- 🩸 A health bar with key hints, camera and post-processing effects
- 🎮 A player is created automatically if the scene has none

## How to Play

- **WASD / arrow keys** — move
- **F** — set yourself on fire: you run 1.5× faster but lose health (risk/reward!)
- **Type `blow`** — put the fire out (keyboard modality)
- **Blow into the microphone** — put the fire out (voice modality)
- **R** — restart after winning or dying
- Avoid lava tiles and patrolling enemies; escaping an enemy's line of sight makes it give up.

## Inspector Settings (Maze Generator)

| Setting | What it does | Default |
|---|---|---|
| `Size` | Maze size (odd number; 15 → 7x7 corridors) | 15 |
| `Cell Size` | Corridor width | 4 |
| `Wall Height` | Height of the walls | 3.5 |
| `Camera Mode` | TopDown (fixed bird's-eye) or FollowBehind | TopDown |
| `Player` | The player (auto-detected if empty) | auto |
| `Spawn Enemies` | Whether enemies appear | on |
| `Enemy Count` | Number of patrolling enemies (spread across regions) | 3 |
| `Lava Count` | Number of lava tiles | 5 |
| `Torch Count` | Number of torches | 14 |
| `Hide Old Floor` | Deactivates the old "Plane" floor | on |

Fog color/density can be tweaked on the `Scene Beautifier` component; fire controls (keys, word, speed multiplier) on the player's `Fire Controls` component.

## Files

- `Assets/MazeGenerator.cs` — main script that builds everything (also contains `TorchFlicker` and `Spinner`)
- `Assets/SceneBeautifier.cs` — fog, lighting and post-processing
- `Assets/CameraFollow.cs` — smooth follow camera (FollowBehind mode)
- `Assets/MazeExit.cs` — exit portal + win screen
- `Assets/GameOverScreen.cs` — death screen
- `Assets/EnemyPatrol.cs` — patrol + line-of-sight chase AI
- `Assets/enemyTouchDamage.cs` — contact damage
- `Assets/PlayerFireEffect.cs` — flame particles while burning
- `Assets/lavaIgnite.cs` — lava sets the player on fire
- `Assets/FireControls.cs` — F to ignite, type "blow" to extinguish, speed boost while burning
- `Assets/HealthBarBuilder.cs` — health bar UI built from code

The kid's original scripts (`PlayerMovement`, `playerHealth`, `microphoneController`, `floorLava`, `healthbarui`) are still the core of the gameplay; `enemy.cs` (EnemyChase) was replaced by `EnemyPatrol` and is no longer used.
