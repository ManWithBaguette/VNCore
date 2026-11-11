using VNCore;
using VNCore.Compiler;
using VNCore.Dialogue;
using VNCore.DialogueManager;

// var compiler = new ScriptCompiler();

// compiler.Compile("test-data/test.txt", "scene1");

Console.WriteLine("Compilation complete!");
int choice = 0;
var saveManager = new SaveManager();
saveManager.LoadSave("");
var dialogueManager = new DialogueManager(saveManager.Current);
// var dialogueManager = new DialogueManager();
dialogueManager.LoadScene("output.json");
while (dialogueManager.dialogueState != State.END) //Test code. Later probably simplify use of the dialogue manager.
{
    if (dialogueManager.dialogueState != State.LAST)
    {
        choice = 0;
        DialogueLine line = dialogueManager.GetCurrentLine();
        
        Console.WriteLine($"Speaker: {line.Speaker}\nLine: {line.Text}\n");
        if (dialogueManager.choices.Count > 0)
        {
            foreach (var element in dialogueManager.choices)
            {
                Console.WriteLine($"Choice {element.Key} Text: {element.Value.Text}");
            }
        }
    }
    if (int.TryParse(Console.ReadLine(), out int value))
    {
        choice = value;
    }

    if (choice != 0)
    {
        dialogueManager.SelectChoice(choice);
    }
    if (dialogueManager.choices.Count == 0 || dialogueManager.currentChoice != null)
        dialogueManager.Advance(); // Change, still runs if choice
    
}
saveManager.WriteSave();
