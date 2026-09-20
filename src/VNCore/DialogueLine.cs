using System.Text.Json.Serialization;

namespace VNCore.Dialogue;

public class DialogueLine : DialogueElement
{
    [JsonPropertyOrder(0)]
    public string Speaker { get; set; } = "";
    [JsonPropertyOrder(100)]
    public Dictionary<int, DialogueElement> Choices { get; set; } = new();
}