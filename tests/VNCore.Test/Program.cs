using VNCore.Compiler;

var compiler = new ScriptCompiler();

compiler.Compile("test-data/test.txt");

Console.WriteLine("Compilation complete!");