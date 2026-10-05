using VNCore;
using VNCore.Compiler; 
using VNCore.DialogueManager;

// var compiler = new ScriptCompiler();

// compiler.Compile("test-data/test.txt");

Console.WriteLine("Compilation complete!");
SaveData save = new();

var dialogueManager = new DialogueManager(save);

dialogueManager.readScript("output.json");