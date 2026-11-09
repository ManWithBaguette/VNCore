using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using VNCore.Dialogue;

namespace VNCore.DialogueManager;

public enum State
{
    DIALOGUE,
    CHOICE,
    END
}

public class DialogueManager //Note. modify so that dialogue doesnt require a save file.
{
    public SaveData? saveData;
    private Dictionary<int, Dictionary<int, DialogueLine>> scene = new();
    private DialogueLine currentLine = new();
    private int currentBranch = 1;
    private int currentLineNum = 0;

    public State dialogueState;

    public Dictionary<int, DialogueElement> choices = new();

    private List<DialogueLine> dialogueTree = new();
    
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
        choices = currentLine.Choices;
        if (choices.Count() > 0)
        {
            dialogueState = State.CHOICE;
        }
    }
    public DialogueLine GetCurrentLine()
    {
        return currentLine;
    }

    public void SelectChoice(int num) // Assume choice selection in range.
    {
        if (! choices.ContainsKey(num))
        {
            Console.WriteLine("Invalid Choice Selection");
            return;
        }
        DialogueElement choice = choices[num];

        if (saveData != null)
            saveData.Update(choice.Metadata);

        if (choice.VNCoreData.TryGetValue("switch", out string? destination))
        {
            Switch(destination);
        } else
        {
            Console.WriteLine("Choice with no switch selected.");
            dialogueState = State.DIALOGUE;
            currentLineNum++;
            UpdateLine();
            // Advance();
        }
    }

    public void Advance()
    {

        choices = currentLine.Choices;
        Console.WriteLine("Choices updated");

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
            return;
        }
        
        if (saveData != null)
            saveData.Update(currentLine.Metadata);


        if (currentLine.VNCoreData.TryGetValue("switch", out string? destination))
        {
            Switch(destination);
        }
        else
        {
            Console.WriteLine("No switch");
            currentLineNum++;
            UpdateLine();
        }
        
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
        choices = currentLine.Choices;
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
            dialogueState = State.END;
            return;
        }
        currentLine = scene[currentBranch][currentLineNum];
        choices = currentLine.Choices;
    }
}