using System.Text.Json;
using System.Text.RegularExpressions;
using VNCore.Dialogue;

namespace VNCore.DialogueManager;

public enum State
{
    Unloaded,
    Dialogue,
    Choice,
    Last,
    End
}

public enum DialogueError
{
    None,
    FileNotFound,
    FileAccessDenied,
    FileReadError,
    InvalidJson,
    
    InvalidScene,
    InvalidDestination,
    
    IllegalState,
    InvalidChoice
}

public class DialogueManager
{
    public SaveData? saveData;
    private Dictionary<int, Dictionary<int, DialogueLine>> scene = new();
    private DialogueLine currentLine = new();
    public DialogueLine CurrentLine => currentLine;
    public int CurrentBranch { get; private set; } = 0;
    public int CurrentLineNum { get; private set; } = 0;

    public State DialogueState { get; private set; } = State.Unloaded;

    private Dictionary<int, DialogueElement> choices = new();

    public IReadOnlyDictionary<int, DialogueElement> Choices => choices;
    
    private DialogueElement? currentChoice;

    public DialogueElement? CurrentChoice => currentChoice;

    public event Action? OnSceneLoaded;
    public event Action? OnSceneUnloaded;
    public event Action<DialogueLine>? OnDialogueChanged;
    public event Action<IReadOnlyDictionary<int, DialogueElement>>? OnChoicesUpdated;
    public event Action? OnDialogueEnded;

    public record DialogueResult(
        bool Success,
        string? ErrorMessage = null,
        DialogueError Error = DialogueError.None,
        int? Branch = null,
        int? Line = null
    );

    public DialogueManager()
    {
        saveData = new SaveData();
    }

    public DialogueManager(SaveData saveData)
    {
        this.saveData = saveData;
    }

    public DialogueResult LoadScene(string path)
    {
        UnloadScene();

        if (string.IsNullOrWhiteSpace(path))
        {
            return new DialogueResult(false, "Scene path is empty", DialogueError.FileNotFound);
        }

        Dictionary<int, Dictionary<int, DialogueLine>>? loadedScene;

        try
        {
            if (!path.ToLower().EndsWith(".json"))
            {
                path = path += ".json";
            }
            string jsonContent = File.ReadAllText(path);
            loadedScene = JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, DialogueLine>>>(jsonContent);


        }
        catch (FileNotFoundException ex)
        {
            return new DialogueResult(false, $"File not found: {ex.Message}", DialogueError.FileNotFound);
        }
        catch (DirectoryNotFoundException ex)
        {
            return new DialogueResult(false, $"Directory not found: {ex.Message}", DialogueError.FileNotFound);
        }
        catch (IOException ex)
        {
            return new DialogueResult(false, $"File error: {ex.Message}", DialogueError.FileReadError);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new DialogueResult(false, $"Access error: {ex.Message}", DialogueError.FileAccessDenied);
        }
        catch (JsonException ex)
        {
            return new DialogueResult(false, $"Invalid JSON: {ex.Message}", DialogueError.InvalidJson);
        }

        DialogueResult validation =  ValidateScene(loadedScene);
        if (!validation.Success)
        {
            return validation;
        }

        scene = loadedScene!;


        try
        {
            DialogueResult result =  UpdateLine();
            if (result.Success)
            {
                OnSceneLoaded?.Invoke();
            }
            return result;
        }
        catch (DialogueException ex)
        {
            UnloadScene();
            return HandleException(ex);
        }
    }

    public DialogueResult SelectChoice(int num)
    {
        if (DialogueState != State.Choice)
        {
            return new DialogueResult(false, "Cannot select choice outside Choice state", DialogueError.IllegalState);
        }

        if (!choices.TryGetValue(num, out var choice))
        {
            Console.WriteLine("Invalid Choice Selection");
            return new DialogueResult(false, $"Choice {num} is unavailable", DialogueError.InvalidChoice);
        }
        currentChoice = choice;
        return new DialogueResult(true);
    }

    public DialogueResult Advance()
    {
        if (DialogueState == State.Unloaded)
        {
            return new DialogueResult(false, "No scene is currently loaded.", DialogueError.IllegalState);
        }

        try {
            switch (DialogueState)
            {
                case State.Dialogue:
                    return AdvanceDialogue();

                case State.Choice:
                    return AdvanceChoice();

                case State.Last:
                    EndDialogue();
                    return new DialogueResult(true);

                case State.End:
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
        CurrentLineNum++;
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
            return new DialogueResult(false, "Cannot advance: no choice has been selected", DialogueError.IllegalState);
        }

        DialogueElement selectedChoice = currentChoice;
        currentChoice = null;

        if (saveData != null)
        {
            saveData.Update(selectedChoice.Metadata);
        }
        if (selectedChoice.VNCoreData.TryGetValue("switch", out string? destination))
        {
            return Switch(destination);
        }
        return MoveToNextLine();
    }

    private void EndDialogue()
    {
        if (DialogueState == State.Last)
        {
            DialogueState = State.End;
            OnDialogueEnded?.Invoke();
        }
    }

    private DialogueResult Switch(string destination)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new DialogueException(DialogueError.InvalidDestination, "Switch destination is empty", CurrentBranch, CurrentLineNum);
        }

