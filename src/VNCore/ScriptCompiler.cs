using System.Text.Json;
using VNCore.Dialogue;

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

public enum CompileError
{
    None,

}
public class ScriptCompiler
{
    private Dictionary<int, Dictionary<int, DialogueLine>> scene = [];
    private Queue<string> fileQueue = new();
    private List<string> compiledFiles = new();

    private int branchId;
    private int lineId;
    private int choiceId;

    private string lineText = "";
    private string lineFlag = "";
    private string lineParameter = "";

    private Dictionary<int, DialogueLine> lines = new Dictionary<int, DialogueLine>();
    private MainState mainState;
    private BlockState blockState;

    private readonly List<String> CoreDataFlags = [
        "switch",
        "switch-condition",
        "condition",
        "precondition",
        "postcondition",
    ];

    public record CompileResult (
        bool Success,
        string? ErrorMessage = null,
        CompileError? Error = CompileError.None
    );

    public CompileResult Compile(string filepath, string outputPath)
    {
        Reset();

        try
        {
            
            fileQueue.Enqueue(filepath);
            compiledFiles.Add(filepath);

            while (fileQueue.Count > 0)
            {   
                mainState = MainState.None;
                blockState = BlockState.None;
                lines = new Dictionary<int, DialogueLine>();
                branchId ++;
                lineId = -1;
                scene[branchId] = lines;
            
                foreach (var line in File.ReadLines(fileQueue.Dequeue()))
                {
                    
                    int indentation = GetIndentation(line);
                    string trimmed = line.Trim();

                    UpdateMainState(trimmed, indentation);


                    switch (mainState)
                    {
                        case MainState.SettingSpeaker:
                            SetSpeaker();
                            break;

                        case MainState.SettingText: 
                            SetText(trimmed);
                            break;

                        case MainState.SettingLineData:
                            switch (blockState)
                            {
                                case BlockState.SettingMetadata:
                                    SetData();
                                    break;
                                case BlockState.SettingChoice:
                                    choiceId = 0;
                                    break;
                                case BlockState.AddingChoice:
                                    AddChoice();
                                    break;
                                case BlockState.SettingChoiceText:
                                    SetText(trimmed);
                                    break;
                                case BlockState.SettingChoiceData:
                                    SetData();
                                    break;
                            }
                            break;
                    }
                }
            }
            string json = JsonSerializer.Serialize(scene, JsonContext.Default.DictionaryInt32DictionaryInt32DialogueLine);
            if (!outputPath.ToLower().EndsWith(".json"))
            {
                outputPath += ".json";
            }
            File.WriteAllText(outputPath, json);
            return new CompileResult(true);
        }
        catch (ScriptCompileException ex)
        {
            return new CompileResult(false, ex.Message, ex.Error);
        }
        catch (IOException ex)
        {
            return new CompileResult(false, $"File I/O error: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            return new CompileResult(false, $"File access error: {ex.Message}");
        }
    }

    private void SetText(string line)
    {
        if (line.StartsWith("$\""))
        {
            if (!lineText.StartsWith("$\""))
            {
                lineText = "$\"" + lineText;
            }
            line = line[2..];
        }

        lineText += (lineText.Length > 0 ? " " : "") + line;

        if (mainState == MainState.SettingText)
        {
            scene[branchId][lineId].Text = lineText;
        }
        else if (blockState == BlockState.SettingChoiceText)
        {
            scene[branchId][lineId].Choices[choiceId].Text = lineText;
        }

    }

    private void SetSpeaker()
    {
        lineId ++;
        lines[lineId] = new DialogueLine {Speaker = lineFlag.Trim()};
        lineText = lineParameter.Trim();
        scene[branchId][lineId].Text = lineText;
    }

    private void AddChoice()
    {
        choiceId ++;
        scene[branchId][lineId].Choices[choiceId] = new DialogueElement();
        lineText = lineParameter.Trim();
        scene[branchId][lineId].Choices[choiceId].Text = lineText;
    }

    private void SetData()
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

            if (blockState == BlockState.SettingMetadata)
                scene[branchId][lineId].VNCoreData[lineFlag] = switchTarget;
            else if (blockState == BlockState.SettingChoiceData)
                scene[branchId][lineId].Choices[choiceId].VNCoreData[lineFlag] = switchTarget;

            return;
        }

        if (isCoreData)
        {
            if (blockState == BlockState.SettingMetadata)
            {
                scene[branchId][lineId].VNCoreData[lineFlag] = lineParameter.Trim();
                Console.WriteLine("LineData Set");
            }
            else if (blockState == BlockState.SettingChoiceData)
            {
                scene[branchId][lineId].Choices[choiceId].VNCoreData[lineFlag] = lineParameter.Trim();
                Console.WriteLine("ChoiceData Set");
            }
        } else
        {
            if (blockState == BlockState.SettingMetadata)
            {
                scene[branchId][lineId].Metadata[lineFlag] = lineParameter.Trim();
                Console.WriteLine("LineData Set");
            }
            else if (blockState == BlockState.SettingChoiceData)
            {
                scene[branchId][lineId].Choices[choiceId].Metadata[lineFlag] = lineParameter.Trim();
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

    private void Reset()
    {
        scene = new Dictionary<int, Dictionary<int, DialogueLine>>();
        fileQueue = new Queue<string>();
        compiledFiles = new List<string>();

        branchId = -1;
        lineId = -1;
        choiceId = 0;

        lineText = "";
        lineFlag = "";
        lineParameter = "";

        lines = new Dictionary<int, DialogueLine>();

        mainState = MainState.None;
        blockState = BlockState.None;
    }

    private class ScriptCompileException : Exception
    {
        public CompileError Error { get; }
        private string currentFile = "";
        private int currentLine;
        public ScriptCompileException(CompileError error, string message) : base(message)
        {
            Error = error;
        }
    }
}