using System.Text.Json;
using VNCore.Dialogue;

namespace VNCore.Compiler;

enum State
{
    SettingSpeaker,
    SettingText

}


public class ScriptCompiler
{
    public void Compile(string filepath)
    {
        Dictionary<int, Dictionary<int, DialogueLine>> scene = new Dictionary<int, Dictionary<int, DialogueLine>>();
        Dictionary<int, DialogueLine> lines = new Dictionary<int, DialogueLine>();

        int branchId = 1;
        int lineId = -1;

        scene[branchId] = lines;

        State state = State.SettingSpeaker;
        string lineText = "";

        foreach (var line in File.ReadLines(filepath))
        {
            var trimmed = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmed))
                continue;

            switch (state)
            {
                case State.SettingSpeaker:
                    if (trimmed.EndsWith(':'))
                    {
                        lineId ++;
                        lines[lineId] = new DialogueLine {speaker = trimmed[..^1]};
                        state = State.SettingText;
                    }
                    break;

                case State.SettingText: // May need to reconsider formatting later when adding " into actual dialogue
                    bool startsText = trimmed.StartsWith('"');
                    bool endsText = trimmed.EndsWith('"');

                    if (startsText && trimmed.Length > 1)
                        trimmed = trimmed[1..];

                    if (endsText && trimmed.Length > 0)
                        trimmed = trimmed[..^1];

                    lineText += (lineText.Length > 0 ? " " : "") + trimmed;

                    if (endsText)
                    {
                        scene[branchId][lineId].text = lineText;

                        Console.WriteLine(
                            $"Saving line {lineId}: " +
                            $"speaker='{scene[branchId][lineId].speaker}', " +
                            $"text='{scene[branchId][lineId].text}'"
                        );

                        lineText = "";
                        state = State.SettingSpeaker;
                    }

                    break;
            }
        }
        string json = JsonSerializer.Serialize(scene, JsonContext.Default.DictionaryInt32DictionaryInt32DialogueLine);
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
// to make ^ easier, instead of adding ifs, might just make users duplicate the file and check in game conditions for which one is used.



// 
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//