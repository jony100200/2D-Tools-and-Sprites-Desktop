# Anomaly Directive - Utility Scripts

This folder contains reusable utility scripts that can be used across any Unity project. These utilities follow SOLID principles and are designed to be game-type agnostic.

## 📁 Available Utilities

### 1. Timer.cs - Delayed Actions System
**Purpose**: Handle timed actions without coroutines
```csharp
// Simple timer
Timer.Create(2f, () => Debug.Log("Timer complete!"));

// Timer with progress callback
Timer.Create(3f,
    () => Debug.Log("Done!"),
    progress => Debug.Log($"Progress: {progress}")
);

// Named timers for management
Timer.Create(5f, () => Debug.Log("Named timer"), "MyTimer");
Timer.StopAll("MyTimer");

// Update in MonoBehaviour
void Update() {
    Timer.UpdateAll();
}
```

### 2. UpdateUtility.cs - Continuous Actions System
**Purpose**: Run continuous actions without MonoBehaviour
```csharp
// Simple continuous action
UpdateUtility.Create(() => {
    transform.Rotate(Vector3.up * Time.deltaTime);
});

// Action that completes when condition is met
UpdateUtility.Create(() => {
    transform.Translate(Vector3.forward * Time.deltaTime);
    return transform.position.z > 10f; // Return true to stop
});

// Named updates for management
var handle = UpdateUtility.Create(() => Debug.Log("Update"), "MyUpdate");
handle.Pause();
handle.Resume();
handle.Stop();

// Update in MonoBehaviour
void Update() {
    UpdateUtility.UpdateAll();
}
```

### 3. MonoBehaviourHooks.cs - Event Delegation
**Purpose**: Attach delegates to Unity events without subclassing
```csharp
// Add to any GameObject
var hooks = gameObject.AddComponent<MonoBehaviourHooks>();

// Attach custom logic to Unity events
hooks.onUpdateAction = () => {
    // Custom update logic
};

hooks.onCollisionEnterAction = (collision) => {
    // Custom collision logic
};
```

### 4. Grid2D.cs - 2D Grid System
**Purpose**: Generic 2D grid for games (pathfinding, tilemaps, etc.)
```csharp
// Create a grid
var grid = new Grid2D<int>(10, 10, 1f, Vector2.zero, (grid, pos) => 0);

// Set/get values
grid.SetGridObject(new Vector2Int(5, 5), 42);
int value = grid.GetGridObject(new Vector2Int(5, 5));

// Convert between world and grid coordinates
Vector2Int gridPos = grid.GetGridPosition(worldPosition);
Vector2 worldPos = grid.GetWorldPosition(gridPos);

// Iterate through all cells
grid.ForEach((pos, value) => {
    Debug.Log($"Cell {pos} has value {value}");
});

// Get neighboring cells
Vector2Int[] neighbors = grid.GetNeighborPositions(new Vector2Int(5, 5), true);
```

### 5. StateMachine.cs - State Management
**Purpose**: Simple state machine for object behavior
```csharp
// Define states
public class IdleState : BaseState {
    public override void Enter() { /* enter logic */ }
    public override void Update() { /* update logic */ }
}

// Create and use state machine
var stateMachine = new StateMachine<BaseState>();
stateMachine.Initialize(new IdleState());
stateMachine.ChangeState(new WalkingState());

// Update in MonoBehaviour
void Update() {
    stateMachine.Update();
}
```

### 6. ObjectPool.cs - Performance Optimization
**Purpose**: Object pooling for frequently created/destroyed objects
```csharp
// Create pool
var bulletPool = new ObjectPool<Bullet>(bulletPrefab, 20);

// Get object from pool
Bullet bullet = bulletPool.Get();

// Return object to pool
bulletPool.Return(bullet);

// MonoBehaviour version (auto-managed)
public class BulletPool : MonoObjectPool<Bullet> { }
```

### 7. Singleton.cs - Manager Pattern
**Purpose**: Ensure single instances of managers
```csharp
// Persistent singleton (survives scene changes)
public class GameManager : Singleton<GameManager> {
    protected override void OnAwake() {
        // Initialization logic
    }
}

// Scene singleton (destroyed on scene change)
public class UIManager : SceneSingleton<UIManager> { }

// Usage
GameManager.Instance.DoSomething();
```

### 8. EventSystem.cs - Loose Coupling
**Purpose**: Type-safe event system for component communication
```csharp
// Subscribe to events
EventSystem.Subscribe<GameEvents.PlayerHealthChanged>((data) => {
    Debug.Log($"Health: {data.CurrentHealth}/{data.MaxHealth}");
});

// Publish events
EventSystem.Publish(new GameEvents.PlayerHealthChanged {
    CurrentHealth = 50,
    MaxHealth = 100
});

// Unsubscribe
EventSystem.Unsubscribe<GameEvents.PlayerHealthChanged>(handler);
```

