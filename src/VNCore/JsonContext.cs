using System.Text.Json.Serialization;

namespace VNCore.Compiler;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<int, string>))]
public partial class JsonContext : JsonSerializerContext
{
}