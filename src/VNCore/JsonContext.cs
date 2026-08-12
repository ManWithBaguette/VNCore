using System.Text.Json.Serialization;

namespace VNCore.Compiler;

[JsonSerializable(typeof(Dictionary<int, string>))]
public partial class JsonContext : JsonSerializerContext
{
}