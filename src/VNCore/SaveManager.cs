using System.Text.Json;
using VNCore.Compiler;
namespace VNCore;

public class SaveManager
{
    public SaveData Current { get; set; } = new();
    public void WriteSave() // Consider changing string path to int, etc. decide on how files are written (local folder, etc.) maybe overload
    {
        string json = JsonSerializer.Serialize(Current, JsonContext.Default.SaveData);
        File.WriteAllText($"SaveFile.json", json);
    }

    public void LoadSave(string path)
    {
        string json = File.ReadAllText($"SaveFile.json");
        Current = JsonSerializer.Deserialize<SaveData>(json);
    }

}