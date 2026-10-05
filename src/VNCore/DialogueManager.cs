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

    private List<DialogueLine> dialogueTree = new();
    
    public DialogueManager(SaveData saveData)
    {
        this.saveData = saveData;
    }

    public void LoadScene(string path)
    {
        string jsonContent = File.ReadAllText(path);
        scene = JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, DialogueLine>>>(jsonContent);
    }
    public DialogueLine GetCurrentLine()
    {
        return currentLine;
    }

    public void SelectChoice(int num)
    {

    }


    public void readScript(string path)
    {
        string jsonContent = File.ReadAllText(path);
        scene = JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, DialogueLine>>>(jsonContent);
        // dialogueTree.Add(currentLine);
        while (scene[currentBranch].ContainsKey(currentLineNum))
        {
            currentLine = scene[currentBranch][currentLineNum];
            dialogueTree.Add(currentLine);
            Console.WriteLine($"{scene[currentBranch][currentLineNum].Speaker}");
            Advance();
        }
    }

    public void Advance()
    {
        if (currentLine.VNCoreData.ContainsKey("switch"))
        {
            int[] switchDestination = currentLine.VNCoreData["switch"].Split(':').Select(int.Parse).ToArray();
            Console.WriteLine("This Line contains switch");
            currentBranch = switchDestination[0];
            currentLineNum = switchDestination[1];
        }
        else
        {
            Console.WriteLine("No switch");
            currentLineNum++;
        }
    }
}