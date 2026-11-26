# DialogueManager

### Namespace: `VNCore.DialogueManager`

`DialogueManager` manages the runtime execution of compiled visual novel dialogue scenes.

It loads compiled JSON scene data, maintains the current dialogue state, and controls progression through dialogue lines and player choices. 
It also handles conditional dialogue and choices through SaveData, processes variable interpolation in formatted strings, and supports switching between dialogue branches.

The manager provides events that allow other systems, such as the game's UI, to respond to scene loading, dialogue changes, choice updates, and dialogue completion.

## Public API:
```
public DialogueManager();
public DialogueManager(SaveData saveData);

public DialogueResult LoadScene(string path);
public DialogueResult Advance();
public DialogueResult SelectChoice(int num);
public void UnloadScene();
public void SetLine(int branch, int line);
public void SetSaveData(SaveData save);
public void RemoveSaveData();
```

### Constructors
| Constructor | Description |
|---|---|
| DialogueManager() | Creates a DialogueManager with a new, default SaveData instance. |
| DialogueManager(SaveData saveData) | Creates a DialogueManager using the provided SaveData instance. |

### Methods
| Method | Description |
|---|---|
| LoadScene(string path) | Loads and validates a compiled JSON scene, then initializes dialogue execution at branch 0, line 0. The .json extension is automatically appended if not already present. Returns a DialogueResult. |
| Advance() | Advances dialogue execution according to the current dialogue state. Progresses to the next line, processes a selected choice, or ends the dialogue when appropriate. Returns a DialogueResult. |
| SelectChoice(int num) | Selects an available choice using its corresponding choice number. Returns a DialogueResult. |
| UnloadScene() | Unloads the current scene and resets the DialogueManager to its initial state. |
| SetLine(int branch, int line) | Sets the current dialogue position to the specified branch and line. |
| SetSaveData(SaveData save) | Replaces the currently assigned SaveData instance. |
| RemoveSaveData() | Replaces the current SaveData instance with a new, default instance. |

### Properties
| Property | Type | Description |
|---|---|---|
| CurrentLine | DialogueLine | The dialogue line currently being displayed. |
| Current Branch | int | The branch number of the current dialogue position. |
| CurrentLineNum | int | The line number of the current dialogue position. |
| DialogueState | State | The current state of dialogue execution. |
| Choices | IReadOnlyDictionary<int, DialogueElement> | The currently available player choices, indexed by their choice numbers. |
| CurrentChoice | DialogueElement? | The choice currently selected by the player, if any. |
| saveData | SaveData? | The save data instance used for conditional checks, variable interpolation, and metadata updates. |

### State
Represents the current execution state of the DialogueManager.
```
public enum State
{
    Unloaded,
    Dialogue,
    Choice,
    Last,
    End
}
```
| State | Description |
|---|---|
| Unloaded | No scene is currently loaded. |
| Dialogue | A dialogue line is currently active and can be advanced. |
| Choice | The current dialogue line contains available choices, requiring the player to select one before advancing. |
| Last | The end of the current dialogue branch has been reached. Advancing transitions the manager into the End state. |
| End | Dialogue execution has finished. |

### Events
The DialogueManager exposes events that allow external systems to respond to changes in dialogue execution.
| Event | Description |
|---|---|
| OnSceneLoaded | Invoked when a scene has been successfully loaded and initialized. |
| OnSceneUnloaded | Invoked when the current scene is unloaded. |
| OnDialogueChanged | Invoked when the active dialogue line changes. Provides the new DialogueLine. |
| OnChoicesUpdated | Invoked whenever the available choices are updated. Provides the updated choice dictionary. |
| OnDialogueEnded | Invoked when dialogue execution transitions into the End state. |

## DialogueResult
Represents the outcome of a DialogueManager operation.
```
public record DialogueResult(
    bool Success,
    string? ErrorMessage = null,
    DialogueError Error = DialogueError.None,
    int? Branch = null,
    int? Line = null
);
```
| Property | Type | Description |
|---|---|---|
| Success | bool | Whether the operation succeeded. |
| ErrorMessage | string? | Description of an error, if available. |
| Error | DialogueError | The category of error encountered. |
| Branch | int? | The branch associated with the error, if applicable. |
| Line | int? | The line associated with the error, if applicable. |

### DialogueError
Represents the different error categories that can occur during dialogue execution.
```
public enum DialogueError
{
    None,
    FileNotFound,
    FileAccessDenied,
    FileReadError,
    InvalidJson,
    InvalidScene,
    InvalidDestination,
    IllegalState,
    InvalidChoice
}
```
| Error | Description |
|---|---|
| None | No error occurred. |
| FileNotFound | The specified scene file or directory could not be found. |
| FileAccessDenied | Access to the specified scene file was denied. |
| FileReadError | An error occurred while reading the scene file. |
| InvalidJson | The scene file contains invalid JSON data or could not be deserialized. |
| InvalidScene | The loaded scene data does not satisfy the required scene structure. |
| InvalidDestination | A branch switch references an invalid or nonexistent destination. |
| IllegalState | An operation was attempted while the DialogueManager was in an incompatible state. |
| InvalidChoice | The specified choice is not currently available. |

## Usage Example:
```
DialogueManager manager = new DialogueManager();

DialogueManager.DialogueResult result =
    manager.LoadScene("Scenes/Chapter1");

if (!result.Success)
{
    Console.WriteLine(result.ErrorMessage);
    return;
}

manager.OnDialogueChanged += line =>
{
    Console.WriteLine($"{line.Speaker}: {line.Text}");
};

manager.OnChoicesUpdated += choices =>
{
    foreach (var choice in choices)
    {
        Console.WriteLine($"{choice.Key}: {choice.Value.Text}");
    }
};

// Advance dialogue
manager.Advance();

// Select a choice and advance
manager.SelectChoice(1);
manager.Advance();
```
