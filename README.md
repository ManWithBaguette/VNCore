# VNCore

VNCore is a dialogue management and scripting library designed for visual novel development. It provides a custom dialogue scripting format, a compiler that converts scripts into JSON scene data, and a dialogue manager that handles dialogue progression, branching, player choices, and conditional logic.

The library also includes a save data system that allows scripts to store, modify, and evaluate variables during runtime.

## Features
```
Custom Scripting Language: Write dialogue and scene instructions in a human-readable format.

Script Compiler: Converts dialogue scripts into structured JSON files.

Dialogue Management: Handles dialogue progression, scene loading, and unloading.

Branching Dialogue: Supports switching between dialogue branches and external script files.

Player Choices: Supports multiple choices, each with its own metadata and conditions.

Conditional Dialogue: Allows dialogue and choices to be conditionally displayed based on saved variables.

Dynamic Text: Supports variable interpolation within dialogue text.

Save Data: Stores and modifies runtime variables, including arithmetic operations and comparisons.

Event Notifications: Provides events for scene loading, dialogue changes, choice updates, and dialogue completion.
```

## Installation & Packages

| Package | Platform | Distribution |
|:-------:|:--------:|:------------:|
| VNCore  | Standard C# Projects | NuGet |
| VNCore.Godot | Godot (C#) | NuGet |
| VNCore.Unity | Unity | Unity Package Manager |
---

### Standard C# Projects
Install VNCore through NuGet:
```
dotnet add package VNCore
```
### Godot
Install VNCore.Godot through NuGet:
```
dotnet add package VNCore.Godot
```
### Unity
Install VNCore.Unity through the Unity Package Manager.

## Documentation

- [Getting Started](docs/getting-started.md)
- [Scripting Language](docs/scripting-language.md)
- [API Reference](docs/api-reference.md)
- [Save System](docs/save-system.md)
- [Architecture](docs/architecture.md)
- [Unity Integration](docs/integrations/unity.md)
- [Godot Integration](docs/integrations/godot.md)