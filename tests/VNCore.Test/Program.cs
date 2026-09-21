using VNCore.Compiler; using VNCore.DialogueManager;

var compiler = new ScriptCompiler();

compiler.Compile("test-data/test.txt");

Console.WriteLine("Compilation complete!");

var dialogueManager = new DialogueManager();

dialogueManager.readScript("output.json");