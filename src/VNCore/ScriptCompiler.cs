using System.Text.Json;
using VNCore.Dialogue;
using VNCore.DialogueManager;

namespace VNCore.Compiler;

enum MainState
{  
    None,
    SettingSpeaker,
    SettingText,
    SettingLineData
}

enum BlockState
{
    None,
    SettingMetadata,
    SettingChoice,
    AddingChoice,
    SettingChoiceText,
    SettingChoiceData
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

    private string lineFlag = "";
    private string lineParameter = "";

    private Dictionary<int, DialogueLine> lines = [];

    private List<String> CoreDataFlags = [
        "switch",
        "condition",
        "precondition",
        "postcondition",


    ];

    private MainState mainState;
    private BlockState blockState;

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
            lines = new Dictionary<int, DialogueLine>();
            scene[branchId] = lines;
            lineId = -1;
        
            foreach (var line in File.ReadLines(fileQueue.Dequeue()))
            {
                int indentation = GetIndentation(line);
                string trimmed = line.Trim();

                UpdateMainState(trimmed, indentation);


                switch (mainState)
                {
                    case MainState.SettingSpeaker:
                        SetSpeaker(trimmed);
                        Console.WriteLine("Speaker Set");
                        break;

                    case MainState.SettingText: 
                        SetText(trimmed);
                        break;
                    
                    case MainState.SettingLineData:
                        SetData(trimmed);
                        break;

                    // case MainState.SettingLineData:
                    
                    //     break;
                    // case State.SettingChoice:
                    //     CreateChoice(trimmed);

                    //     break;
                    // case State.SettingChoiceText:
                    //     SetText(trimmed);
                    //     break;
                    // case State.SettingChoiceData:
                    //     SetData(trimmed);
                    //     break;
                }

                if (mainState == MainState.SettingLineData)
                {
                    switch (blockState)
                    {
                        case BlockState.SettingMetadata:
                            break;
                    }
                }
                

            }

            // if (!string.IsNullOrWhiteSpace(lineText))
            // {
            //     if (state == State.SettingText) 
            //         scene[branchId][lineId].Text = lineText;
            //     else if (state == State.SettingChoiceText)
            //         scene[branchId][lineId].Choices[choiceId].Text = lineText;

            //     lineText = "";
            // }
            
