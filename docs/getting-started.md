## Usage

VNCore is intended to be used as a library within other C# projects. The primary way to use VNCore is by importing its compiled DLL into your project and interacting with its public API.

### Using VNCore:

1. Obtain the compiled VNCore DLL.
2. Add its reference to your C# project.
3. Import the required VNCore namespaces.
4. Create a dialogue script .txt following the scripting language guidelines.
5. Create a ScriptCompiler object and compile the dialogue script txt into a scene JSON.
6. Create a DialogueManager object and load in the scene JSON.
7. Use DialogueManager APIs to manage dialogue progression during runtime.

Related Documentation:
[scripting language](scripting-language.md)
[ScriptCompiler](api/script-compiler.md)
[DialogueManager](api/dialogue-manager.md)
[Save System](save-system.md)

### Working with the repository:

Cloning this repository is only necessary if you intend to:

- Build the VNCore DLL yourself.
- Modify or extend the library's source code.
- Examine the internal implementation.