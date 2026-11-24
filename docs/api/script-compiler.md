# ScriptCompiler

### Namespace: `VNCore.Compiler`

`ScriptCompiler` converts formatted txt dialogue scripts into structured JSON scene data.

The compiler processes script files line by line, interpreting indentation and keywords to distinguish between dialogue, metadata, and choices, and generates a JSON representation that can be loaded by DialogueManager.

It also supports references to additional script files, allowing multiple files to be compiled into separate dialogue branches within the same output scene JSON.

## Public API:
```
public CompileResult Compile(string filepath, string outputPath); 
```
Compiles a VNCore script txt and writes the resulting scene data to a JSON file.

| Parameter | Description |
|---|---|
| filepath | Path to the source dialogue .txt script. |
| outputPath | Destination path for the compiled JSON scene file. |

The .txt and .json extension is automatically appended to the output path if not already present.
Returns a CompileResult.
### CompileResult
Represents the outcome of a compilation operation.
```
public record CompileResult(
  bool Success,
  string? ErrorMessage = null,
  CompileError? Error = CompileError.None
);
```
| Property | Type | Description |
|---|---|---|
| Success | bool | Whether compilation succeeded |
| ErrorMessage | string? | Description of an error, if available |
| Error | CompileError? | Compilation error category |

Usage Example:
```
ScriptCompiler compiler = new ScriptCompiler();

ScriptCompiler.CompileResult result =
  compiler.Compile("Scripts/Chapter1.txt", "Scenes/Chapter1.json");

if (!result.Success)
  Console.WriteLine(result.ErrorMessage);
```
