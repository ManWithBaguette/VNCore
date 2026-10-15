using VNCore;
using VNCore.Compiler;
using VNCore.Dialogue;
using VNCore.DialogueManager;

// var compiler = new ScriptCompiler();

// compiler.Compile("test-data/test.txt");

Console.WriteLine("Compilation complete!");
SaveData save = new();
int choice = 0;
var dialogueManager = new DialogueManager(save);

// dialogueManager.readScript("output.json");
dialogueManager.LoadScene("output.json");
while (dialogueManager.dialogueState != State.END) //Test code. Later probably simplify use of the dialogue manager.
{
    if (choice != 0)
    {
        dialogueManager.SelectChoice(choice);
    }
    else
    {
        dialogueManager.Advance();
    }
    choice = 0;
    DialogueLine line = dialogueManager.GetCurrentLine();
    
    Console.WriteLine($"Speaker: {line.Speaker}\nLine: {line.Text}\n");
    if (int.TryParse(Console.ReadLine(), out int value))
    {
        choice = value;
    }
}