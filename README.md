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
## Usage

VNCore is intended to be used as a library within other C# projects. The primary way to use VNCore is by importing its compiled DLL into your project and interacting with its public API.

### Using VNCore:

1. Obtain the compiled VNCore DLL.
2. Add its reference to your C# project.
3. Import the required VNCore namespaces.
4. Use the provided classes to compile scripts, manage dialogue, and handle save data.

### Working with the repository:

Cloning this repository is only necessary if you intend to:

- Build the VNCore DLL yourself.
- Modify or extend the library's source code.
- Examine the internal implementation.

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
## Architecture

  VNCore is primarily divided into four components:

### 1. ScriptCompiler

Namespace: VNCore.Compiler

The ScriptCompiler converts human-readable dialogue scripts into JSON scene data.

It processes script files line by line, interpreting indentation and keywords to distinguish between dialogue, metadata, and choices.

The compiler also supports references to other script files, allowing multiple files to be compiled into separate dialogue branches within the same output.

#### Public API:
```
public CompileResult Compile(string filepath, string outputPath); 
```
| Parameter | Description |
|---|---|
| filepath | Path to the source dialogue .txt script. |
| outputPath | Destination for the compiled JSON scene file. |

The method returns a CompileResult containing: 

- Success: Whether compilation completed successfully.
- ErrorMessage: An optional description of the error.
- Error: The assosciated CompileError value.

Usage Example:
```
ScriptCompiler compiler = new ScriptCompiler();

ScriptCompiler.CompileResult result =
  compiler.Compile("Scripts/Chapter1.txt", "Scenes/Chapter1.json");

if (!result.Success)
  Console.WriteLine(result.ErrorMessage);
```

### 2. DialogueManager
### 3. SaveData
### 4. SaveManager

## Scripting Language

## Getting Started

## JSON Scene Structure

## Dependencies

