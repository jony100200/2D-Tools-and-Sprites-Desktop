# KS Health System UI Components

## 🎨 **Overview**

The KS Health System UI provides modular, customizable UI components specifically designed for displaying health information. Each component is designed to be easily configurable and can work independently or together.

## 📁 **Components**

### **HealthUIController.cs**
**Main coordinator** for health UI components. Manages multiple UI elements and coordinates their updates.

**Key Features:**
- Auto-finds UI components in children
- Event-driven updates from HealthSystem
- Real-time or event-based updates
- Public API for dynamic health system assignment

### **HealthBar.cs**
**Visual health bar** with gradient colors and smooth animations.

**Key Features:**
- Customizable gradient colors (red→yellow→green)
- Smooth animation transitions
- Custom sprites for fill/background
- Invert fill direction option

### **HealthText.cs**
**Text display** for health values with multiple formatting options.

**Key Features:**
- Multiple display formats (Current/Max/Percentage)
- Custom formatting strings
- Health-based color gradients
- Animated value changes
- TextMeshPro support

### **HealthIcon.cs**
**Icon display** that changes based on health percentage.

**Key Features:**
- Multiple health states with different icons
- Configurable health thresholds
- Color tinting per state
- Smooth transitions between states
- Custom icon sizes

### **HealthUIManager.cs**
**Manager** for multiple health UI instances (players, enemies, etc.).

**Key Features:**
- Register/unregister multiple health UIs
- Batch operations on all UIs
- Utility methods for creating player/enemy UIs
- ID-based UI management

---

## 🚀 **Quick Setup**

### **Basic Health Bar**
```
1. Create UI Canvas
2. Add Slider component
3. Add HealthBar script to Slider
4. In HealthUIController, assign the HealthBar
5. Done! Auto-connects to HealthSystem
```

### **Health Text Display**
```
1. Add TextMeshPro Text to Canvas
2. Add HealthText script
3. Configure format (Current/Max/Percentage)
4. Assign to HealthUIController
```

### **Health Icon**
```
1. Add Image component to Canvas
2. Add HealthIcon script
3. Add health states with icons/colors
4. Assign to HealthUIController
```

---

## 🎛️ **Customization Examples**

### **Custom Health Bar Colors**
```csharp
// Get HealthBar component
HealthBar healthBar = GetComponent<HealthBar>();

// Create custom gradient
Gradient customGradient = new Gradient();
customGradient.colorKeys = new GradientColorKey[] {
    new GradientColorKey(Color.blue, 0f),    // Low health
    new GradientColorKey(Color.cyan, 0.5f),  // Medium health
    new GradientColorKey(Color.white, 1f)    // Full health
};

healthBar.SetFillGradient(customGradient);
```

### **Custom Text Format**
```csharp
HealthText healthText = GetComponent<HealthText>();

// Use custom format
healthText.SetCustomFormat("HP: {CURRENT}/{MAX} ({PERCENTAGE}%)");

// Or use preset formats
healthText.SetFormat(HealthText.HealthTextFormat.CurrentAndPercentage);
```

### **Health State Icons**
```csharp
HealthIcon healthIcon = GetComponent<HealthIcon>();

// Add custom states
healthIcon.AddHealthState("Critical", 0f, criticalIcon, Color.red, new Vector2(32, 32));
healthIcon.AddHealthState("Damaged", 0.25f, damagedIcon, Color.yellow, new Vector2(32, 32));
healthIcon.AddHealthState("Healthy", 0.75f, healthyIcon, Color.green, new Vector2(32, 32));
```

---

## 🔧 **Advanced Usage**

### **Multiple Characters**
```csharp
// Use HealthUIManager for multiple characters
HealthUIManager uiManager = GetComponent<HealthUIManager>();

// Register player UI
uiManager.RegisterHealthUI("Player", playerUIController, playerHealthSystem);

// Register enemy UIs
foreach (var enemy in enemies) {
    uiManager.CreateEnemyHealthUI(enemy.gameObject);
}
```

### **Dynamic UI Creation**
```csharp
// Create UI at runtime
GameObject uiObject = new GameObject("DynamicHealthUI");
HealthUIController controller = uiObject.AddComponent<HealthUIController>();

// Add components programmatically
HealthBar bar = uiObject.AddComponent<HealthBar>();
HealthText text = uiObject.AddComponent<HealthText>();

// Connect to health system
controller.SetHealthSystem(targetHealthSystem);
```

### **Event-Based Updates**
```csharp
// HealthUIController automatically subscribes to health events
// No manual polling needed!

// For custom event handling
healthSystem.Death += () => {
    // Custom death UI logic
    healthUIController.gameObject.SetActive(false);
};
```

---

## 🎨 **UI Toolkit Integration**

For Unity UI Toolkit compatibility, the components are designed to work alongside UITK:

### **Hybrid Approach**
- Use UITK for main UI layout
- Use these components for health-specific displays
- Components work with both Canvas and UITK

### **UITK Health Bar**
```csharp
// Can create UITK versions of these components
// Using VisualElement instead of Canvas
public class UITKHealthBar : VisualElement {
    // Similar API but using UITK elements
}
```

---

## 📊 **Performance Notes**

- **Event-driven**: Only updates when health changes
- **Object pooling ready**: Components can be pooled
- **Minimal allocations**: Uses cached strings and objects
- **Batch updates**: HealthUIManager can update multiple UIs efficiently

---

## 🐛 **Common Issues**

### **UI Not Updating**
- Check HealthUIController is assigned to HealthSystem
- Verify event subscription in OnEnable/OnDisable
- Ensure UI components are children of controller

### **Text Not Showing**
- Verify TextMeshPro is installed
- Check font asset is assigned
- Ensure text component is enabled

### **Icons Not Changing**
- Verify health states are configured correctly
- Check health percentages are in correct range
- Ensure sprites are assigned to states

### **Performance Issues**
- Disable real-time updates if not needed
- Use object pooling for multiple enemies
- Batch UI updates in LateUpdate

---

## ✅ **Component Checklist**

- [ ] HealthUIController attached and configured
- [ ] HealthSystem assigned to controller
- [ ] UI components (Bar/Text/Icon) added as needed
- [ ] Canvas render mode set correctly
- [ ] TextMeshPro installed (if using text)
- [ ] Sprites assigned (if using icons)
- [ ] Colors/gradients configured
- [ ] Animation settings adjusted

---

## 🚀 **Next Steps**

1. **Test in Scene**: Create test scene with health UI
2. **Customize Appearance**: Adjust colors, fonts, icons
3. **Add Animations**: Configure smooth transitions
4. **Multiple Characters**: Use HealthUIManager for complex scenes
5. **UITK Migration**: Consider UITK versions for future projects

---

*These UI components are designed to be **modular, performant, and easily customizable** for any health display needs!* 🎮
