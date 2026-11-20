using System.Text.Json.Serialization;

namespace VNCore.Dialogue;

public class DialogueLine : DialogueElement
{
    [JsonPropertyOrder(0)]
    public string Speaker { get; set; } = "";
    [JsonPropertyOrder(100)]
    public Dictionary<int, DialogueElement> Choices { get; set; } = new();

    public DialogueLine() {}

    public DialogueLine(DialogueLine original) : base(original)
    {
        Speaker = original.Speaker;
        Choices = original.Choices.ToDictionary(choice => choice.Key, choice => new DialogueElement(choice.Value));
    }
}