# VN-Core

VN-Core is a dialogue management and scripting library designed for visual novel development. It provides a custom dialogue scripting format, a compiler that converts scripts into JSON scene data, and a dialogue manager that handles dialogue progression, branching, player choices, and conditional logic.

The library also includes a save data system that allows scripts to store, modify, and evaluate variables during runtime.

## Features

    Custom Scripting Language: Write dialogue and scene instructions in a human-readable format.

    Script Compiler: Converts dialogue scripts into structured JSON files.

    Dialogue Management: Handles dialogue progression, scene loading, and unloading.

    Branching Dialogue: Supports switching between dialogue branches and external script files.

    Player Choices: Supports multiple choices, each with its own metadata and conditions.

    Conditional Dialogue: Allows dialogue and choices to be conditionally displayed based on saved variables.

    Dynamic Text: Supports variable interpolation within dialogue text.

    Save Data: Stores and modifies runtime variables, including arithmetic operations and comparisons.

    Event Notifications: Provides events for scene loading, dialogue changes, choice updates, and dialogue completion.

## Usage


## Architecture

  VN-Core is primarily divided into four components:

### 1. ScriptCompiler

The ScriptCompiler converts human-readable dialogue scripts into JSON scene data.

It processes script files line by line, interpreting indentation and keywords to distinguish between dialogue, metadata, and choices.

The compiler also supports references to other script files, allowing multiple files to be compiled into separate dialogue branches within the same output.

### 2. DialogueManager
### 3. SaveData
### 4. SaveManager

## Scripting Language

## Getting Started

## JSON Scene Structure

## Dependencies

