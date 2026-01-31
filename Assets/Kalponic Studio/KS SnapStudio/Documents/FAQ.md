# KS SnapStudio FAQ

## 📋 Table of Contents

- [General Questions](#-general-questions)
- [Technical Requirements](#-technical-requirements)
- [Installation & Setup](#-installation--setup)
- [Usage Questions](#-usage-questions)
- [Capture Issues](#-capture-issues)
- [Output & Quality](#-output--quality)
- [Pricing & Licensing](#-pricing--licensing)
- [Support](#-support)

---

## ❓ General Questions

### What is KS SnapStudio?
KS SnapStudio is a professional Unity editor tool that converts 3D character animations into high-quality 2D sprite sequences. Perfect for 2D game development, asset creation, and marketing materials.

### Who is this tool for?
- **Indie Game Developers** creating 2D games and studios from 3D assets
- **Asset Creators** producing sprite libraries
- **Marketing Teams** generating character thumbnails
- **Animation Studios** batch processing character animations
- **Educators** teaching animation principles

### What's the difference between KS SnapStudio and KS ThumbSmith?
- **KS SnapStudio**: Full animation capture with frame sequences
- **KS ThumbSmith**: Standalone thumbnail generation (currently free)

---

## 💻 Technical Requirements

### What Unity versions are supported?
- Unity 2021.3 or higher
- Unity 6 fully supported
- Works on Windows, macOS, and Linux

### What render pipelines work?
- **Universal Render Pipeline (URP)** - Recommended
- **Built-in Render Pipeline** - Fully supported
- **HDRP** - Compatible but may require adjustments

### System requirements?
- **RAM**: 4GB minimum, 8GB recommended
- **Storage**: 500MB free space for captures
- **GPU**: Any DirectX 11 compatible GPU
- **CPU**: Multi-core processor recommended

### Can I use it in commercial projects?
Yes! KS SnapStudio is available for commercial use. You can create and sell games and studios/assets created with the tool.

---

## 📦 Installation & Setup

### How do I install KS SnapStudio?
1. Download from Unity itch.io
2. Import package files into your Unity project
3. Restart Unity if prompted
4. Access via Tools → Kalponic Studio → Animation → KS SnapStudio

### Do I need to set up anything special?
No special setup required! The tool handles:
- Scene creation and cleanup
- Lighting configuration
- Camera positioning
- Output folder organization

### Can I use it with existing projects?
Yes, it works with any Unity project containing 3D characters with animations.

---

## 🎮 Usage Questions

### What types of characters can I capture?
Any 3D character with:
- **Animator Component** (Mecanim system) - Recommended
- **Legacy Animation Component** - Also supported
- **Animation Clips** assigned to the character

### Can I capture multiple animations at once?
Yes! Enable "Capture All Animations" to process entire animation libraries in one batch. Add animation controller and all the animations you want to capture for that character to that character animation controller. No connection needed.

### How do I navigate between tabs?
**Mouse:** Click any tab button at the top of the window.

**Keyboard:** 
- Press `Left Arrow` or `Right Arrow` keys to switch tabs
- The window must have focus (click anywhere in the window first)
- Navigation wraps around (last tab → first tab)

**Persistence:** Your selected tab is automatically remembered and restored when reopening the window.

### Why do tabs sometimes show errors?
Tabs remain clickable even if their content fails to load. This ensures you can always access working features. Check the Unity Console for detailed error messages if a tab shows error content.

### Can I use multiple tabs at the same time?
No, only one tab's content is visible at a time. However, settings from different tabs are independent and don't interfere with each other.

### How long does capture take?
Depends on:
- Animation length (seconds)
- FPS setting (frames per second)
- Resolution (higher = longer)
- Hardware performance

Typical times: 30 seconds to 5 minutes per animation.

### Can I pause or cancel capture?
Currently, captures run continuously. For long animations, consider:
- Reducing FPS setting
- Using shorter animation clips
- Processing in smaller batches
- Press stop play button to halt capture immediately for animations.

### What happens to my original scene?
The tool:
1. Creates a temporary capture scene. Please make a new scene to start capturing.
2. Performs the capture
3. Automatically restores your original scene
4. Cleans up temporary assets

---

## 🚨 Capture Issues

### "No target selected" error
**Solution**: Drag your character GameObject from the Hierarchy into the Target field.

### "Target has no animations" error
**Cause**: Character missing Animator or Animation component
**Solution**:
1. Add Animator component to character
2. Assign Animator Controller or Animation Clips
3. Ensure animations are properly configured

### "Not in Play Mode" error
**Solution**: Click "Start Capture" and allow Unity to enter Play Mode automatically.

### Capture starts but produces black/empty sprites
**Possible causes**:
- Character not visible to camera
- Incorrect lighting setup
- Material issues with render pipeline
- Character positioned outside camera view

**Solutions**:
- Check character position in capture scene
- Verify materials are compatible
- Test with simple character first

### Memory errors during capture
**Solutions**:
- Reduce resolution (try 512x512 for testing)
- Lower FPS setting
- Process fewer animations at once
- Close other applications to free memory

### Poor sprite quality or artifacts
**Possible causes**:
- Low resolution settings
- Fast moving animations
- Complex shaders/materials
- Insufficient lighting

**Solutions**:
- Increase resolution
- Adjust lighting in capture scene
- Simplify materials if possible
- Use higher quality settings

---

## 🖼️ Output & Quality

### What format are sprites saved in?
PNG with transparency, perfect for 2D games and studios and Unity sprite import.

### How are files organized?
```
OutputFolder/
├── CharacterName_AnimationName_001.png
├── CharacterName_AnimationName_002.png
├── CharacterName_AnimationName_003.png
└── ...
```

### Can I change the naming convention?
Currently uses `CharacterName_AnimationName_FrameNumber.png` format. Custom naming coming in future updates.

### Why are some sprites trimmed differently?
Trimming removes transparent borders. For consistent sizes, disable trimming in settings.

### Can I adjust sprite pivot points?
Yes! The tool automatically calculates optimal pivot points based on character center.

### What's the best resolution for my game?
Depends on your game:
- **Pixel Art**: 64x64 to 256x256
- **HD 2D**: 512x512 to 1024x1024
- **Marketing**: 1024x1024 or higher

---

## 💰 Pricing & Licensing

### How much does KS SnapStudio cost?
- **KS SnapStudio**: $10 (Animation Capture + Thumbnail Generation). Launch discount $5


### Is there a free trial?
Thumbnail generation stand alone is free.Both animation capture and thumbnail generation are included in the $5 package. 

### Can I use it commercially?
Yes! Create and sell games and studios/assets made with KS SnapStudio.

### What is the refund policy?
**No refunds** - "What you see is what you get" policy for digital software. All purchases are final. Contact support with any questions before purchasing.

### Team licensing?
Individual license per developer. Contact us for team discounts.

---

## 🆘 Support

### Where can I get help?
- **Discord Community**: Real-time help and discussions
- **Documentation**: Check user guide and troubleshooting
- **Email Support**: kalponicGames@gmail.com
- **Bug Reports**: Include Unity version and error details

### What information should I include in bug reports?
- Unity version and render pipeline
- Character setup (components, animations)
- Exact error messages
- Steps to reproduce
- System specifications

### Feature requests?
We love hearing from users! Share ideas on Discord or via email.

### Beta access?
Join our Discord for early access to new features and beta testing.

---

## 🔧 Advanced Questions

### Can I modify the capture lighting?
The tool sets up professional lighting automatically. For custom lighting, you can modify the capture scene during setup.

### Does it work with custom shaders?
Most Unity shaders work. Some complex shaders may need adjustments for optimal capture.

### Can I capture UI elements or 2D objects?
Designed for 3D characters. For 2D to 2D conversion, consider other tools.

### Integration with other asset pipelines?
Works with any 3D character workflow. Can be integrated into automated build pipelines.

### API access for developers?
API access planned for future versions. Contact us for enterprise integration needs.

---

*This FAQ is updated regularly. For the latest information, check our [Discord community](https://discord.gg/7dbN6R4) or [documentation](USER_GUIDE.md).*

*Can't find what you're looking for? Contact kalponicGames@gmail.com*