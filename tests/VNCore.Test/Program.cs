using VNCore;
using VNCore.Compiler;
using VNCore.Dialogue;
using VNCore.DialogueManager;

// var compiler = new ScriptCompiler();

// compiler.Compile("test-data/refactorTest.txt", "refactor");

// Console.WriteLine("Compilation complete!");
// int choice = 0;
// var saveManager = new SaveManager();
// saveManager.LoadSave("");
// var dialogueManager = new DialogueManager(saveManager.Current);
// // var dialogueManager = new DialogueManager();
// DialogueManager.DialogueResult result = dialogueManager.LoadScene("scenes/refactor.json");
// while (dialogueManager.DialogueState != State.End) //Test code. Later probably simplify use of the dialogue manager.
// {
//     if (dialogueManager.DialogueState != State.Last)
//     {
//         if (!result.Success)
//         {
//             Console.WriteLine(result.ErrorMessage);
//             Console.WriteLine($"Error: {result.Error}");
//         }

//         choice = 0;
        
//         Console.WriteLine($"Speaker: {dialogueManager.CurrentLine.Speaker}\nLine: {dialogueManager.CurrentLine.Text}\n");
//         if (dialogueManager.Choices.Count > 0)
//         {
//             foreach (var element in dialogueManager.Choices)
//             {
//                 Console.WriteLine($"Choice {element.Key} Text: {element.Value.Text}");
//             }
//         }
//     }
//     if (int.TryParse(Console.ReadLine(), out int value))
//     {
//         choice = value;
//     }

//     if (choice != 0)
//     {
//         dialogueManager.SelectChoice(choice);
//     }
//     if (dialogueManager.Choices.Count == 0 || dialogueManager.CurrentChoice != null)
//         dialogueManager.Advance(); // Change, still runs if choice
    
// }
// saveManager.WriteSave();

var compiler = new ScriptCompiler();
var manager = new DialogueManager();
int exitCode = RunTests();

return exitCode;

int RunTests()
{
    if (!Test_01())
        return 1;
    if (!Test_02())
        return 1;

    return 0;
}

bool Test_01()
{
    if (!InitializeTest(1))
        return false;
    
    while (manager.DialogueState != State.End)
    {
        DialogueManager.DialogueResult result = manager.Advance();
        if (!result.Success)
        {   
            Console.WriteLine("\nTest_01 Failure");
            Console.WriteLine(result.ErrorMessage);
            return false;
        }
    }
    Console.WriteLine("Test_01 Success");
    return true;
}

bool Test_02()
{
    if (!InitializeTest(2))
        return false;

    while (manager.DialogueState != State.End)
    {
        DialogueManager.DialogueResult result = manager.Advance();
        if (!result.Success)
        {   
            Console.WriteLine("\nTest_02 Failure");
            Console.WriteLine(result.ErrorMessage);
            return false;
        }
    }
    Console.WriteLine("Test_02 Success");
    return true;
}

bool InitializeTest(int test)
{
    string scriptPath;
    string scenePath;
    switch (test)
    {
        case 1:
            scriptPath = "tests/scripts/01_basic_dialogue.txt";
            scenePath = "tests/scenes/01";
            break;

        case 2:
            scriptPath = "tests/scripts/02_line_metadata.txt";
            scenePath = "tests/scenes/02";
            break;

        case 3:
            scriptPath = "tests/scripts/03_choices_and_choice_data.txt";
            scenePath = "tests/scenes/03";
            break;

        default:
            Console.WriteLine("Invalid Test");
            return false;
    }

    Console.WriteLine($"\nRunning Test_0{test}");

    ScriptCompiler.CompileResult compileResult = compiler.Compile(scriptPath, scenePath);

    if (!compileResult.Success)
    {
        Console.WriteLine($"{compileResult.ErrorMessage}");
        return false;
    }

    DialogueManager.DialogueResult dialogueResult = manager.LoadScene(scenePath);
    
    if (!dialogueResult.Success)
    {
        Console.WriteLine($"{dialogueResult.ErrorMessage}");
        return false;
    }
    return true;
}