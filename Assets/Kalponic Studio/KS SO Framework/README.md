# KS SO Framework - Enhanced Edition v3.0

**Unity UI Toolkit Integration & Advanced Features**

An enhanced ScriptableObject framework with professional inspector experience, reactive UI bindings, event systems, and advanced data structures.

## 🎯 What's New in v3.0

### ✨ Unity UI Toolkit Integration
- **Reactive UI Bindings**: Automatic synchronization between ScriptableVariables and UI elements
- **LabelBinding**: Display variable values in text elements
- **SliderBinding**: Two-way binding for numeric variables with range support
- **ToggleBinding**: Boolean variable binding with checkboxes
- **ImageFillBinding**: Visual progress bars from numeric variables
- **TextFieldBinding**: Two-way string input binding

### 🎧 Event Listener Components
- **VoidEventListener**: Respond to parameterless events with UnityEvents
- **TypedEventListener**: Handle typed events (int, float, bool, string, Vector3, Color, GameObject)
- **ConditionalEventListener**: Advanced conditional responses
- **AnimationEventListener**: Trigger Mecanim animations
- **AudioEventListener**: Play audio clips with randomization

### 🔗 Variable Reference System
- **VariableReference**: Indirect variable access for better decoupling
- **DynamicVariableReference**: Runtime type-flexible references
- **Reference Types**: Int, Float, Bool, String, Vector3, Color references

### 🌐 Scriptable Singletons
- **GameSettings**: Audio, graphics, and gameplay configuration
- **GameState**: Progress tracking and resource management
- **InputSettings**: Mouse, controller, and key binding configuration

### 📊 Advanced Data Structures
- **ScriptableDictionary**: Key-value pair storage with serialization
- **ScriptableEnum**: Dynamic enum creation with associated data
- **Dictionary Types**: String, String-Int, Prefab dictionaries

## 📦 Installation

### Enhanced Framework (Recommended)
1. Copy `Assets/Kalponic Studio/KS SO Framework/` to your project
2. The framework includes all features: NaughtyAttributes, UI Toolkit bindings, event listeners, and advanced structures
3. All dependencies are included in the assembly

### Core Features
- **ScriptableVariable<T>**: Base variable class with persistence and events
- **ScriptableEvent<T>**: Event system for decoupled communication
- **Enhanced Types**: Range variables, string utilities, color presets, vector controls
- **UI Bindings**: Reactive connections to Unity UI Toolkit elements
- **Event Listeners**: Drag-and-drop event response components
- **Variable References**: Indirect variable access patterns
- **Singletons**: Global configuration and state management
- **Advanced Structures**: Dictionaries and dynamic enums

## � Folder Structure

The framework is organized into logical folders for easy navigation:

```
KS SO Framework/
├── Core/                          # Base classes and NaughtyAttributes
│   ├── ScriptableVariable.cs      # Base variable implementation
│   ├── ScriptableEvent.cs         # Base event implementation
│   └── [NaughtyAttributes files]  # Inspector enhancement library
├── Variables/                     # Variable type implementations
│   ├── EnhancedVariableTypes.cs   # Range, color, vector variables
│   └── VariableTypes.cs           # Basic variable types
├── Events/                        # Event system components
│   ├── EventListeners.cs          # MonoBehaviour event listeners
│   └── EventTypes.cs              # Event type definitions
├── UI/                            # UI Toolkit bindings
│   └── UIToolkitBindings.cs       # Reactive UI components
├── References/                    # Variable reference system
│   └── VariableReferences.cs      # Indirect variable access
├── Singletons/                    # Global singleton implementations
│   └── ScriptableSingletons.cs    # GameSettings, GameState, etc.
├── DataStructures/                # Advanced data containers
│   ├── AdvancedDataStructures.cs  # Dictionaries and enums
│   ├── ListTypes.cs              # List implementations
│   └── ScriptableList.cs         # Base list class
├── Tests/                         # Testing and validation
│   └── KS_SO_Framework_Test.cs    # Comprehensive test suite
├── Documentation/                 # Documentation and guides
│   └── README.md                  # This file
├── KalponicStudio.SO_Framework.asmdef  # Assembly definition
└── [*.meta files]                 # Unity metadata
```

## �🚀 Quick Start

### 1. Create Enhanced Variables
```
Right-click in Project window → Create → SO Framework/Variables/
- Int Range Variable (clamped values with validation)
- Float Range Variable (precision control with ranges)
- Enhanced String Variable (length limits, auto-trim)
- Color Variable (HDR support, manipulation tools)
- Vector3 Variable (normalization, directional presets)
- Vector2 Variable (2D vector utilities)
- Vector2Int Variable (grid-based operations)
- Quaternion Variable (rotation utilities)
- Component Variable (Unity component references)
- LayerMask Variable (layer selection helpers)
```

