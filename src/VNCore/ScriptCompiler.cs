using System.Text.Json;
using System.Text.Json.Serialization;

namespace VNCore.Compiler;
public class ScriptCompiler
{
    public void Compile(string filepath)
    {
        Dictionary<int, string> lines = new Dictionary<int, string>();
        
        int currentLine = 0;

        foreach (var line in File.ReadLines(filepath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                currentLine ++;
            }
            else
            {
                lines[currentLine] = line;
            }
        }

        string json = JsonSerializer.Serialize(lines, JsonContext.Default.DictionaryInt32String);
        string outputPath = Path.GetFullPath("output.json");
        Console.WriteLine(outputPath);
        File.WriteAllText("output.json", json);
    }
}

class Test
{
    static void Main()
    {
        var compiler = new ScriptCompiler();
        compiler.Compile("test.txt");
    }
}

// Alice: 
// "Hello."
//     wait: 0.5
//     autoAdvance: true
//     expression: happy 
//     voice: alice_hello.wav 
//     animation: wave

// Turns into

// {
//     "speaker": "Alice",
//     "text": "Hello.",
//
//     "engine": {
//         "wait": 0.5,
//         "autoAdvance": true
//     },
//
//     "metadata": {
//         "expression": "happy",
//         "voice": "alice_hello.wav",
//         "animation": "wave",
//     }
// }


//:
//""
//Choice:  
//  "Do x"
//      affinity: +1
//  if: affinity=1, hate=1
//      "choice 2 unlocks. when dialogue sees if, it checks condition using seperate function, probably need a condition check function in game to work properly
//       if returns true, then reads the next tabbed line, otherwise it pretends they dont exist. for " it should treat multiple lines as the same type until the second " is found"