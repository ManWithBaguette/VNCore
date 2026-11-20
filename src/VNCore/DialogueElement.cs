using System.Text.Json.Serialization;

namespace VNCore.Dialogue;

public class DialogueElement
{
    [JsonPropertyOrder(1)]
    public string Text { get; set; } = "";
    [JsonPropertyOrder(98)]
    public Dictionary<string, string> VNCoreData { get; set; } = new();
    [JsonPropertyOrder(99)]
    public Dictionary<string, string> Metadata { get; set; } = new();

    public DialogueElement() {}

    public DialogueElement(DialogueElement original)
    {
        Text = original.Text;
        VNCoreData = new Dictionary<string, string>(original.VNCoreData);
        Metadata = new Dictionary<string, string>(original.Metadata);
    }
}