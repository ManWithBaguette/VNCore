# API Reference

This page provides an overview of VNCore's public API, including its core classes, enumerations, and data structures.

---

VNCore is organized around three primary responsibilities:
- **Compilation:** Converting VNCore txt scripts into structured JSON scene data.
- **Dialogue Management:** Loading scene data and managing runtime dialogue progression.
- **Save Management:** Storing, modifying, evaluating, and persisting runtime variables.

## Core Components
| Component | Description | Documentation |
|---|---|---|
| ScriptCompiler | Compiles VNCore scripts into JSON scene data. | [Script Compiler](docs/api/script-compiler.md) |
| DialogueManager | Manages dialogue progression, branching, and player choices. | [Dialogue Manager](docs/api/dialogue-manager.md) |
| SaveData | Stores and manages runtime variables | [Save Data](docs/api/save-data.md) |
| SaveManager | Handles JSON serialization and deserialization of save data. | [Save Manager](docs/api/save-manager.md) |

## API Documentation
### Script Compilation
- [ScriptCompiler](docs/api/script-compiler.md) - Compilation process and compiler functionality.
- [Compile Results](docs/api/compile-results.md) - Compilation results and error information.
### Dialogue Management
- [DialogueManager](docs/api/dialogue-manager.md) - Dialogue state and progression
- [Dialogue Data](docs/api/dialogue-data.md) - Dialogue lines, elements, and related data structures.
- [Dialogue Results](docs/api/dialogue-results.md) - Results returned by dialogue operations.
- [Dialogue States and Errors](docs/api/dialogue-states-and-errors.md) - Available dialogue states and error types.
### Save System
- [SaveData](docs/api/save-data.md) - Runtime variable storage, updates, and comparisons.
- [SaveManager](docs/api/save-manager.md) - Save file serialization and loading.
