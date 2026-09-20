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
}