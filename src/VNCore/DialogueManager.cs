using System.Text.Json;
using System.Text.RegularExpressions;
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
    IllegalState
}

public class DialogueManager
{
    public SaveData? saveData;
    private Dictionary<int, Dictionary<int, DialogueLine>> scene = new();
    private DialogueLine currentLine = new();
    public DialogueLine CurrentLine => currentLine;
    private int currentBranch = 0;
    private int currentLineNum = 0;

    public State DialogueState { get; private set; }

    private Dictionary<int, DialogueElement> choices = new();

    public IReadOnlyDictionary<int, DialogueElement> Choices => choices;
    
    private DialogueElement? currentChoice;

    public DialogueElement? CurrentChoice => currentChoice;

    public event Action<DialogueLine>? OnDialogueChanged;
    public event Action<IReadOnlyDictionary<int, DialogueElement>>? OnChoicesUpdated;
    public event Action? OnDialogueEnded;

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

            if (scene == null)
            {
                throw new DialogueException(DialogueError.InvalidScene, "Scene data is empty");
            }

            ValidateScene(loadedScene);

            if (!loadedScene.TryGetValue(0, out var firstBranch) || firstBranch == null || !firstBranch.ContainsKey(0))
            {
                throw new DialogueException(DialogueError.InvalidScene, "Scene must contain branch 0 line 0");
            }

        }
        catch (DialogueException ex)
        {
            return HandleException(ex);
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



        scene = loadedScene;

        currentBranch = 0;
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
            return new DialogueResult(false, $"Choice {num} is unavailable");
        }
        currentChoice = choice;
        return new DialogueResult(true);
    }

    public DialogueResult Advance()
    {
        try {
            switch (DialogueState)
            {
                case State.DIALOGUE:
                    return AdvanceDialogue();

                case State.CHOICE:
                    return AdvanceChoice();

                case State.LAST:
                    EndDialogue();
                    return new DialogueResult(true);

                case State.END:
                    return new DialogueResult(true);

                default:
                    throw new DialogueException(DialogueError.IllegalState, "Current Dialogue Manager state is illegal");
            }
        }
        catch (DialogueException ex)
        {
            return HandleException(ex);
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
            OnDialogueEnded?.Invoke();
        }
    }

    private DialogueResult Switch(string destination)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new DialogueException(DialogueError.InvalidDestination, "Switch destination is empty", currentBranch, currentLineNum);
        }

        string[] parts = destination.Split(':');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int branch) || !int.TryParse(parts[1], out int line))
        {
            throw new DialogueException(DialogueError.InvalidDestination, $"Invalid switch destination: {destination}", currentBranch, currentLineNum);
        }

        if (!scene.TryGetValue(branch, out var branchData))
        {
            throw new DialogueException(DialogueError.InvalidDestination, $"Branch {branch} does not exist", currentBranch, currentLineNum);
        }

        if (!branchData.ContainsKey(line))
        {
            throw new DialogueException(DialogueError.InvalidDestination, $"Line {line} does not exist in branch {branch}", currentBranch, currentLineNum);
        }

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

            if (line.Text.StartsWith("$\""))
            {
                line.Text = ReplaceFString(line.Text);
            }

            OnDialogueChanged?.Invoke(currentLine);

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
            if (DialogueState == State.CHOICE)
            {
                OnChoicesUpdated?.Invoke(Choices);
            }
            return new DialogueResult(true);
        }
    }

    private void UpdateChoices()
    {
        choices.Clear();
        currentChoice = null;
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
                if (choices[choiceNum].Text.StartsWith("$\""))
                {
                    choices[choiceNum].Text = ReplaceFString(choices[choiceNum].Text);
                }
            }
        }
        
    }

    private string ReplaceFString(string fString)
    {
        if (saveData == null)
        {
            return fString;
        }
        string text = fString[2..];

        return Regex.Replace(text, @"\{([^{}]+)\}", match =>
        {
            string variable = match.Groups[1].Value;
            if (saveData.Data.TryGetValue(variable, out var value))
                return value.ToString();

            return match.Value;
        });
    }

    public DialogueResult ValidateScene(Dictionary<int, Dictionary<int, DialogueLine>> scene)
    {

        try
        {
            if (scene == null)
            {
                throw new DialogueException(DialogueError.InvalidScene, "Scene data is empty");
            }
            
            if (!scene.ContainsKey(0))
            {
                throw new DialogueException(DialogueError.InvalidScene, "Missing initial branch 0");
            }
            foreach (var branch in scene)
            {
                if (branch.Value == null)
                {
                    throw new DialogueException(DialogueError.InvalidScene, $"Branch {branch.Key} is null", branch.Key);
                }

                foreach (var line in branch.Value)
                {
                    if (line.Value == null)
                    {
                        throw new DialogueException(DialogueError.InvalidScene, $"Line {line.Key} in branch {branch.Key} is null", branch.Key, line.Key);
                    }
                }
            }
            return new DialogueResult(true);
        }
        catch(DialogueException ex)
        {
            return HandleException(ex);
        }
    }

    private class DialogueException : Exception
    {
        public DialogueError Error { get; }
        public int CurrentBranch { get ; }
        public int CurrentLine { get ; }
        public DialogueException(DialogueError error, string message, int currentBranch = -1, int currentLine = -1) : base(message)
        {
            Error = error;
            CurrentBranch = currentBranch;
            CurrentLine = currentLine;
        }
    }

    private DialogueResult HandleException(DialogueException ex)
    {
        return new DialogueResult(false, ex.Message, ex.Error);
    }
}