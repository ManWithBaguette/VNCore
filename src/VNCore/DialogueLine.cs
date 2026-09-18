namespace VNCore.Dialogue;

public class DialogueLine
{
    public string speaker { get; set; } = "";
    public string text { get; set; } = "";
    public Dictionary<string, string> VNCoreData { get; set; } = new();
    public Dictionary<string, string> MetaData { get; set; } = new();

}