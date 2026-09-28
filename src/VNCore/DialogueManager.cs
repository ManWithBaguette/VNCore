using System.Text.Json;
using VNCore.Dialogue;

namespace VNCore.DialogueManager;

public class DialogueManager
{
    private Dictionary<int, Dictionary<int, DialogueLine>> scene = new();
    private DialogueLine currentLine;
    private int currentBranch = 1;
    private int currentLineNum = 0;

    private List<DialogueLine> dialogueTree = new();
    

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
            nextLine();
            currentLineNum++;
        }
    }

    public void nextLine()
    {
        if (currentLine.VNCoreData.ContainsKey("switch"))
        {
            int[] switchDestination = currentLine.VNCoreData["switch"].Split(':').Select(int.Parse).ToArray();
            Console.WriteLine("This Line contains switch");
        }
        else
        {
            Console.WriteLine("No switch");
        }
    }
}