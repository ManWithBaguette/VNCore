using System.Text.Json;
using VNCore.Dialogue;

namespace VNCore.Compiler;

enum State
{
    SettingSpeaker,
    SettingText,
    SettingLineData,
    SettingChoice,
    SettingChoiceText,
    SettingChoiceData,

}


public class ScriptCompiler
{
    private Dictionary<int, Dictionary<int, DialogueLine>> scene = [];
    private Queue<string> fileQueue = new();
    private List<string> compiledFiles = new();
    private int branchId;
    private string lineText = "";
    private int lineId;
    private int choiceId;

    private State state;

    public void Compile(string filepath)
    {
        scene = new Dictionary<int, Dictionary<int, DialogueLine>>();
        fileQueue = new Queue<string>();
        fileQueue.Enqueue(filepath);

        compiledFiles.Add(filepath);

        branchId = 1;

        while (fileQueue.Count > 0)
        {   
            Dictionary<int, DialogueLine> lines = new Dictionary<int, DialogueLine>();
            scene[branchId] = lines;
            state = State.SettingSpeaker;
            lineId = -1;
        
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
                            lines[lineId] = new DialogueLine {Speaker = trimmed[..^1]};
                            state = State.SettingText;
                        }
                        Console.WriteLine("Speaker Set");
                        break;

                    case State.SettingText: // May need to reconsider formatting later when adding " into actual dialogue. Also modify later so can also start on the same line as speaker: i.e. Nelson: LineText
                        // Consider making anything that comes after the speaker name text. add a seperate stop condition for ending the line text and transitioning into metadata collection
                        SetText(trimmed);
                        break;
                    
                    case State.SettingLineData:

                        // Optionally remove VNCoreData and only have metadata, or add if cases here to sort between VNCoreData and other Metadata
                        // Consider making a conditional dictionary nested inside the metadata dictionaries for if cases in metadata. i.e. if x, anne: anger+3
                        SetData(trimmed);
                        
                        break;

                    case State.SettingChoice:
                        choiceId ++;
                        scene[branchId][lineId].Choices[choiceId] = new DialogueElement();
                        state = State.SettingChoiceText;

                        break;
                    case State.SettingChoiceText:
                        SetText(trimmed);
                        break;
                    case State.SettingChoiceData:
                        SetData(trimmed);
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

    private void SetText(string line)
    {
        bool startsText = line.StartsWith('"');
        bool endsText = line.EndsWith('"');

        if (startsText && line.Length > 1)
            line = line[1..];

        if (endsText && line.Length > 0)
            line = line[..^1];

        lineText += (lineText.Length > 0 ? " " : "") + line;

        if (endsText)
        {
            if (state == State.SettingText) {
                scene[branchId][lineId].Text = lineText;
                state = State.SettingLineData;
            } else if (state == State.SettingChoiceText) {
                scene[branchId][lineId].Choices[choiceId].Text = lineText;
                Console.WriteLine($"Choice Text: {lineText}");
                state = State.SettingChoiceData;
            }
            
            Console.WriteLine(
                $"Saving line {lineId}: " +
                $"speaker='{scene[branchId][lineId].Speaker}', " +
                $"text='{scene[branchId][lineId].Text}'"
            );

            lineText = "";
        }
        Console.WriteLine("Text Set");
    }
    private void SetData(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            state = State.SettingSpeaker;
            return;
        }

        int colonIndex = line.IndexOf(':');
        string flag = line[..colonIndex].Trim();
        string parameter = line[(colonIndex + 1)..].Trim();
        
        
        if (flag == "switch")
        {

            string switchTarget;

            if (int.TryParse(parameter, out int value))
            {
                switchTarget = value.ToString();
            }
            else if (parameter.Contains(':'))
            {
                Dictionary<string, string> switchParameters = new();
                string[] parts = parameter.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (string part in parts)
                {
                    string[] keyValue = part.Split(':', 2);
                    switchParameters[keyValue[0]] = keyValue[1];
                }

                if (switchParameters.TryGetValue("path", out string? target))
                {
                    if (!compiledFiles.Contains(target))
                    {
                        fileQueue.Enqueue(target);
                        compiledFiles.Add(target);
                    }
                    switchTarget = compiledFiles.IndexOf(target).ToString();
                } else
                    switchTarget = branchId.ToString();

                if (switchParameters.TryGetValue("line", out string? lineValue))
                {
                    switchTarget = $"{switchTarget}:{lineValue}";
                }
            }
            else
                switchTarget = compiledFiles.IndexOf(parameter).ToString();

            if (state == State.SettingLineData)
                scene[branchId][lineId].VNCoreData[flag] = switchTarget;
            else if (state == State.SettingChoiceData)
                scene[branchId][lineId].Choices[choiceId].VNCoreData[flag] = switchTarget;

            return;

        } else if (flag == "choice" && state == State.SettingLineData) {
            state = State.SettingChoice;
        } else if (int.TryParse(flag, out int value))
        {
            choiceId ++;
            scene[branchId][lineId].Choices[choiceId] = new DialogueElement();
            state = State.SettingChoiceText;
        }

        if (state == State.SettingLineData)
        {
            scene[branchId][lineId].VNCoreData[flag] = parameter;
            Console.WriteLine("LineData Set");
        }
        else if (state == State.SettingChoiceData)
        {
            scene[branchId][lineId].Choices[choiceId].VNCoreData[flag] = parameter;
            Console.WriteLine("ChoiceData Set");
        }

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