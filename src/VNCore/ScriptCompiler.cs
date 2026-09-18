using System.Text.Json;
using VNCore.Dialogue;

namespace VNCore.Compiler;

enum State
{
    SettingSpeaker,
    SettingText,
    SettingLineData,

}


public class ScriptCompiler
{
    public void Compile(string filepath)
    {
        Dictionary<int, Dictionary<int, DialogueLine>> scene = new Dictionary<int, Dictionary<int, DialogueLine>>();
        
        Queue<string> fileQueue = new Queue<string>();
        fileQueue.Enqueue(filepath);


        int branchId = 1;


        while (fileQueue.Count > 0)
        {   
            Dictionary<int, DialogueLine> lines = new Dictionary<int, DialogueLine>();
            scene[branchId] = lines;
            State state = State.SettingSpeaker;
            string lineText = "";
            int lineId = -1;

            foreach (var line in File.ReadLines(fileQueue.Dequeue()))
            {
                var trimmed = line.Trim();


                switch (state)
                {
                    case State.SettingSpeaker:
                        if (string.IsNullOrWhiteSpace(trimmed))
                            continue;

                        if (trimmed.EndsWith(':'))
                        {
                            lineId ++;
                            lines[lineId] = new DialogueLine {speaker = trimmed[..^1]};
                            state = State.SettingText;
                        }
                        break;

                    case State.SettingText: // May need to reconsider formatting later when adding " into actual dialogue. Also modify later so can also start on the same line as speaker: i.e. Nelson: LineText
                        // Consider making anything that comes after the speaker name text. add a seperate stop condition for ending the line text and transitioning into metadata collection
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
                            state = State.SettingLineData;
                        }

                        break;
                    case State.SettingLineData:
                        if (string.IsNullOrWhiteSpace(trimmed))
                            {
                                state = State.SettingSpeaker;
                                continue;
                            }
                        int colonIndex = trimmed.IndexOf(':');
                        string flag = trimmed[..colonIndex].Trim();
                        string paramater = trimmed[(colonIndex + 1)..].Trim();

                        // Optionally remove VNCoreData and only have metadata, or add if cases here to sort between VNCoreData and other Metadata
                        // Consider making a conditional dictionary nested inside the metadata dictionaries for if cases in metadata. i.e. if x, anne: anger+3
                        scene[branchId][lineId].VNCoreData[flag] = paramater;

                        if (flag == "switch")
                        {
                            fileQueue.Enqueue(paramater);
                            Console.WriteLine(flag);
                            Console.WriteLine(paramater);
                        }

                        break;

                }
            }
            branchId ++;
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