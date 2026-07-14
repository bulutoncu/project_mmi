# 🌀 Maze Game — Setup Guide

## Setup (2 minutes)

1. Open the project in Unity.
2. In the **Hierarchy**, right-click → **Create Empty** → name it `Maze`.
3. Select it and in the **Inspector** click **Add Component** → type `Maze Generator`.
4. Press **Play**. That's it!

On Play, everything is built automatically:
- 🧱 A random maze (different every run!)
- 👹 Patrolling enemies that chase you on sight (red light = they spotted you)
- 🩸 A health bar — enemies damage you on contact, at 0 HP you die
- 💚 A spinning green exit portal in the far corner — touch it to **win**
- 🎮 A player is created automatically if the scene has none
- 📷 Camera and post-processing effects

## How to Play

- **WASD / arrow keys** — move
- **R** — restart after winning or dying (a brand new maze is generated)
- Enemies hurt you while touching you (20 HP per hit) — shake them off before your health runs out!
- Escaping an enemy's line of sight makes it give up.

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
| `Hide Old Floor` | Deactivates the old "Plane" floor | on |

Fog color/density can be tweaked on the `Scene Beautifier` component.

## Files

- `Assets/MazeGenerator.cs` — main script that builds everything (also contains `Spinner`)
- `Assets/SceneBeautifier.cs` — fog, lighting and post-processing
- `Assets/CameraFollow.cs` — smooth follow camera (FollowBehind mode)
- `Assets/MazeExit.cs` — exit portal + win screen
- `Assets/EnemyPatrol.cs` — patrol + line-of-sight chase AI
- `Assets/enemyTouchDamage.cs` — contact damage
- `Assets/playerHealth.cs` — health (damage/heal)
- `Assets/GameOverScreen.cs` — death screen
- `Assets/HealthBarBuilder.cs` — health bar UI built from code (`healthbarui.cs` keeps it updated)
- `Assets/PlayerMovement.cs` — WASD movement
