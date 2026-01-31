# KS Tooltip System

A clean, SOLID, game-agnostic tooltip system for Unity that follows KISS principles.

## 🎯 Design Principles

- **KISS**: Keep it simple - minimal interfaces, clear responsibilities
- **SOLID**: Single responsibility, open/closed, dependency inversion
- **Game-Agnostic**: Works with any game type or UI framework
- **Dependency Injection**: No singletons, maximum testability

## 🏗️ Architecture

### Core Components

- **`ITooltipData`**: Data contract for tooltip content
- **`ITooltipService`**: Main service interface
- **`ITooltipDisplay`**: UI framework abstraction
- **`ITooltipPositioner`**: Positioning logic abstraction
- **`TooltipConfig`**: Configuration data

### Concrete Implementations

- **`TooltipData`**: ScriptableObject for tooltip content
- **`TooltipManager`**: Main service implementation
- **`UIToolkitTooltipDisplay`**: UI Toolkit display implementation
- **`MouseFollowPositioner`**: Mouse-following positioner
- **`TooltipTrigger`**: Generic trigger component

## 🚀 Quick Setup

1. **Create TooltipManager** in your scene
2. **Add UIToolkitTooltipDisplay** component to the same GameObject
3. **Create UIDocument** with tooltip UI elements
4. **Create TooltipData** assets for your content
5. **Attach TooltipTrigger** to UI elements that need tooltips

### Required UI Structure

Your UIDocument must contain:
```xml
<VisualElement name="TooltipContainer">
    <Label name="TooltipTitle" />
    <Label name="TooltipDescription" />
</VisualElement>
```

## 📝 Usage Examples

### Basic Setup
```csharp
// Create tooltip data
var tooltipData = TooltipData.CreateRuntime("My Title", "My description");

// Attach to GameObject
var trigger = gameObject.AddComponent<TooltipTrigger>();
trigger.SetTooltipData(tooltipData);
```

### Custom Configuration
```csharp
var config = new TooltipConfig
{
    ShowDelay = 1.0f,
    HideDelay = 0.3f,
    Offset = new Vector2(15, 15),
    FollowMouse = true,
    ClampToScreen = true
};

tooltipManager.UpdateConfiguration(config);
```

## 🔧 Extension Points

### Custom Display Implementation
```csharp
public class CustomTooltipDisplay : MonoBehaviour, ITooltipDisplay
{
    // Implement interface methods
    public void ShowTooltip(ITooltipData data, Vector2 position) { /* ... */ }
    // ...
}
```

### Custom Positioner
```csharp
public class AnchoredPositioner : ITooltipPositioner
{
    // Implement positioning logic
    public Vector2 CalculatePosition(Vector2 trigger, Vector2 size, Vector2 screen) { /* ... */ }
    // ...
}
```

## ✅ Benefits

- **Framework Agnostic**: Works with UI Toolkit, UGUI, IMGUI, or custom UI
- **Testable**: Dependency injection makes unit testing easy
- **Extensible**: Easy to add new display or positioning strategies
- **Performance**: Minimal overhead, efficient updates
- **Clean Code**: Follows SOLID principles and clean architecture

## 🐛 Troubleshooting

- **Tooltips not showing**: Ensure UIToolkitTooltipDisplay is in scene
- **UI not found**: Check UIDocument has correct element names
- **Positioning issues**: Verify UI is in Screen Space Overlay mode

---

**Made with ❤️ by Kalponic Studio**</content>
<parameter name="filePath">f:\Unity Workplace\Anomaly Directive\Assets\Plugins\KS Tooltips\README.md