            branchId ++;
        }
        string json = JsonSerializer.Serialize(scene, JsonContext.Default.DictionaryInt32DictionaryInt32DialogueLine);
        Console.WriteLine($"{outputPath}.json");
        File.WriteAllText($"scenes\\{outputPath}.json", json);
    }

    private void SetText(string line)
    {
        lineText += (lineText.Length > 0 ? " " : "") + line;
        Console.WriteLine($"Text Added: {line}");

        if (mainState == MainState.SettingText)
        {
            scene[branchId][lineId].Text = lineText;
        }
        else if (blockState == BlockState.SettingChoiceText)
        {
            scene[branchId][lineId].Choices[choiceId].Text = lineText;
        }

        Console.WriteLine(
            $"Saving line {lineId}: " +
            $"speaker='{scene[branchId][lineId].Speaker}', " +
            $"text='{lineText}'"
        );
    }

    private void SetSpeaker(string line)
    {
        lineId ++;
        lines[lineId] = new DialogueLine {Speaker = lineFlag.Trim()};
        lineText = lineParameter.Trim();
    }

    private void CreateChoice(string line)
    {
        choiceId = 1;
        scene[branchId][lineId].Choices[choiceId] = new DialogueElement();
        if (!string.IsNullOrWhiteSpace(line))
        {
            lineText = line.Contains(':') ? line[(line.IndexOf(':') + 1)..].Trim() : line;
        }
        blockState = BlockState.SettingChoiceText;
    }

    private void SetData(string line)
    {
        bool isCoreData = CoreDataFlags.Contains(lineFlag.ToLower());
        lineFlag = isCoreData ? lineFlag.ToLower() : lineFlag;
        
        if (lineFlag == "switch")
        {
            string switchTarget;

            if (int.TryParse(lineParameter, out int value))
            {
                switchTarget = $"0:{value}";
            }
            else if (lineParameter.Contains(':'))
            {
                Dictionary<string, string> switchParameters = new();
                string[] parts = lineParameter.Split(' ', StringSplitOptions.RemoveEmptyEntries);

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
                switchTarget = $"{compiledFiles.IndexOf(lineParameter)}:0";

            if (mainState == MainState.SettingLineData)
                scene[branchId][lineId].VNCoreData[lineFlag] = switchTarget;
            else if (blockState == BlockState.SettingChoiceData)
                scene[branchId][lineId].Choices[choiceId].VNCoreData[lineFlag] = switchTarget;

            Console.WriteLine("Switch flag set");
            return;
        }

        if (isCoreData)
        {
            if (mainState == MainState.SettingLineData)
            {
                scene[branchId][lineId].VNCoreData[lineFlag] = lineParameter;
                Console.WriteLine("LineData Set");
            }
            else if (blockState == BlockState.SettingChoiceData)
            {
                scene[branchId][lineId].Choices[choiceId].VNCoreData[lineFlag] = lineParameter;
                Console.WriteLine("ChoiceData Set");
            }
        } else
        {
            if (mainState == MainState.SettingLineData)
            {
                scene[branchId][lineId].Metadata[lineFlag] = lineParameter;
                Console.WriteLine("LineData Set");
            }
            else if (blockState == BlockState.SettingChoiceData)
            {
                scene[branchId][lineId].Choices[choiceId].Metadata[lineFlag] = lineParameter;
                Console.WriteLine("ChoiceData Set");
            }
        }
        
    }

    private void UpdateMainState(string line, int indentation)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            mainState = MainState.None;
            blockState = BlockState.None;
            return;
        }

        if (line.Contains(':'))
        {
            lineFlag = line[..line.IndexOf(':')];
            lineParameter = line[(line.IndexOf(':') + 1)..];
        }
        else
        {
            lineFlag = "";
            lineParameter = "";
        }

        if (mainState == MainState.None)
        {
            mainState = MainState.SettingSpeaker;
        }
        else if (mainState == MainState.SettingSpeaker)
        {
            if (indentation == 1 && line.Contains(':'))
            {
                mainState = MainState.SettingLineData;
            }
            else
            {
                mainState = MainState.SettingText;
            }
        }
        else if (mainState == MainState.SettingText)
        {
            if (indentation == 1 && line.Contains(':'))
            {
                mainState = MainState.SettingLineData;
            }
        }

        if (mainState == MainState.SettingLineData)
        {   
            UpdateBlockState(line, indentation);
        }

    }

    private void UpdateBlockState(string line, int indentation)
    {
        if (line.Contains(':') && indentation > 0)
        {

            switch (indentation) {
                case 1:
                    if (lineFlag.Equals("choice", StringComparison.CurrentCultureIgnoreCase))
                    {
                        blockState = BlockState.SettingChoice;
                    }
                    else
                    {
                        blockState = BlockState.SettingMetadata;
                    }
                    return;

                case 2:
                    if (lineFlag.StartsWith("choice-"))
                    {    
                        blockState = BlockState.AddingChoice;
                    }
                    return;

                case 3:
                    blockState = BlockState.SettingChoiceData;
                    return;
            }
        }
        else if (blockState == BlockState.AddingChoice)
        {
            blockState = BlockState.SettingChoiceText; // implicitly only allowed at 0 or 2 indents. including flags and being at other indents may cause issues
            return;
        }
    }
    private int GetIndentation(string line)
    {
        int count = 0;

        foreach (char c in line)
        {
            if (c == ' ')
                count ++;
            else if (c == '\t')
                count += 4;
            else
                break;
        }
        return count / 4;
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