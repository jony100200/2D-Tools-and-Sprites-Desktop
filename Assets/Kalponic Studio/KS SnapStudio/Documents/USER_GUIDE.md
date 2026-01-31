# KS SnapStudio User Guide

## 📖 Table of Contents

- [Quick Start](#-quick-start)
- [Interface Overview](#-interface-overview)
- [Animation Capture Setup](#-animation-capture-setup)
- [Capture Settings Explained](#-capture-settings-explained)
- [Advanced Configuration](#-advanced-configuration)
- [Troubleshooting](#-troubleshooting)
- [Best Practices](#-best-practices)
- [FAQ](#-faq)

---

## 🚀 Quick Start

### Step 1: Installation
1. Download KS SnapStudio from itch.io
2. Import the package into your Unity project
3. Restart Unity if prompted

### Step 2: Open the Tool
- **Menu**: Tools → Kalponic Studio → Animation → KS SnapStudio
- **Shortcut**: The window will open with three tabs
- **Navigation**: Use mouse clicks or Left/Right arrow keys to switch tabs
- **Persistence**: Your last selected tab will be remembered

### Step 3: Basic Capture
1. **Select Character**: Drag your 3D model into the "Target" field
2. **Choose Animation**: Select an Animation Clip or leave blank for all animations
3. **Set Output**: Choose where to save your sprites
4. **Configure**: Adjust resolution, FPS, and other settings
5. **Capture**: Click "Start Capture" and enter Play Mode

### Step 4: Results
- Find your sprites in the output folder
- Organized by animation name and frame number
- Ready to use in your 2D game!

---

## 🎛️ Interface Overview

### Main Window Features

#### 🎨 Modern UI Design
- **Unity UI Toolkit**: Built with Unity's modern UI framework for optimal performance
- **Professional Styling**: Clean, dark theme optimized for long editing sessions
- **Responsive Layout**: Adapts to different window sizes and screen resolutions

#### ⌨️ Keyboard Navigation
- **Tab Switching**: Use `Left Arrow` and `Right Arrow` keys to navigate between tabs
- **Focus Management**: Click any tab or press arrow keys to give the window focus
- **Accessibility**: Full keyboard accessibility with visual focus indicators

#### 💾 State Persistence
- **Tab Memory**: The last selected tab is automatically restored when reopening the window
- **Settings Memory**: All configuration options are saved and restored across sessions
- **Domain Reload**: Survives Unity's domain reloads without losing your place

### Main Tabs

#### 🎬 Animation Capture Tab
The core functionality for converting 3D animations to 2D sprites.

**Key Features:**
- High-performance capture engine
- Support for Animator Controllers and legacy Animation components
- Real-time preview and progress monitoring
- Automatic scene setup and cleanup

#### 🖼️ Thumbnail Generation Tab
Generate professional character thumbnails and marketing images with advanced customization options.

#### ⚙️ Settings Tab
Global preferences and configuration options.

**Configuration Areas:**
- General preferences (tooltips, auto-save)
- Default capture values (resolution, FPS)
- Output settings (paths, organization)
- Integration settings (ThumbSmith connection)
- Advanced options (debug logging, reset)

### Tab Navigation

#### Mouse Navigation
- **Click**: Click any tab button to switch immediately
- **Visual Feedback**: Active tab shows blue highlight (`#2196F3`)
- **Smooth Transitions**: Instant content switching with no delays

#### Keyboard Navigation
- **Left Arrow**: Move to previous tab
- **Right Arrow**: Move to next tab
- **Wraparound**: Navigation wraps from last to first tab
- **Focus Required**: Window must have focus for keyboard navigation

#### Tab States
- **Active Tab**: Blue background, white text, content visible
- **Inactive Tabs**: Dark background, gray text
- **Always Enabled**: Tabs remain clickable even if content has errors
- **Lazy Loading**: Tab content loads only when first accessed

### Key UI Elements

- **Tab Bar**: Three main tabs at the top with blue active highlight
- **Content Area**: Main workspace that changes based on selected tab
- **Status Display**: Progress and error messages at the bottom
- **Validation**: Real-time checking with visual indicators
- **Persistence**: All settings automatically saved and restored

---

- [Tab System & Navigation](#-tab-system--navigation)

### Understanding the Tab Interface

KS SnapStudio uses a modern tabbed interface built with Unity UI Toolkit for optimal performance and user experience.

#### Tab Behavior
- **Always Available**: All tabs remain clickable and functional at all times
- **Content Isolation**: Each tab maintains its own settings and state
- **Error Resilience**: If a tab's content fails to load, the tab button stays active
- **Memory Efficient**: Tab content loads on-demand and stays cached

#### Visual Indicators
- **Active Tab**: Blue background (`#2196F3`) with white text
- **Inactive Tabs**: Dark gray background with gray text
- **Focus Ring**: Blue outline when tab has keyboard focus
- **Hover Effect**: Slight brightening on mouse hover

### Navigation Methods

#### Using the Mouse
1. **Click any tab** to switch immediately
2. **Visual feedback** shows instantly with blue highlight
3. **Content changes** without any loading delays

#### Using the Keyboard
1. **Click anywhere in the window** to give it focus
2. **Press Left/Right arrow keys** to navigate between tabs
3. **Navigation wraps around** (last tab → first tab)
4. **Focus indicator** shows which tab is selected

#### State Persistence
- **Automatic Saving**: Your selected tab is remembered across sessions
- **Domain Reload**: Survives Unity recompilation without losing position
- **Project Restart**: Returns to your last active tab when reopening

### Tab Content Areas

#### Animation Capture Tab
- **Primary workflow** for 3D to 2D conversion
- **Settings specific** to capture operations
- **Progress monitoring** during capture sessions

#### Thumbnail Generation Tab
- **Professional portraits** for character marketing and UI
- **Advanced backgrounds** with solid colors and gradients
- **Dynamic rotation** with customizable speed control
- **Custom lighting** with intensity, color, and directional settings
- **Batch processing** for multiple characters
- **Multiple formats** (PNG, JPG, TGA) with quality settings

#### Settings Tab
- **Global preferences** affecting all operations
- **Default values** for new projects
- **Integration settings** for connected tools
- **Advanced options** for debugging and customization

---

## 📋 Tab System & Navigation

### Character Requirements

Your character needs **either**:
- **Animator Component** with Animator Controller (recommended)
- **Legacy Animation Component** with Animation Clips

### Supported Character Types
- ✅ Humanoid characters with Mecanim
- ✅ Generic rigged characters
- ✅ Legacy animation setups
- ✅ Characters with multiple animation clips

### Scene Requirements
- No special scene setup needed
- Tool creates temporary capture scene automatically
- Original scene restored after capture

---

## ⚙️ Capture Settings Explained

### Basic Settings

#### 🎯 Target
- **What**: The GameObject to capture
- **Requirements**: Must have Animator or Animation component
- **Tip**: Drag from Hierarchy or Project window

#### 📁 Output Path
- **What**: Folder where sprites will be saved
- **Default**: `KS_SnapStudio_Renders/`
- **Tip**: Use "Browse" button for folder selection

#### 📐 Width & Height
- **What**: Resolution of output sprites
- **Range**: 64px to 4096px
- **Default**: 1024x1024
- **Tip**: Higher resolution = better quality but larger files

#### 🎞️ FPS (Frames Per Second)
- **What**: How many frames to capture per second of animation
- **Range**: 1-60 FPS
- **Default**: 24 FPS
- **Tip**: Match your game's target FPS

#### 🎬 Max Frames
- **What**: Maximum frames to capture per animation
- **Default**: 24 frames
- **Tip**: Prevents extremely long animations from creating too many files

### Advanced Settings

#### 🎨 Character Fill %
- **What**: How much of the frame the character should occupy
- **Range**: 10%-100%
- **Default**: 75%
- **When**: Only used when trimming is disabled
- **Tip**: Higher values make characters appear larger

#### ✂️ Trim Sprites
- **What**: Remove transparent borders around characters
- **Default**: ON
- **Benefits**: Smaller file sizes, better alignment
- **When to disable**: If you need consistent frame sizes

#### 🪞 Mirror Sprites
- **What**: Create left-facing versions of right-facing animations
- **Default**: ON
- **Benefits**: Doubles your animation library
- **Tip**: Great for platformer characters

#### 🌈 HDR Capture
- **What**: Capture with High Dynamic Range
- **Default**: OFF
- **When to use**: Characters with emissive materials or bright effects
- **Note**: May require HDR-enabled camera in your project

#### 📏 Pixel Size
- **What**: Size of pixels in the output (for pixel art style)
- **Range**: 1-16 pixels
- **Default**: 16
- **Tip**: Smaller values = finer detail, larger files

#### 🎭 Capture All Animations
- **What**: Capture all animations from the character
- **Default**: OFF (single animation mode)
- **When to use**: Batch processing entire character animation sets
- **Note**: Requires Animator Controller selection

---

## 🎮 Step-by-Step Capture Guide

### Method 1: Single Animation Capture

1. **Prepare Character**
   - Select character in Hierarchy
   - Ensure it has Animator/Animation component
   - Set up any animation clips

2. **Open KS SnapStudio**
   - Tools → Kalponic Studio → Animation → KS SnapStudio
   - The **Animation Capture** tab will be selected by default
   - Use tabs at the top to switch between features

3. **Configure Basic Settings**
   - Drag character to "Target" field
   - Set output path
   - Choose resolution (1024x1024 recommended)
   - Set FPS (24 recommended)

4. **Select Animation** (Optional)
   - If capturing specific animation, select from dropdown
   - Leave blank to capture default/selected animation

5. **Adjust Advanced Settings**
   - Enable/disable trimming based on needs
   - Set character fill percentage
   - Configure mirroring if needed

6. **Create Capture Scene**
   - Click "Create Scene" button
   - Tool sets up lighting and camera automatically

7. **Test Configuration**
   - Click "Test" to validate settings
   - Check status messages for any issues

8. **Start Capture**
   - Click "Start Capture"
   - Enter Play Mode when prompted
   - Monitor progress bar

9. **Review Results**
   - Exit Play Mode when complete
   - Check output folder for sprite sequences

### Method 2: Batch Animation Capture

1. **Setup Character**
   - Select character with Animator Controller
   - Ensure multiple animation clips are available

2. **Enable Batch Mode**
   - Check "Capture All Animations" toggle
   - Select Animator Controller in settings

3. **Configure Settings**
   - Same as single capture, but affects all animations

4. **Capture Process**
   - Follow steps 6-9 from single capture
   - Tool will process each animation sequentially

---

## 🔧 Advanced Configuration

### Animator Controller Setup

For best results with Mecanim:
1. Create Animator Controller
2. Add animation clips as states
3. Set default state
4. Assign controller to character's Animator

### Custom Lighting

The tool automatically sets up professional lighting, but you can modify:
- **Light Intensity**: Affects shadow and highlight strength
- **Light Color**: Changes overall tone
- **Ambient Light**: Fills shadows

### Camera Configuration

- **Automatic Positioning**: Tool calculates optimal distance
- **Field of View**: 30 degrees for minimal distortion
- **Orthographic Mode**: Ensures consistent scaling

---

## 🐛 Troubleshooting

### Common Issues

#### "No target selected"
- **Cause**: No GameObject in Target field
- **Solution**: Drag character from Hierarchy to Target field

#### "Target has no animations"
- **Cause**: Character missing Animator/Animation component
- **Solution**: Add Animator component and assign clips

#### "Output path not accessible"
- **Cause**: Permission issues or invalid path
- **Solution**: Choose different folder or check write permissions

#### "Capture failed: Not in Play Mode"
- **Cause**: Attempted capture while not in Play Mode
- **Solution**: Click "Start Capture" and enter Play Mode

#### "Memory error during capture"
- **Cause**: Large animations or high resolution
- **Solution**: Reduce resolution or increase Max Frames limit

### Performance Tips

- **Lower Resolution**: 512x512 for testing, 1024x1024 for final
- **Reduce FPS**: 12-15 FPS for simple animations
- **Enable Trimming**: Reduces file sizes significantly
- **Batch Processing**: More efficient than individual captures

---

## 💡 Best Practices

### File Organization
```
Project/
├── Assets/
│   ├── Characters/
│   │   ├── Hero/
│   │   │   ├── Hero.fbx
│   │   │   ├── Hero_Animations/
│   │   │   │   ├── Idle.anim
│   │   │   │   ├── Walk.anim
│   │   │   │   └── Run.anim
│   ├── Sprites/
│   │   ├── Hero_Idle_001.png
│   │   ├── Hero_Idle_002.png
│   │   ├── Hero_Walk_001.png
│   │   └── ...
```

### Animation Preparation
- **Clean Rigs**: Ensure character rigs are properly set up
- **Consistent Scale**: Characters should be properly scaled
- **Animation Quality**: Smooth animations capture better
- **Loop Points**: Set proper loop points for seamless animation

### Output Optimization
- **Trim Sprites**: Always enable for smaller files
- **Appropriate Resolution**: Match your game's pixel density
- **Consistent FPS**: Use same FPS across all character animations
- **Naming Convention**: Use descriptive animation names

---

## ❓ FAQ

### Getting Started
**Q: Do I need special Unity knowledge?**
A: Basic Unity skills are helpful, but the tool guides you through the process.

**Q: Can I capture any 3D model?**
A: Yes, as long as it has animation data (Animator or Animation component).

**Q: How long does capture take?**
A: Depends on animation length and settings. Typically 30 seconds to 5 minutes.

### Technical Questions
**Q: What Unity versions are supported?**
A: Unity 2021.3 and higher, including Unity 6.

**Q: Does it work with URP/HDRP?**
A: Yes, works with all render pipelines.

**Q: Can I modify the capture lighting?**
A: The tool sets up optimal lighting, but you can modify it in the capture scene.

### Output & Formats
**Q: What format are sprites saved in?**
A: PNG with transparency.

**Q: Can I change the file naming?**
A: Currently uses AnimationName_FrameNumber.png format.

**Q: Are sprites automatically imported into Unity?**
A: Yes, Unity automatically imports the generated PNG files.

### Pricing & Features
**Q: Is thumbnail generation included?**
A: Yes! Both animation capture and thumbnail generation are included in the $5 package.

**Q: Can I get a refund?**
A: No refunds - "What you see is what you get" policy for digital software purchases.

**Q: Is support included?**
A: Yes, via Discord community and email support.

---

## 📞 Need Help?

- **Discord Community**: [Join for real-time help](https://discord.gg/7dbN6R4)
- **Documentation**: Check this guide and the [FAQ](FAQ.md)
- **Email Support**: kalponicGames@gmail.com
- **Bug Reports**: Include Unity version, character setup, and error messages

---

*Last updated: October 2025*

*Thank you for choosing KS SnapStudio! We hope this guide helps you create amazing 2D animations from your 3D characters.*