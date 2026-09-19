namespace VNCore.Dialogue;

public class DialogueLine : DialogueElement
{
    public string Speaker { get; set; } = "";
    public Dictionary<int, DialogueElement> Choices { get; set; } = new();
}