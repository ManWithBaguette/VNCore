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
    public DialogueLine CurrentLine => currentLine;
    private int currentBranch = 1;
    private int currentLineNum = 0;

    public State DialogueState { get; private set; }

    private Dictionary<int, DialogueElement> choices = new();

    public IReadOnlyDictionary<int, DialogueElement> Choices => choices;
    
    private DialogueElement? currentChoice;

    public DialogueElement? CurrentChoice => currentChoice;

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
            DialogueState = State.CHOICE;
        }
    }

    public void SelectChoice(int num) // Assume choice selection in range. <- modify to protect against this actually
    {
        if (DialogueState != State.CHOICE)
        {
            return;
        }

        if (!choices.ContainsKey(num))
        {
            Console.WriteLine("Invalid Choice Selection");
            currentChoice = null;
            return;
        }
        currentChoice = choices[num];
    }

    public void Advance()
    {
        switch (DialogueState)
        {
            case State.DIALOGUE:
                AdvanceDialogue();
                break;

            case State.CHOICE:
                AdvanceChoice();
                break;

            case State.LAST:
                EndDialogue();
                break;

            case State.END:
                break;
        }
    }

    private void MoveToNextLine()
    {
        currentLineNum++;
        UpdateLine();
    }

    private void AdvanceDialogue()
    {
        if (saveData != null)
        {
            saveData.Update(currentLine.Metadata);
        }

        if (currentLine.VNCoreData.TryGetValue("switch", out string? destination))
        {
            Switch(destination);
            return;
        }

        MoveToNextLine();
    }

    private void AdvanceChoice()
    {
        if (currentChoice == null)
        {
            return;
        }

        DialogueElement selectedChoice = currentChoice;
        currentChoice = null;

        if (saveData != null)
        {
            saveData.Update(selectedChoice.Metadata);
        }
        if (selectedChoice.VNCoreData.TryGetValue("switch", out string? destination))
        {
            Console.WriteLine("Switch on Choice");
            Switch(destination);
            return;
        }
        MoveToNextLine();
    }

    private void EndDialogue()
    {
        if (DialogueState == State.LAST)
        {
            DialogueState = State.END;
        }
    }

    private void Switch(string destination)
    {
        Console.WriteLine($"Destination: {destination}");
        int[] switchDestination = destination.Split(':').Select(int.Parse).ToArray();
        Console.WriteLine("This Line contains switch");
        Console.WriteLine($"Branch: {switchDestination[0]}, Line: {switchDestination[1]}");
        currentBranch = switchDestination[0];
        currentLineNum = switchDestination[1];
        UpdateLine();
    }

    private void UpdateLine()
    {
        while (true)
        {
            if (!scene.ContainsKey(currentBranch))
            {
                Console.WriteLine("Branch change invalid");
                return;
            }
            if (!scene[currentBranch].ContainsKey(currentLineNum))
            {
                Console.WriteLine("End of Branch reached.");
                DialogueState = State.LAST;
                return;
            }

            currentLine = scene[currentBranch][currentLineNum];

            if (saveData != null && currentLine.VNCoreData.TryGetValue("precondition", out var condition))
            {
                if (!saveData.Compare(condition))
                {
                    currentLineNum++;
                    continue;
                }
            }
            UpdateChoices();
            DialogueState = choices.Count > 0 ? State.CHOICE : State.DIALOGUE;
            return;
        }
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