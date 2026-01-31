# KS Health System - Scene Setup Guide

## 🎯 **Quick Scene Setup (5 Minutes)**

### **Step 1: Create Player GameObject**
```
Hierarchy → Right-click → Create Empty
Name: "Player"
Add Components:
- Rigidbody2D (for physics)
- SpriteRenderer (for visuals)
- Any movement script
```

### **Step 2: Add Health System**
```
Select Player → Add Component → HealthSystem

Inspector Settings:
- Max Health: 100
- Current Health: 100
- Enable Invulnerability: true
- Invulnerability Duration: 1.0
- Regenerate Health: false (or true for auto-heal)
```

### **Step 3: Add Visual Effects (Optional)**
```
Select Player → Add Component → HealthVisualSystem

Required Setup:
- Drag Player's SpriteRenderer to "Main Renderer"
- Create UI Images for screen flash (optional)
- Add Particle Systems (optional)
- Add AudioSource + audio clips (optional)
```

### **Step 4: Create Event Channel**
```
Project Window → Right-click → Create → Kalponic Studio → Health → Event Channel
Name: "PlayerHealthEvents"
Save in: Assets/_Project/Scripts/KS Health System/Resources/
```

### **Step 5: Connect Events**
```
Select Player:
- Drag PlayerHealthEvents to HealthSystem → Health Events
- Drag PlayerHealthEvents to HealthVisualSystem → Health Events
```

---

## 🧪 **Test Your Setup**

### **Create Test Script**
```csharp
using UnityEngine;
using KalponicStudio.Health;

public class HealthTester : MonoBehaviour
{
    [SerializeField] private HealthSystem health;

    void Update()
    {
        // Press D to take damage
        if (Input.GetKeyDown(KeyCode.D))
            health.TakeDamage(20);

        // Press H to heal
        if (Input.GetKeyDown(KeyCode.H))
            health.Heal(15);

        // Press K to die
        if (Input.GetKeyDown(KeyCode.K))
            health.Kill();
    }
}
```

### **Add to Scene**
```
Create Empty GameObject → Name: "HealthTester"
Add Component → HealthTester script
Drag Player's HealthSystem to the Health field
```

### **Test Results**
- **D key**: Should flash red, take 20 damage
- **H key**: Should flash green, heal 15 health
- **K key**: Should trigger death effects

---

## 🎨 **Visual Effects Setup**

### **Screen Flash (Recommended)**
```
1. Create UI Canvas (if none exists)
2. Add UI → Image (name: "DamageFlash")
3. Set Color: Red (0,0,0,0) - transparent
4. Drag to HealthVisualSystem → Damage Flash Image
5. Repeat for Heal Flash with green color
```

### **Camera Shake (Optional)**
```
1. Add CameraShake component to Main Camera
2. Drag Main Camera to HealthVisualSystem → Camera Shake
3. Adjust shake intensity in HealthVisualSystem
```

### **Particles (Optional)**
```
1. Create Particle System on Player
2. Drag to HealthVisualSystem → Damage/Heal Particles
3. Configure particle effects as desired
```

---

## 🔊 **Audio Setup (Optional)**

### **Add Audio**
```
1. Add AudioSource to Player
2. Drag AudioSource to HealthVisualSystem
3. Add audio clips for:
   - Damage Sound
   - Heal Sound
   - Death Sound
   - Low Health Sound
```

---

## 📊 **UI Health Bar (Optional)**

### **Create Health Bar**
```csharp
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private HealthSystem health;
    [SerializeField] private Slider slider;

    void Start()
    {
        slider.maxValue = health.MaxHealth;
        slider.value = health.CurrentHealth;
    }

    void Update()
    {
        slider.value = health.CurrentHealth;
    }
}
```

### **Setup in Scene**
```
1. Create UI Canvas
2. Add UI Slider
3. Add HealthBar script to Canvas
4. Drag Player HealthSystem to script
5. Drag Slider to script
```

---

## � **Common Issues**

### **"HealthSystem not found"**
- Make sure HealthSystem component is added to Player
- Check script execution order if needed

### **"Event channel not working"**
- Verify event channel is assigned to both systems
- Check that listeners are properly subscribed

### **"Visual effects not showing"**
- Ensure SpriteRenderer is assigned to HealthVisualSystem
- Check that UI Images are properly set up
- Verify particle systems are configured

### **"Audio not playing"**
- Add AudioSource component to Player
- Assign audio clips in HealthVisualSystem
- Check volume settings

---

## ✅ **Success Checklist**

- [ ] Player GameObject exists with Rigidbody2D
- [ ] HealthSystem component added and configured
- [ ] HealthVisualSystem component added (optional)
- [ ] HealthEventChannelSO created and assigned
- [ ] Test script working (D/H/K keys)
- [ ] Visual effects configured (optional)
- [ ] Audio working (optional)
- [ ] UI health bar connected (optional)

---

## 🚀 **Next Steps**

1. **Replace test script** with actual game logic
2. **Add enemy health** using same setup
3. **Connect to game UI** (HUD, menus)
4. **Add shield system** if needed
5. **Test edge cases** (0 health, max health, etc.)

**Your health system is now ready for gameplay!** 🎮

---

## 🎨 **UI Components (NEW!)**

The health system now includes a complete set of modular UI components in the `UI/` folder:

### **HealthUIController**
Main coordinator that manages health bar, text, and icon components automatically.

### **HealthBar**
- Customizable gradient colors (red→yellow→green)
- Smooth animations
- Custom sprites support
- Invert fill direction option

### **HealthText**
- Multiple formats: "50/100", "50%", "HP: 50/100 (50%)"
- Health-based color gradients
- TextMeshPro support
- Animated value changes

### **HealthIcon**
- State-based icons (healthy/damaged/critical)
- Configurable health thresholds
- Color tinting per state
- Smooth transitions

### **HealthUIManager**
- Manage multiple health UIs (player + enemies)
- Batch operations
- Utility methods for creating UIs

**Quick UI Setup:**
```
1. Add HealthUIController to a Canvas GameObject
2. Add HealthBar/HealthText/HealthIcon as children
3. Assign HealthSystem to the controller
4. Customize colors/icons as needed
```

See `UI/README.md` for detailed documentation!</content>
<parameter name="filePath">f:\Unity Workplace\Anomaly Directive\Assets\_Project\Scripts\KS Health System\README.md