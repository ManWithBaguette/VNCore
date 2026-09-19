namespace VNCore.Dialogue;

public class DialogueElement
{
    public string Text { get; set; } = "";
    public Dictionary<string, string> VNCoreData { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}