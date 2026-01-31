# ✅ **Kalponic Studio — Universal Unity Coding Rules**

**Version 3.1 — Clean, Practical, Updated Standards**

*Last Updated: November 12, 2025*

A unified coding style and architectural guideline for all Kalponic Studio Unity projects, tools, and plugins.

---
**Rules**

* Runtime assemblies MUST NOT reference `UnityEditor`.
* Domain assemblies MUST NOT reference `UnityEngine`.
* Editor tools go under `_Tools`.
* Content assets (textures, prefabs, scenes) never mix with code.

---

# 🔤 **2. Naming Conventions**

| Type                  | Rule               | Example                                     |
| --------------------- | ------------------ | ------------------------------------------- |
| **Constants**         | `UPPER_SNAKE_CASE` | `MAX_HEALTH`, `DEFAULT_SPEED`               |
| **Properties**        | `PascalCase`       | `CurrentHealth`, `IsGrounded`               |
| **Fields**            | `camelCase`        | `moveSpeed`, `animationMap`                 |
| **Methods**           | `PascalCase()`     | `Move()`, `ApplyDamage()`                   |
| **Parameters**        | `camelCase`        | `time`, `direction`, `amount`               |
| **Events**            | C# event pattern   | `event Action<DamageEventArgs> DamageTaken` |
| **Event Raisers**     | `On` + PascalCase  | `OnDamageTaken()`                           |
| **Interfaces**        | `I` + PascalCase   | `IAnimator`, `ISaveSystem`                  |
| **ScriptableObjects** | Noun-based         | `EnemyConfig`, `AbilityData`                |
| **Folders**           | PascalCase         | `Animations`, `Prefabs`, `Data`             |

**Rules**

* Avoid names referencing IP, brands, or real persons.
* Class names must represent a clear purpose (SRP).

---

# 🧠 **3. Core Principles (Required)**

### ✅ **Single Responsibility (SRP)**

One class = one job.
If you can’t describe a class in one sentence, split it.

### ✅ **Loose Coupling**

Use **interfaces** or **ScriptableObjects** to reduce dependencies.

### ✅ **Minimal State**

No hidden state. Everything explicit and controlled.

### ✅ **Composition Over Inheritance**

Prefer components and SO-driven architectures over deep inheritance.

### ✅ **Law of Demeter**

Objects only talk to immediate dependencies.
No long chains like:
`player.transform.GetComponent<Weapon>().stats.damage`

---

# 🧱 **4. Architecture — Three-Layer Structure**

```
Domain (Pure C#)
↑
Application (Adapters, Services, Systems)
↑
Presentation (Unity)
```

### ✅ **Domain Layer**

* Minimize Unity types where possible.
* Core gameplay rules, math, events, ability systems, combat resolution.
* **Clarification**: Some Unity.Math types (float3, quaternion) are acceptable for performance, but avoid UnityEngine dependencies.

### ✅ **Application Layer**

* Services that bridge Unity and Domain.
* Input service, save service, animator adapter, VFX adapter.

### ✅ **Presentation Layer**

* Monobehaviours, UI Toolkit, animations, physics calls.
* Only reacts to domain events.

---

# 🧩 **5. ScriptableObject Rules**

Use ScriptableObjects for:

* Ability definitions
* Item definitions
* Stats
* Animation maps
* Loot tables
* Enemy configs
* Global game settings
* Editor pipeline settings

**Rules**

* No behavior-heavy logic inside SOs.
* SOs = **data only**, except simple validation or helper methods.
* Never reference scene objects from SOs.

---

# 🔧 **6. Update Loop Discipline**

### ✅ Allowed

* `Update()` only in Presentation
* Domain logic updated through:

  * Tick System
  * Explicit method calls
  * Events

### ❌ Not allowed

* Business logic inside Monobehaviour `Update()`
* Multiple uncontrolled Update calls

### ✅ Recommendation

Create a **central TickService** that updates domain systems.

---

# 🎮 **7. Animation System Rules**

* Never reference Animator directly in gameplay logic.
* Use an interface:

```
public interface IAnimator {
    void Play(string stateName, float speed = 1f, bool mirror = false);
}
```

* Map Unity Animator states via ScriptableObject:

  * `Idle → Idle_Clip`
  * `Slash → Slash_Clip`

* Domain triggers animation events; Application handles mapping.

---

# 📦 **8. Asset Workflow Rules**

### ✅ Folder rules

* No random assets in root.
* Every texture, prefab, animation must be inside a proper category folder.

### ✅ Naming

* Prefab: `Enemy_Goblin`, `Prop_Crate`
* Scene: `MainMenu`, `Level01_Forest`
* Materials: `MAT_Wood01`

---

# 🛠️ **9. Tools & Editor Rules**

* All tools must live under:

```
Tools → Kalponic Studio → <ToolName>
```

* Use UI Toolkit for all editor windows.
* No example data inside the main tool project (keep a Samples folder).
* Batch actions must have:

  * Preview
  * Confirm dialog
  * Logging

---

# 🚀 **10. Package & Dependency Rules**

* Pin exact versions in `Packages/manifest.json`.
* Never use "latest" or unstable preview packages.
* Allowed core packages:

  * URP
  * Input System
  * Addressables
  * UI Toolkit
  * Shader Graph
* Consider for performance-critical features:

  * Entities/DOTS (now stable and production-ready)
  * Unity Physics (DOTS-based physics)
* Avoid:

  * Experimental terrain tools
  * Deprecated packages (BinaryFormatter, etc.)

---

# 🧪 **11. Testing Rules**

### ✅ Pure C# tests allowed in `/Tests`

Test:

* Combat math
* Ability cooldown logic
* Inventory stacking
* Save/load
* Random generator

No UnityEngine tests inside pure domain tests.

---

# 💾 **12. Save System Rules**

* Use JSON, MessagePack, or custom binary serialization.
* Never use deprecated BinaryFormatter.
* Version your save files:

  * `SaveData_v1`
  * `SaveData_v2`
* Migrations must live beside DTOs.
* No direct scene references inside save files.

---

# ⚡ **13. Performance Rules**

* No allocations in hot paths (verify GC.Alloc).
* Prefer:

  * object pooling
  * pre-allocated lists
  * struct-based calculations
* Use profiler regularly:

  * Frame debugger
  * Memory profiler
  * CPU timeline

---

# 🧯 **14. Error Handling & Logging**

* No scattered `Debug.Log`.
* Use a centralized logger:

  * `KSLog.Info()`
  * `KSLog.Warning()`
  * `KSLog.Error()`
* For production builds: disable debug logs.

---

# 🌐 **15. Code Style**

### ✅ Clean

### ✅ Short methods

### ✅ Descriptive variable names

### ✅ Consistent spacing

### ✅ Early return strategy

Example:

```csharp
if (!isAlive)
    return;
```

---

# ✅ **16. Summary**

Your workflow must follow these core themes:

* **Clear structure**
* **Strict naming**
* **Separation of concerns**
* **ScriptableObject-driven data**
* **No hidden dependencies**
* **Minimal Update**
* **Pure domain logic isolated**
* **Unity only at the presentation layer**
* **Stable, safe, finishable systems**

This is the final, optimized, easy-to-share rulebook.

**Recent Updates (v3.1):**
- Updated package rules to reflect DOTS stability
- Removed deprecated BinaryFormatter reference
- Clarified domain layer Unity type usage
- Added modern serialization alternatives