### 9. MathUtility.cs - Common Math Functions
**Purpose**: Reusable math functions for games
```csharp
// Remap values
float healthBar = MathUtility.Remap(health, 0, 100, 0, 1);

// Angle utilities
float wrappedAngle = MathUtility.WrapAngle(450f); // Returns 90
float shortestDiff = MathUtility.ShortestAngle(350f, 10f); // Returns 20

// Easing functions
float easedValue = MathUtility.EaseOutExpo(t);
float bouncedValue = MathUtility.EaseOutBounce(t);

// Vector utilities
Vector2 circlePoint = MathUtility.PointOnCircle(center, radius, angle);
float distanceToLine = MathUtility.DistanceToLineSegment(point, lineStart, lineEnd);
```

### 11. SaveLoadUtility.cs - Data Persistence System
**Purpose**: Handle save/load operations with multiple formats and platforms
```csharp
// Save data in different formats
SaveLoadUtility.Save(playerData, "player.save", SaveLoadUtility.SaveFormat.JSON);
SaveLoadUtility.Save(gameSettings, "settings.save", SaveLoadUtility.SaveFormat.Binary);

// Load data with error handling
PlayerData loadedData = SaveLoadUtility.Load<PlayerData>("player.save");
if (loadedData != null) {
    // Use loaded data
}

// Check if save exists
if (SaveLoadUtility.SaveExists("player.save")) {
    // Load game
}

// Async operations
await SaveLoadUtility.SaveAsync(largeData, "bigfile.save");
var loaded = await SaveLoadUtility.LoadAsync<MyData>("bigfile.save");
```

### 12. SceneManagementUtility.cs - Scene Loading & Transitions
**Purpose**: Async scene management with progress tracking and transitions
```csharp
// Load scene with progress callback
await SceneManagementUtility.LoadSceneAsync("GameLevel",
    SceneManagementUtility.LoadMode.Single,
    progress => loadingBar.fillAmount = progress,
    scene => Debug.Log($"Loaded: {scene.name}")
);

// Transition between scenes
await SceneManagementUtility.TransitionScenes("Menu", "Game",
    transitionDelay: 1f,
    onProgress: progress => UpdateLoadingUI(progress),
    onTransitionStart: () => ShowLoadingScreen(),
    onTransitionComplete: () => HideLoadingScreen()
);

// Coroutine version for MonoBehaviours
StartCoroutine(SceneManagementUtility.LoadSceneCoroutine("Level1",
    onProgress: progress => Debug.Log($"Loading: {progress * 100}%")));
```

### 13. InputUtility.cs - Input Management & Device Detection
**Purpose**: Common input patterns, device detection, and input buffering
```csharp
// Detect current input device
InputUtility.InputDeviceType device = InputUtility.GetCurrentDeviceType();
if (device == InputUtility.InputDeviceType.Gamepad) {
    // Show gamepad controls
}

// Check for key holds
if (InputUtility.IsKeyHeld(KeyCode.Space, 2f)) {
    // Charged jump
}

// Get smoothed input
Vector2 smoothInput = InputUtility.GetInputAxis("Horizontal", "Vertical");
Vector2 wasdDirection = InputUtility.GetWASDDirection();

// Input buffering for combos
InputBuffer buffer = new InputBuffer(0.5f);
if (Input.GetKeyDown(KeyCode.A)) buffer.AddInput("LightAttack");
if (buffer.CheckSequence("LightAttack", "LightAttack", "HeavyAttack")) {
    // Perform combo
}

// Custom input bindings
InputUtility.RegisterBinding(new InputUtility.InputBinding(
    "Pause",
    () => Input.GetKeyDown(KeyCode.Escape),
    () => PauseGame()
));

// Update bindings in Update()
void Update() {
    InputUtility.UpdateBindings();
}
```

## 🎯 Usage Guidelines

### SOLID Principles
- **SRP**: Each utility has one clear responsibility
- **OCP**: Utilities are extensible through inheritance/interfaces
- **LSP**: Subtypes can replace base types
- **ISP**: Interfaces are focused and minimal
- **DIP**: High-level modules don't depend on low-level modules

### Performance Considerations
- Timer and UpdateUtility require manual Update() calls
- ObjectPool reduces garbage collection pressure
- MathUtility uses efficient algorithms
- EventSystem is type-safe and performant

### Integration with Unity
- Most utilities work without MonoBehaviour dependencies
- CoroutineUtility requires MonoBehaviour for execution
- Singleton handles DontDestroyOnLoad automatically
- Grid2D works with both 2D and 3D coordinates

## 📝 Adding New Utilities

When adding new utilities:
1. Follow naming convention: `UtilityName.cs`
2. Add to `AnomalyDirective.Utilities` namespace
3. Include comprehensive XML documentation
4. Provide usage examples
5. Update this README

## 🔄 Version History

- **v1.0**: Initial release with 10 core utilities
  - Timer, UpdateUtility, MonoBehaviourHooks
  - Grid2D, StateMachine, ObjectPool
  - Singleton, EventSystem, MathUtility, CoroutineUtility

- **v1.1**: Added advanced utilities for game development
  - SaveLoadUtility: Multi-format data persistence
  - SceneManagementUtility: Async scene loading with progress
  - InputUtility: Device detection and input management