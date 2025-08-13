# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Auto-Battle-Tactics is a Unity 6000.1.9f1 multiplayer strategy game built in C# with a Node.js WebSocket server backend. The game features team-based combat with units battling autonomously while players strategically deploy and manage their forces.

## Development Commands

### Unity Build & Development
- **Open in Unity**: Open Unity Hub and add the project folder, then open with Unity 6000.1.9f1
- **Build**: Use Unity's Build Settings (File → Build Settings) to build for target platform
- **Test Mode**: Use Unity's Play Mode for testing gameplay mechanics

### Server Development
- **Start Server**: `cd Server && node index.js` (runs on port 3000)
- **Install Dependencies**: `cd Server && npm install`
- **Server Dependencies**: Express.js for HTTP, WebSocket (ws) for real-time communication

## Architecture Overview

### Core Game Systems

**Entity-Component Architecture**: The game uses a component-based entity system where each unit (Entity) contains multiple specialized components:
- `HealthComponent`: Manages HP, damage, death
- `AttackComponent`: Handles combat logic and targeting
- `MoveComponent`: Controls movement and NavMesh navigation
- `AnimationComponent`: Manages animations and state transitions
- `TeamComponent`: Handles team assignment and allegiances
- `EffectComponent`: Manages visual/audio effects

**Data-Driven Design**: Uses ScriptableObjects for configuration:
- `EntityData`: Unit stats, animations, effects, and behavior parameters
- `ProjectileData`: Projectile behavior and visual settings
- `ObjectData`: Core structure health and properties

### Key Managers

**BattleManager** (`Assets/Scripts/GameScene/BattleManager.cs`): Central registry for all battle units, handles team-based unit queries and battle state management.

**CombatManager** (`Assets/Scripts/Game/CombatManager.cs`): Manages combat events and game-end conditions through static events.

**GameManager** (`Assets/Scripts/Main/GameManager.cs`): Persistent singleton managing user profiles, match history, authentication tokens, and cross-scene data.

### Networking Architecture

**Client-Server Model**: 
- Unity client uses `WebSocketClient.cs` with NativeWebSocket package
- Node.js server handles matchmaking, team assignment, and real-time synchronization
- Maximum 2 players per game session
- Server assigns random teams (Red/Blue) and manages game state transitions

**Message Types**:
- `init`: Unit initialization and placement
- `ready`: Player ready state for game start
- `teamAssign`: Server-assigned team designation
- `gameStart`: Begin battle phase

### Scene Structure

1. **0-LoginScene**: User authentication and profile management
2. **1-MainScene**: Main menu, matchmaking, and lobby
3. **2-GameScene**: Primary battle arena with NavMesh navigation
4. **3-GameScene2**: Alternative battle map
5. **4-BattleScene**: Combat-focused scene

### Component Dependencies

Units require these Unity components (enforced via `RequireComponent`):
- `NavMeshAgent`: For pathfinding and movement
- `CapsuleCollider`: For collision detection
- `Rigidbody`: For physics interactions
- All custom components listed above

### Asset Organization

- `Assets/Scripts/Game/`: Core gameplay systems and components
- `Assets/Scripts/GameScene/`: Scene-specific managers and controllers
- `Assets/Scripts/Main/`: Cross-scene systems, networking, and user management
- `Assets/Scripts/Game/Prefab/`: Unit and projectile prefabs organized by type
- `Assets/Scripts/Game/Scriptable Object/`: Data configuration assets

### External Dependencies

**Unity Packages**:
- Unity.AI.Navigation (2.0.8): NavMesh and pathfinding
- Unity.InputSystem (1.14.0): Modern input handling
- Unity.RenderPipelines.Universal (17.1.0): URP rendering
- NativeWebSocket: Real-time client-server communication

**Server Dependencies**:
- Express.js (5.1.0): HTTP server framework
- ws (8.18.3): WebSocket implementation

## Development Notes

- The project uses Korean comments in some files - this is normal and expected
- Unit prefabs must include all required components or the game will log errors
- NavMesh areas are configured for different unit sizes (Small, Medium, Large)
- The game supports multiple attack types: Melee, Ranged, and Magic with different mechanics
- Object pooling is implemented for projectiles and effects to optimize performance
- Team assignment is handled server-side to prevent cheating