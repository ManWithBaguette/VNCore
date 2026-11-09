using System.Diagnostics;
using System.Drawing;
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

    private List<String> CoreDataFlags = [
        "switch",
        "condition",
        "precondition",
        "postcondition",


    ];

    private State state;

    public void Compile(string filepath, string outputPath)
    {
        scene = new Dictionary<int, Dictionary<int, DialogueLine>>();
        compiledFiles = new List<string>();
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

                        int colonIndex = trimmed.IndexOf(':');

                        if (colonIndex != -1)
                        {
                            lineId ++;
                            lines[lineId] = new DialogueLine {Speaker = trimmed[..colonIndex].Trim()};

                            lineText = trimmed[(colonIndex + 1)..].Trim();
                            state = State.SettingText;
                        }
                        Console.WriteLine("Speaker Set");
                        break;

                    case State.SettingText: 
                        SetText(trimmed);
                        break;
                    
                    case State.SettingLineData:

                        // Optionally remove VNCoreData and only have metadata, or add if cases here to sort between VNCoreData and other Metadata
                        // Consider making a conditional dictionary nested inside the metadata dictionaries for if cases in metadata. i.e. if x, anne: anger+3
                        SetData(trimmed);
                        
                        break;

                    case State.SettingChoice:
                        choiceId = 1;
                        scene[branchId][lineId].Choices[choiceId] = new DialogueElement();
                        if (!string.IsNullOrWhiteSpace(trimmed))
                        {
                            lineText = trimmed.Contains(':') ? trimmed[(trimmed.IndexOf(':') + 1)..].Trim() : trimmed;
                        }
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

            if (!string.IsNullOrWhiteSpace(lineText))
            {
                if (state == State.SettingText) 
                    scene[branchId][lineId].Text = lineText;
                else if (state == State.SettingChoiceText)
                    scene[branchId][lineId].Choices[choiceId].Text = lineText;

                lineText = "";
            }
            
            branchId ++;
        }
        string json = JsonSerializer.Serialize(scene, JsonContext.Default.DictionaryInt32DictionaryInt32DialogueLine);
        Console.WriteLine($"{outputPath}.json");
        File.WriteAllText($"scenes\\{outputPath}.json", json);
    }

    private void SetText(string line)
    {
        if (line.Contains(':') || string.IsNullOrWhiteSpace(line))
        {
            if (state == State.SettingText)
            {
                scene[branchId][lineId].Text = lineText;
                state = State.SettingLineData;
            }
            if (state == State.SettingChoiceText)
            {
                scene[branchId][lineId].Choices[choiceId].Text = lineText;
                state = State.SettingChoiceData;
            }
            Console.WriteLine(
                $"Saving line {lineId}: " +
                $"speaker='{scene[branchId][lineId].Speaker}', " +
                $"text='{lineText}'"
            );
            lineText = "";
            SetData(line);
            return;
        }
        
        lineText += (lineText.Length > 0 ? " " : "") + line;
        Console.WriteLine($"Text Added: {line}");


        // bool startsText = line.StartsWith('"');
        // bool endsText = line.EndsWith('"');

        // if (startsText && line.Length > 1)
        //     line = line[1..];

        // if (endsText && line.Length > 0)
        //     line = line[..^1];

        // lineText += (lineText.Length > 0 ? " " : "") + line;

        // if (endsText)
        // {
        //     if (state == State.SettingText) {
        //         scene[branchId][lineId].Text = lineText;
        //         state = State.SettingLineData;
        //     } else if (state == State.SettingChoiceText) {
        //         scene[branchId][lineId].Choices[choiceId].Text = lineText;
        //         Console.WriteLine($"Choice Text: {lineText}");
        //         state = State.SettingChoiceData;
        //     }
            
        //     Console.WriteLine(
        //         $"Saving line {lineId}: " +
        //         $"speaker='{scene[branchId][lineId].Speaker}', " +
        //         $"text='{scene[branchId][lineId].Text}'"
        //     );

        //     lineText = "";
        // }
        // Console.WriteLine("Text Set");
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
        
        bool isCoreData = CoreDataFlags.Contains(flag.ToLower());

        
        if (flag == "switch")
        {

            string switchTarget;

            if (int.TryParse(parameter, out int value))
            {
                switchTarget = $"0:{value}";
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
                switchTarget = $"{compiledFiles.IndexOf(parameter)}:0";

            if (state == State.SettingLineData)
                scene[branchId][lineId].VNCoreData[flag] = switchTarget;
            else if (state == State.SettingChoiceData)
                scene[branchId][lineId].Choices[choiceId].VNCoreData[flag] = switchTarget;

            Console.WriteLine("Switch flag set");
            return;

        } else if (flag == "choice" && state == State.SettingLineData) {
            state = State.SettingChoice;
        } else if (int.TryParse(flag, out int value) && state == State.SettingChoiceData)
        {
            choiceId ++;
            scene[branchId][lineId].Choices[choiceId] = new DialogueElement();
            lineText = parameter;
            state = State.SettingChoiceText;
        }

        if (isCoreData)
        {
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
        } else
        {
            if (state == State.SettingLineData)
            {
                scene[branchId][lineId].Metadata[flag] = parameter;
                Console.WriteLine("LineData Set");
            }
            else if (state == State.SettingChoiceData)
            {
                scene[branchId][lineId].Choices[choiceId].Metadata[flag] = parameter;
                Console.WriteLine("ChoiceData Set");
            }
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