using System.Text.Json;
using VNCore.Dialogue;

namespace VNCore.DialogueManager;

public class DialogueManager
{
    private SaveData saveData;
    private Dictionary<int, Dictionary<int, DialogueLine>> scene = new();
    private DialogueLine currentLine = new();
    private int currentBranch = 1;
    private int currentLineNum = 0;

    public Dictionary<int, DialogueElement> choices = new();

    private List<DialogueLine> dialogueTree = new();
    
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
    }
    public DialogueLine GetCurrentLine()
    {
        return currentLine;
    }

    public void SelectChoice(int num) // Assume choice selection in range.
    {
        DialogueElement choice = choices[num];

        saveData.Update(choice.Metadata);

        if (choice.VNCoreData.TryGetValue("switch", out string? destination))
        {
            Switch(destination);
        } else
        {
            Advance();
        }
    }


    public void readScript(string path) // Testing function. The contents of this function should be essentially made on the user side after completion.
    {
        LoadScene(path);
        // dialogueTree.Add(currentLine);
        while (scene[currentBranch].ContainsKey(currentLineNum))
        {
            currentLine = scene[currentBranch][currentLineNum];
            dialogueTree.Add(currentLine);
            Console.WriteLine($"{scene[currentBranch][currentLineNum].Speaker}");

            if (currentLine.Choices.Count > 0) 
            {
                int select;
                while (true)
                {
                    Console.WriteLine("Select a Choice");
                    if (int.TryParse(Console.ReadLine(), out int result))
                    {
                        if (currentLine.Choices.ContainsKey(result))
                        {
                            select = result;
                            break;
                        } else
                        {
                            Console.WriteLine("Choice invalid, enter again.");
                        }
                    }
                }
                
                SelectChoice(select);
            } else
            {
                Console.ReadLine();
                Advance();
            }
        }
    }

    public void Advance()
    {

        saveData.Update(currentLine.Metadata);

        choices = currentLine.Choices; 
        Console.WriteLine("Choices updated");

        if (currentLine.VNCoreData.TryGetValue("switch", out string? destination))
        {
            Switch(destination);
        }
        else
        {
            Console.WriteLine("No switch");
            currentLineNum++;
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
    }
}