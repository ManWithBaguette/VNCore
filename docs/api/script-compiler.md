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
