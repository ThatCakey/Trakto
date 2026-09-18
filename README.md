# Trakto

Trakto is a cross-platform video editing application built in C# using .NET 10 and Avalonia UI. It relies on a custom video processing engine, Visive, to handle media manipulation, composition, and timeline rendering.

## Architecture

The application is structured around a Model-View-ViewModel (MVVM) architecture leveraging `CommunityToolkit.Mvvm`. 

### Layout Engine
The UI utilizes a custom Binary Space Partitioning (BSP) window manager implemented in `LayoutEngine.cs`. 
- **Tree Structure:** The workspace is represented by a binary tree. `SplitNode` represents horizontal/vertical subdivisions, and `LeafNode` contains the actual panel content (tabs).
- **Dynamic Views:** Panels dynamically inject UserControls (`TimelineView`, `PreviewView`, etc.) based on a `ViewType` string resolution system.
- **Persistence:** Layout trees are serialized to JSON using `System.Text.Json` polymorphic serialization. Parent nodes and dynamic views are automatically re-wired on load.

### Video Backend (Visive)
The `Visive` project is a dedicated C# backend engine that drives video processing.
- **VideoObject:** The core composition unit. Handles timeline slicing, clip appending, overlay stacking, and effect chaining.
- **FrameObject:** Stores pixel data in a custom struct (10-bit RGB, 8-bit Alpha). Features a high-performance `WriteToBuffer` method that down-shifts memory into a flat 8-bit RGBA byte array.
- **Rendering Bridge:** Trakto utilizes `Marshal.Copy` to directly pass Visive's flat 8-bit RGBA byte array into Avalonia's `WriteableBitmap` backbuffer memory, achieving fast, I/O-free frame rendering.
- **FFmpeg Integration:** Relies on FFmpeg sub-processes for initial decoding, extraction, and final media export.

## Development

### Prerequisites
- .NET 10 SDK
- FFmpeg (must be available in the system PATH)

### Project Structure
- `Trakto/`
  - `ViewModels/` - MVVM view models, layout state, and session management.
  - `Views/` - Avalonia UI XAML definitions and code-behind.
  - `Models/` - Data models.
- `Trakto/Visive/`
  - The Visive processing library. (Note: Visive `.cs` files are excluded from the main Trakto build context via `Trakto.csproj` to prevent compilation collision with the `ProjectReference`).

### Build & Run
```bash
dotnet build
dotnet run
```