### 2. Set Up UI Bindings
```csharp
// 1. Create UIDocument with UI elements
// 2. Add binding components to your GameObject
// 3. Assign ScriptableVariables to binding components
// 4. UI automatically updates when variables change

// Example: Health bar binding
var healthBar = gameObject.AddComponent<SliderBinding>();
healthBar.DataSource = healthVariable; // IntRangeVariable
healthBar.ElementName = "HealthSlider";
healthBar.UseRangeVariableLimits = true;
healthBar.Bind(); // Connect to UI
```

### 3. Use Event Listeners
```csharp
// 1. Create event listener component
// 2. Assign ScriptableEvent
// 3. Configure response actions

// Example: Damage event listener
var damageListener = gameObject.AddComponent<IntEventListener>();
damageListener.EventSource = damageEvent;
damageListener.OnEventTriggered.AddListener(value => TakeDamage(value));
damageListener.StartListening();
```

### 4. Leverage Advanced Features
```csharp
// Variable References for decoupling
var healthRef = CreateInstance<IntVariableReference>();
healthRef.UseReference = true;
healthRef.ReferencedVariable = playerHealthVariable;

// Scriptable Singletons for global access
GameSettings.Instance.SetMasterVolume(0.8f);
GameState.Instance.AddGold(100);

// Advanced data structures
var itemDictionary = CreateInstance<StringIntDictionary>();
itemDictionary.Add("HealthPotion", 50);
itemDictionary.Add("ManaPotion", 30);
```

## 🎨 Inspector Experience

### Variable Organization
```
Value Configuration
┌─────────────────────────────────┐
│ Initial Value: 0                │
│ Runtime Value: 0  [ReadOnly]    │
└─────────────────────────────────┘

Persistence Settings
┌─────────────────────────────────┐
│ □ Save Value                    │
│ Save Key: [Conditional]         │
│ Reset Type: SceneLoaded         │
└─────────────────────────────────┘

Debug Information
┌─────────────────────────────────┐
│ Change Count: 0  [ReadOnly]     │
│ Last Changed: Never  [ReadOnly] │
│ [Reset to Initial Value]        │
└─────────────────────────────────┘
```

### UI Binding Configuration
```
Binding Configuration
┌─────────────────────────────────┐
│ Data Source: HealthVariable     │
│ Element Name: HealthBar         │
│ □ Bind On Start                 │
│ □ Two Way Binding               │
└─────────────────────────────────┘

Debug Information
┌─────────────────────────────────┐
│ □ Is Bound                      │
│ Last Update: Never              │
│ [Bind Now] [Update UI]          │
└─────────────────────────────────┘
```

## 🔧 Advanced Usage

### Reactive UI Patterns
```csharp
// One-way binding (UI reflects variable)
public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private LabelBinding _healthLabel;

    void Start()
    {
        _healthLabel.Bind(); // UI updates when health changes
    }
}

// Two-way binding (UI can modify variable)
public class VolumeControl : MonoBehaviour
{
    [SerializeField] private SliderBinding _volumeSlider;

    void Start()
    {
        _volumeSlider.TwoWayBinding = true;
        _volumeSlider.Bind(); // Slider changes update volume variable
    }
}
```

### Event-Driven Architecture
```csharp
// Publisher
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private IntEventSO _damageEvent;

    public void TakeDamage(int damage)
    {
        _damageEvent.Raise(damage);
    }
}

// Subscriber (no code needed with listeners)
[AddComponentMenu("Game/Player Damage Handler")]
public class DamageHandler : MonoBehaviour
{
    [SerializeField] private IntEventListener _damageListener;

    void Start()
    {
        _damageListener.OnEventTriggered.AddListener(HandleDamage);
    }

    private void HandleDamage(int damage)
    {
        // Handle damage logic
    }
}
```

### Variable Reference Patterns
```csharp
// Configuration through references
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private FloatVariableReference _speed;
    [SerializeField] private BoolVariableReference _isAggressive;

    void Update()
    {
        float currentSpeed = _speed.EffectiveValue;
        bool aggressive = _isAggressive.EffectiveValue;
        // Use values...
    }
}
```

## 📚 API Reference

### Core Classes
- **ScriptableVariable<T>**: Base variable with events, persistence, validation
- **ScriptableEvent<T>**: Event system for decoupled communication
- **UIToolkitDataBinding**: Base class for UI Toolkit bindings
- **EventListenerBase**: Base class for event listener components
- **VariableReference<T>**: Indirect variable access system
- **ScriptableSingleton<T>**: Global access pattern base class
- **ScriptableDictionary<TKey,TValue>**: Serializable key-value storage
- **ScriptableEnum**: Dynamic enum system

