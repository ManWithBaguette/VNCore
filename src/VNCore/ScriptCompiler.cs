using System.Text.Json;
using VNCore.Dialogue;

namespace VNCore.Compiler;
public class ScriptCompiler
{
    public void Compile(string filepath)
    {
        Dictionary<int, DialogueLine> lines = new Dictionary<int, DialogueLine>();
        
        int currentLine = 0;
        lines[currentLine] = new DialogueLine();
        foreach (var line in File.ReadLines(filepath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                currentLine ++;
            }
            else
            {
                //lines[currentLine].Add(line);
            }
        }

        string json = JsonSerializer.Serialize(lines, JsonContext.Default.DictionaryInt32DialogueLine);
        string outputPath = Path.GetFullPath("output.json");
        Console.WriteLine(outputPath);
        File.WriteAllText("output.json", json);
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