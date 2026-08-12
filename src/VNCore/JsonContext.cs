using System.Text.Json.Serialization;
using VNCore.Dialogue;

namespace VNCore.Compiler;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<int, DialogueLine>))]
public partial class JsonContext : JsonSerializerContext
{
}