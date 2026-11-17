using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using VNCore.Dialogue;

namespace VNCore.DialogueManager;

public enum State
{
    DIALOGUE,
    CHOICE,
    LAST,
    END
}

public class DialogueManager
{
    public SaveData? saveData;
    private Dictionary<int, Dictionary<int, DialogueLine>> scene = new();
    private DialogueLine currentLine = new();
    private int currentBranch = 1;
    private int currentLineNum = 0;

    public State dialogueState;

    public Dictionary<int, DialogueElement> choices = new();
    
    public DialogueElement? currentChoice;

    private List<DialogueLine> dialogueTree = new();

    private bool skippingLine = false;
    
    public DialogueManager() {}

    public DialogueManager(SaveData saveData)
    {
        this.saveData = saveData;
    }

    public void LoadScene(string path)
    {
        string jsonContent = File.ReadAllText(path);
        scene = JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, DialogueLine>>>(jsonContent);
        currentLine = scene[1][0];
        UpdateChoices();
        if (choices.Count() > 0)
        {
            dialogueState = State.CHOICE;
        }
    }
    public DialogueLine GetCurrentLine()
    {
        return currentLine;
    }

    public void SelectChoice(int num) // Assume choice selection in range. <- modify to protect against this actually
    {
        if (! choices.ContainsKey(num))
        {
            Console.WriteLine("Invalid Choice Selection");
            currentChoice = null;
            return;
        }
        currentChoice = choices[num];
        choices = new();
    }

    public void Advance()
    {
        if (dialogueState == State.LAST)
        {
            dialogueState = State.END;
            return;
        }
        UpdateChoices();

        if (choices.Count > 0)
        {
            dialogueState = State.CHOICE;
        } else
        {
            dialogueState  = State.DIALOGUE;
        }

        if (dialogueState == State.CHOICE)
        {
            Console.WriteLine("Currently in choice selection.");

            if (currentChoice != null)
            {
                if (saveData != null)
                    saveData.Update(currentChoice.Metadata);

                if (currentChoice.VNCoreData.TryGetValue("switch", out string? target))
                {
                    currentChoice = null;
                    Switch(target);
                    return;
                } else
                {
                    Console.WriteLine("Choice with no switch selected.");
                    currentChoice = null;
                    dialogueState = State.DIALOGUE;
                }
            }
        } else if (dialogueState == State.DIALOGUE)
        {
            if (saveData != null)
                saveData.Update(currentLine.Metadata);


            if (currentLine.VNCoreData.TryGetValue("switch", out string? destination))
            {
                Switch(destination);
                return;
            } else
            {
                Console.WriteLine("No switch");
            }
        }
        
        currentLineNum++;
        UpdateLine();
        
    }

    public void Switch(string destination)
    {
        Console.WriteLine($"Destination: {destination}");
        int[] switchDestination = destination.Split(':').Select(int.Parse).ToArray();
        Console.WriteLine("This Line contains switch");
        Console.WriteLine($"Branch: {switchDestination[0]}, Line: {switchDestination[1]}");
        currentBranch = switchDestination[0];
        currentLineNum = switchDestination[1];
        UpdateLine();
        UpdateChoices();
    }

    private void UpdateLine()
    {
        if (!scene.ContainsKey(currentBranch))
        {
            Console.WriteLine("Branch change invalid");
            return;
        }
        if (!scene[currentBranch].ContainsKey(currentLineNum))
        {
            Console.WriteLine("End of Branch reached.");
            dialogueState = State.LAST;
            return;
        }
        currentLine = scene[currentBranch][currentLineNum];
        if (saveData != null && currentLine.VNCoreData.TryGetValue("precondition", out var condition))
        {
            if (!saveData.Compare(condition))
            {
                currentLineNum++;
                UpdateLine();
                return;
            }
        }
        UpdateChoices();
    }

    private void UpdateChoices()
    {
        choices.Clear();
        int choiceNum = 0;

        foreach (var choice in currentLine.Choices)
        {
            bool available = true;

            if (choice.Value.VNCoreData.TryGetValue("precondition", out string? parameter))
            {
                Console.WriteLine("Precondition Detected");
                available = saveData != null && saveData.Compare(parameter);
            }

            if (available)
            {
                choiceNum++;
                choices[choiceNum] = choice.Value;
            }
        }
        
    }
}