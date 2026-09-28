using System.Text.Json;
using VNCore.Dialogue;

namespace VNCore.DialogueManager;

public class DialogueManager
{
    private Dictionary<int, Dictionary<int, DialogueLine>> script = new();
    private DialogueLine currentLine;
    private int currentBranch = 1;
    private int currentLineNum = 0;
    

    public void readScript(string path)
    {
        string jsonContent = File.ReadAllText(path);
        script = JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, DialogueLine>>>(jsonContent);
        int i = 1;
        int j = 0;
        int k = 0;
        while (true)
        {

            Console.WriteLine($"{script[i][j].Speaker}");
            j++;
            nextLine();
        }
    }

    public void nextLine()
    {
        var x = script[currentBranch][0].VNCoreData;
        Console.WriteLine(x.ContainsKey("switch"));
        if (script[currentBranch][0].VNCoreData.ContainsKey("switch"))
        {
            Console.WriteLine("This Line contains switch");
        }
    }
}