        string[] parts = destination.Split(':');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int branch) || !int.TryParse(parts[1], out int line))
        {
            throw new DialogueException(DialogueError.InvalidDestination, $"Invalid switch destination: {destination}", CurrentBranch, CurrentLineNum);
        }

        if (!scene.TryGetValue(branch, out var branchData))
        {
            throw new DialogueException(DialogueError.InvalidDestination, $"Branch {branch} does not exist", CurrentBranch, CurrentLineNum);
        }

        if (!branchData.ContainsKey(line))
        {
            throw new DialogueException(DialogueError.InvalidDestination, $"Line {line} does not exist in branch {branch}", CurrentBranch, CurrentLineNum);
        }

        CurrentBranch = branch;
        CurrentLineNum = line;

        return UpdateLine();
    }

    private DialogueResult UpdateLine()
    {
        while (true)
        {
            if (!scene.TryGetValue(CurrentBranch, out var branch))
            {
                throw new DialogueException(DialogueError.InvalidScene, $"Branch {CurrentBranch} does not exist", CurrentBranch, CurrentLineNum);
            }
            if (!branch.TryGetValue(CurrentLineNum, out var line))
            {
                Console.WriteLine("End of Branch reached.");
                currentChoice = null;
                choices.Clear();
                DialogueState = State.Last;

                OnChoicesUpdated?.Invoke(choices);

                return new DialogueResult(true);
            }

            if (line == null)
            {
                throw new DialogueException(DialogueError.InvalidScene, $"Line {CurrentLineNum} in branch {CurrentBranch} is null", CurrentBranch, CurrentLineNum);
            }

            currentLine = new DialogueLine(line);

            if (string.IsNullOrWhiteSpace(currentLine.Speaker) && string.IsNullOrWhiteSpace(currentLine.Text))
            {
                CurrentLineNum++;
                continue;
            }

            if (currentLine.VNCoreData.TryGetValue("precondition", out var condition))
            {
                if (saveData == null || !saveData.Compare(condition))
                {
                    CurrentLineNum++;
                    continue;
                } 
            }

            if (currentLine.Text.StartsWith("$\""))
            {
                currentLine.Text = ReplaceFString(CurrentLine.Text);
            }

            OnDialogueChanged?.Invoke(currentLine);

            UpdateChoices();
            DialogueState = choices.Count > 0 ? State.Choice : State.Dialogue;

            OnChoicesUpdated?.Invoke(Choices);
            return new DialogueResult(true);
        }
    }

    private void UpdateChoices()
    {
        choices.Clear();
        currentChoice = null;

        foreach (var choice in currentLine.Choices)
        {
            bool available = true;

            if (choice.Value.VNCoreData.TryGetValue("precondition", out string? parameter))
            {
                Console.WriteLine("Precondition Detected");
                available = saveData != null && saveData.Compare(parameter);
            }

            if (!available)
            {
                continue;
            }

            choices[choice.Key] = new DialogueElement(choice.Value);

            if (choices[choice.Key].Text.StartsWith("$\""))
            {
                choices[choice.Key].Text = ReplaceFString(choices[choice.Key].Text);
            }
            
        }
        
    }

    private string ReplaceFString(string fString)
    {
        string text = fString[2..];

        if (saveData == null)
        {
            return text;
        }

        return Regex.Replace(text, @"\{([^{}]+)\}", match =>
        {
            string variable = match.Groups[1].Value;
            if (saveData.Data.TryGetValue(variable, out var value))
                return value.ToString();

            return match.Value;
        });
    }

    public DialogueResult ValidateScene(Dictionary<int, Dictionary<int, DialogueLine>>? scene)
    {

        try
        {
            
            if (scene == null)
            {
                throw new DialogueException(DialogueError.InvalidScene, "Scene data is empty");
            }
            
            if (!scene.TryGetValue(0, out var firstBranch) || firstBranch == null || !firstBranch.ContainsKey(0))
            {
                throw new DialogueException(DialogueError.InvalidScene, "Scene must contain branch 0 line 0");
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

    public void UnloadScene()
    {
        scene.Clear();
        saveData = new SaveData();
        currentLine = new DialogueLine();
        CurrentBranch = 0;
        CurrentLineNum = 0;
        choices.Clear();
        currentChoice = null;
        DialogueState = State.Unloaded;
        OnChoicesUpdated?.Invoke(Choices);
        OnSceneUnloaded?.Invoke();
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
        return new DialogueResult(false, ex.Message, ex.Error, ex.CurrentBranch >= 0 ? ex.CurrentBranch : null, ex.CurrentLine >= 0 ? ex.CurrentLine : null);
    }

    public void SetLine (int branch, int line)
    {
        CurrentBranch = branch;
        CurrentLineNum = line;
        UpdateLine();
    }

    public void SetSaveData (SaveData save)
    {
        saveData = save;
    }

    public void RemoveSaveData ()
    {
        saveData = new SaveData();
    }
}