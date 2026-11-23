using VNCore;
using VNCore.Compiler;
using VNCore.Dialogue;
using VNCore.DialogueManager;

int MAX_ITERATIONS = 100;

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
    if (!Test_03())
        return 1;
    if (!Test_04())
        return 1;
    if (!Test_05())
        return 1;
    if (!Test_06())
        return 1;
    if (!Test_07())
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

bool Test_03()
{
    if (!InitializeTest(3))
        return false;
    
    while (manager.DialogueState != State.End)
    {
        DialogueManager.DialogueResult result = manager.Advance();
        if (manager.DialogueState == State.Choice)
        {
            result = IterateThroughChoice();
        }
        if (!result.Success)
        {   
            Console.WriteLine("\nTest_03 Failure");
            Console.WriteLine(result.ErrorMessage);
            return false;
        }
    }
    Console.WriteLine("Test_03 Success");
    return true;
}

bool Test_04()
{
    if (!InitializeTest(4))
        return false;
    
    while (manager.DialogueState != State.End)
    {
        DialogueManager.DialogueResult result = manager.Advance();
        if (manager.DialogueState == State.Choice)
        {
            result = IterateThroughChoice();
        }
        if (!result.Success)
        {   
            Console.WriteLine("\nTest_04 Failure");
            Console.WriteLine(result.ErrorMessage);
            return false;
        }
    }
    Console.WriteLine("Test_04 Success");
    return true;
}

bool Test_05()
{
    if (!InitializeTest(5))
        return false;
    
    while (manager.DialogueState != State.End)
    {
        DialogueManager.DialogueResult result = manager.Advance();
        if (!result.Success)
        {   
            Console.WriteLine("\nTest_05 Failure");
            Console.WriteLine(result.ErrorMessage);
            return false;
        }
    }
    Console.WriteLine("Test_05 Success");
    return true;
}

bool Test_06()
{
    if (!InitializeTest(6))
        return false;
    
    while (manager.DialogueState != State.End)
    {
        DialogueManager.DialogueResult result = manager.Advance();
        if (!result.Success)
        {   
            Console.WriteLine("\nTest_06 Failure");
            Console.WriteLine(result.ErrorMessage);
            return false;
        }
    }
    Console.WriteLine("Test_06 Success");
    return true;
}

bool Test_07()
{
    if (!InitializeTest(7))
        return false;
    
    // manager = new DialogueManager(new SaveData());
    // manager.LoadScene("tests/scenes/07.json");
    while (manager.DialogueState != State.End)
    {
        Console.WriteLine(manager.CurrentLine.Text);
        DialogueManager.DialogueResult result = manager.Advance();
        if (!result.Success)
        {   
            Console.WriteLine("\nTest_07 Failure");
            Console.WriteLine(result.ErrorMessage);
            return false;
        }
    }
    Console.WriteLine("Test_07 Success");
    return true;
}

DialogueManager.DialogueResult IterateThroughChoice()
{
    int choiceBranch = manager.CurrentBranch;
    int choiceLine = manager.CurrentLineNum;
    int choiceCount = manager.Choices.Count;
    var choiceKeys = manager.Choices.Keys.ToList();

    foreach (int choiceKey in choiceKeys)
    {
        int iterations = 0;

        manager.SetLine(choiceBranch, choiceLine);
        DialogueManager.DialogueResult selection = manager.SelectChoice(choiceKey);

        if (!selection.Success)
            return selection;

        while (manager.DialogueState != State.End)
        {
            if (iterations++ > MAX_ITERATIONS)
            {
                throw new Exception($"Dialogue path failed to terminate:\nBranch: {choiceBranch}\nLine: {choiceLine}\nChoice Number: {choiceKey}");
            }
            manager.Advance();
            if (manager.DialogueState == State.Choice)
            {
                DialogueManager.DialogueResult result = IterateThroughChoice();

                if (!result.Success)
                    return result;
            }
        }
        Console.WriteLine($"{choiceBranch}:{choiceLine} Choice: {choiceKey} Successfully reached end");
    }
    return new DialogueManager.DialogueResult(true);
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

        case 4:
            scriptPath = "tests/scripts/04_multiline_choice_text.txt";
            scenePath = "tests/scenes/04";
            break;

        case 5:
            scriptPath = "tests/scripts/05_fstring_text.txt";
            scenePath = "tests/scenes/05";
            break;

        case 6:
            scriptPath = "tests/scripts/06_branch_main.txt";
            scenePath = "tests/scenes/06";
            break;

        case 7:
            scriptPath = "tests/scripts/07_savedata.txt";
            scenePath = "tests/scenes/07";
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