### UI Binding Components
- **LabelBinding**: Text display binding
- **SliderBinding**: Numeric slider binding
- **ToggleBinding**: Boolean toggle binding
- **ImageFillBinding**: Visual progress binding
- **TextFieldBinding**: Text input binding

### Event Listener Components
- **VoidEventListener**: Parameterless event handling
- **IntEventListener**: Integer event handling
- **FloatEventListener**: Float event handling
- **BoolEventListener**: Boolean event handling
- **StringEventListener**: String event handling
- **Vector3EventListener**: Vector3 event handling
- **ColorEventListener**: Color event handling
- **GameObjectEventListener**: GameObject event handling
- **ConditionalEventListener**: Advanced conditional responses
- **AnimationEventListener**: Animation triggering
- **AudioEventListener**: Audio playback

### Variable Reference Types
- **IntVariableReference**: Integer variable references
- **FloatVariableReference**: Float variable references
- **BoolVariableReference**: Boolean variable references
- **StringVariableReference**: String variable references
- **Vector3VariableReference**: Vector3 variable references
- **ColorVariableReference**: Color variable references
- **DynamicVariableReference**: Runtime-flexible references

### Scriptable Singletons
- **GameSettings**: Global game configuration
- **GameState**: Global game state management
- **InputSettings**: Global input configuration

### Advanced Data Structures
- **StringDictionary**: String-to-string mapping
- **StringIntDictionary**: String-to-integer mapping
- **PrefabDictionary**: String-to-prefab mapping
- **ScriptableEnum**: Dynamic enum creation
- **ItemRarityEnum**: Rarity system with colors
- **GameStateEnum**: Game state management

## 🧪 Testing the Framework

### Comprehensive Test Script
1. Open Unity and create a new scene
2. Create a GameObject and attach the `KS_SO_Framework_Test` component
3. Create the required ScriptableObjects:
   - Variables: IntVariable, FloatVariable, BoolVariable, StringVariable
   - Events: IntEvent, VoidEvent
   - Lists: IntList, StringList
   - Enhanced: IntRangeVariable, ColorVariable, Vector3Variable
   - UI Bindings: Create UIDocument with test elements
   - Event Listeners: Attach listener components
   - References: Create variable references
   - Singletons: Create singleton instances
   - Dictionaries: Create dictionary instances
   - Enums: Create enum instances
4. Assign them to the test component
5. Run the scene or use the "Run Tests" context menu
6. Check the console for test results

### UI Toolkit Testing
1. Create a UIDocument in your scene
2. Add UI elements (Label, Slider, Toggle, etc.) with names
3. Create binding components on a GameObject
4. Assign variables and element names
5. Test that UI updates when variables change
6. Test two-way binding by modifying UI elements

### Event System Testing
1. Create ScriptableEvents
2. Add event listener components to GameObjects
3. Configure response actions
4. Raise events and verify responses trigger
5. Test conditional and specialized listeners

## 🐛 Troubleshooting

### UI Toolkit Bindings Not Working
- Ensure UIDocument component exists and is active
- Check that element names match exactly (case-sensitive)
- Verify UIDocument.rootVisualElement is not null
- Check console for binding errors

### Event Listeners Not Responding
- Ensure event source is assigned
- Check that StartListening() has been called
- Verify event types match (typed vs void events)
- Check for null reference exceptions in response actions

### Variable References Not Working
- Ensure UseReference is checked
- Verify referenced variable type matches reference type
- Check that referenced variable is not null
- For dynamic references, ensure target type is set

### Singletons Not Accessible
- Singletons are created on first access
- Check that singleton script is in Resources folder or preloaded
- Ensure no duplicate singleton instances exist
- Use ResetInstance() to clear corrupted state

### Dictionary Serialization Issues
- Dictionaries serialize keys and values separately
- Ensure key types are serializable by Unity
- Check for null keys (not allowed)
- Use OnValidate to synchronize lists

## 🤝 Credits & License

### NaughtyAttributes Integration
This enhanced version includes NaughtyAttributes by [dbrizov](https://github.com/dbrizov/NaughtyAttributes)

### Unity UI Toolkit
Built on Unity's modern UI system for scalable, responsive interfaces.

### Original KS SO Framework
Developed by Kalponic Studio

**Framework Version:** 3.0 | **Unity 6.2 Compatible** | **UI Toolkit Integrated**

---

**Enhanced by Kalponic Studio** | **Framework Version:** 3.0 | **Unity 6.2 Compatible**

*For detailed API documentation, see the source code comments in each class.*