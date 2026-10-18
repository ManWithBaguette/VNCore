using System.Text.Json.Serialization;
using VNCore.Dialogue;

namespace VNCore.Compiler;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SaveData))]
[JsonSerializable(typeof(Dictionary<int, DialogueLine>))]
[JsonSerializable(typeof(Dictionary<int, Dictionary<int, DialogueLine>>))]
public partial class JsonContext : JsonSerializerContext
{
}