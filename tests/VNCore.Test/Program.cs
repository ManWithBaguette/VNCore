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
RunTests();

void RunTests()
{
    Test_01();
    Test_02();
}

void Test_01()
{
    InitializeTest(1);
    while (manager.DialogueState != State.End)
    {
        DialogueManager.DialogueResult result = manager.Advance();
        if (!result.Success)
        {   
            Console.WriteLine("\nTest_01 Failure");
            Console.WriteLine(result.ErrorMessage);
            return;
        }
    }
    Console.WriteLine("\nTest_01 Success");
}

void Test_02()
{
    InitializeTest(2);
    while (manager.DialogueState != State.End)
    {
        DialogueManager.DialogueResult result = manager.Advance();
        if (!result.Success)
        {   
            Console.WriteLine("\nTest_02 Failure");
            Console.WriteLine(result.ErrorMessage);
            return;
        }
    }
    Console.WriteLine("\nTest_01 Success");
}

void InitializeTest(int test)
{
    string scriptPath;
    string scenePath;
    switch (test)
    {
        case 1:
            Console.WriteLine("Running Test_01\n");
            scriptPath = "tests/scripts/01_basic_dialogue.txt";
            scenePath = "tests/scenes/01";
            break;

        case 2:
            Console.WriteLine("Running Test_02\n");
            scriptPath = "tests/scripts/02_line_metadata.txt";
            scenePath = "tests/scenes/02";
            break;

        case 3:
            Console.WriteLine("Running Test_03\n");
            scriptPath = "tests/scripts/03_choices_and_choice_data.txt";
            scenePath = "tests/scenes/03";
            break;

        default:
            Console.WriteLine("Invalid Test");
            return;
    }

    ScriptCompiler.CompileResult compileResult = compiler.Compile(scriptPath, scenePath);
    DialogueManager.DialogueResult dialogueResult = manager.LoadScene(scenePath);

    if (!compileResult.Success)
    {
        Console.WriteLine($"{compileResult.ErrorMessage}");
    }
    if (!dialogueResult.Success)
    {
        Console.WriteLine($"{dialogueResult.ErrorMessage}");
    }
}