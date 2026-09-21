using System.Text.Json;
using VNCore.Dialogue;

namespace VNCore.DialogueManager;

public class DialogueManager
{
    private Dictionary<int, Dictionary<int, DialogueLine>> script = new();
    private int currentBranch;
    private int currentLine;

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
        }
    }
}