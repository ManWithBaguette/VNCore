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

public enum DialogueError
{
    None,
    FileNotFound,
    InvalidScene,
    InvalidDestination,
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

    public record DialogueResult(
        bool Success,
        string? ErrorMessage = null,
        DialogueError? Error = DialogueError.None
    );

    public DialogueManager() {}

    public DialogueManager(SaveData saveData)
    {
        this.saveData = saveData;
    }

    public DialogueResult LoadScene(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return new DialogueResult(false, "Scene path is empty", DialogueError.FileNotFound);
        }

        Dictionary<int, Dictionary<int, DialogueLine>>? loadedScene;

        try
        {
            string jsonContent = File.ReadAllText(path);
            loadedScene = JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, DialogueLine>>>(jsonContent);
        }
        catch (IOException ex)
        {
            return new DialogueResult(false, $"File error: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            return new DialogueResult(false, $"Access error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return new DialogueResult(false, $"Invalid JSON: {ex.Message}");
        }
        
        if (loadedScene == null)
        {
            return new DialogueResult(false, "Scene data is empty", DialogueError.InvalidScene);
        }

        if (!loadedScene.TryGetValue(1, out var firstBranch) || firstBranch == null || !firstBranch.ContainsKey(0))
        {
            return new DialogueResult(false, "Scene must contain branch 1 line 0", DialogueError.InvalidScene);
        }

        scene = loadedScene;

        currentBranch = 1;
        currentLineNum = 0;
        currentChoice = null;
        choices.Clear();

        UpdateLine();
        return new DialogueResult(true);
    }

    public DialogueResult SelectChoice(int num)
    {
        if (DialogueState != State.CHOICE)
        {
            return new DialogueResult(false, "Cannot select choice outside CHOICE state");
        }

        if (!choices.TryGetValue(num, out var choice))
        {
            Console.WriteLine("Invalid Choice Selection");
            currentChoice = null;
            return new DialogueResult(false, "Choice {num} is unavailable");
        }
        currentChoice = choice;
        return new DialogueResult(true);
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

    private DialogueResult MoveToNextLine()
    {
        currentLineNum++;
        return UpdateLine();
    }

    private DialogueResult AdvanceDialogue()
    {
        if (saveData != null)
        {
            saveData.Update(currentLine.Metadata);
        }

        if (currentLine.VNCoreData.TryGetValue("switch", out string? destination))
        {
            return Switch(destination);
        }

        return MoveToNextLine();
    }

    private DialogueResult AdvanceChoice()
    {
        if (currentChoice == null)
        {
            return new DialogueResult(false, "Cannot advance: no choice has been selected");
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
            return Switch(destination);
        }
        return MoveToNextLine();
    }

    private void EndDialogue()
    {
        if (DialogueState == State.LAST)
        {
            DialogueState = State.END;
        }
    }

    private DialogueResult Switch(string destination)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            return new DialogueResult(false, "Switch destination is empty", DialogueError.InvalidDestination);
        }

        string[] parts = destination.Split(':');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int branch) || !int.TryParse(parts[1], out int line))
        {
            return new DialogueResult(false, $"Invalid switch destination: {destination}", DialogueError.InvalidDestination);
        }

        if (!scene.TryGetValue(branch, out var branchData))
        {
            return new DialogueResult(false, $"Branch {branch} does not exist", DialogueError.InvalidDestination);
        }

        if (!branchData.ContainsKey(line))
        {
            return new DialogueResult(false, $"Line {line} does not exist in branch {branch}", DialogueError.InvalidDestination);
        }

        Console.WriteLine($"Switch Destination: {destination}");
        Console.WriteLine($"Branch: {branch}, Line: {line}");
        currentBranch = branch;
        currentLineNum = line;
        return UpdateLine();
    }

    private DialogueResult UpdateLine()
    {
        while (true)
        {
            if (!scene.TryGetValue(currentBranch, out var branch))
            {
                Console.WriteLine("Branch change invalid");
                return new DialogueResult(false, $"Branch {currentBranch} does not exist");
            }
            if (!branch.TryGetValue(currentLineNum, out var line))
            {
                Console.WriteLine("End of Branch reached.");
                currentChoice = null;
                choices.Clear();
                DialogueState = State.LAST;
                return new DialogueResult(true);
            }

            if (line == null)
            {
                return new DialogueResult(false, $"Line {currentLineNum} in branch {currentBranch} is null");
            }

            currentLine = line;

            if (currentLine.VNCoreData.TryGetValue("precondition", out var condition))
            {
                if (saveData == null || (saveData != null && !saveData.Compare(condition)))
                {
                    currentLineNum++;
                    continue;
                } 
            }

            UpdateChoices();
            DialogueState = choices.Count > 0 ? State.CHOICE : State.DIALOGUE;
            return new DialogueResult(true);
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