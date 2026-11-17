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
while (dialogueManager.DialogueState != State.END) //Test code. Later probably simplify use of the dialogue manager.
{
    if (dialogueManager.DialogueState != State.LAST)
    {
        choice = 0;
        
        Console.WriteLine($"Speaker: {dialogueManager.CurrentLine.Speaker}\nLine: {dialogueManager.CurrentLine.Text}\n");
        if (dialogueManager.Choices.Count > 0)
        {
            foreach (var element in dialogueManager.Choices)
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
    if (dialogueManager.Choices.Count == 0 || dialogueManager.CurrentChoice != null)
        dialogueManager.Advance(); // Change, still runs if choice
    
}
saveManager.WriteSave();
