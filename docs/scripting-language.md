# Scripting Language

VNCore provides a custom, human-readable scripting format for defining visual novel dialogue.

Scripts are written using speaker declarations, dialogue text, metadata, and choice blocks.

The ScriptCompiler converts these scripts into structured JSON scene data, which is subsequently loaded and processed by DialogueManager.

## Basic Structure

A basic dialogue script consists of speaker declarations followed by dialogue text.

Example:
```
Nelson: Hi!

Bobby: Whats up?

Nelson: Nothing.
```
Each dialogue declaration begins with a speaker name followed by a colon.

The text following the colon becomes the initial dialogue text, but can also be left empty, starting dialogue text on the next line.

Additional lines can be used to continue the dialogue text.

## Speakers
A speaker declaration uses the following format:
`SpeakerName:`

The speaker can also be left empty. 

Lines are separated by spaces, and a new line is marked by the colon used for speakers

Example:
```
Nelson: Hello!

Bobby: Welcome.

: If the current line is a narration, the speaker name can be left empty instead of adding 'Narrator' as the speaker.
```
The speaker name and initial dialogue text are extracted from the declaration if present.

Speaker names are stored as strings.

## Dialogue Text

Dialogue text can span multiple physical lines.
```
Nelson: Hi.
This is a continuation of the dialogue.
dialogue can be written across multiple lines.
```
The compiler joins these lines using spaces.

The resulting dialogue is stored as a single text string.

## Indentation

VNCore uses indentation to distinguish dialogue metadata, choice blocks, and choice metadata.

Each indentation level represents four spaces.

Tabs are also counted as one indentation level.

| Indentation | Purpose |
|---|---|
| Level 0 | Speaker declarations and dialogue text. |
| Level 1 | Dialogue metadata and choice block declarations. |
| Level 2 | Individual choice declarations. |
| Level 3 | Choice metadata. |

Example:
```
Nelson: Hello.
    x: +5
    choice:
        choice-1: Go home.
            y: +10
        choice-2: Nothing.
            x: - 5
```

## Metadata

Metadata allows dialgoue lines and choices to modify runtime variables or specify additional behavior.

Metadata is written using:
`    key: value`
Metadata is associated with the dialogue line or choice it is nested under.

### Custom Metadata

Custom metadata keys are stored in the element's Metadata dictionary.
```
Nelson: I am sad.
    x: +10
    y: -5
```
When DialogueManager advances past this dialogue line, the metadata is applied to SaveData.

The values are interpreted by SaveData, allowing direct assignment and arithmetic operations.

### Arithmetic Metadata
Metadata supports arithmetic operations through SaveData.
```
Alice: I appreciate your help.
    trust: +10
```

Supported operations:
| Operator | Operation |
|---|---|
| + | Addition |
| - | Subtraction |
| * | Multiplication |
| / | Division |

For example:
```
Nelson: That was unfortunate.
    anger: +5
    trust: -10
```

## Choices
Choices allow players to select between different dialogue options.

A choice block is declared using `choice:`.

Individual choices are declared using `choice-` followed by an identifier.

Example:
```
Nelson: What would you like to do?
    choice:
        choice-1: Explore the forest.
        choice-2: Visit the town.
        choice-3: Return home.
```
Choices are stored as numbered elements and exposed through DialogueManager's Choices property.

The player selects a choice using its identifier:
```
manager.SelectChoice(1);
manager.Advance();
```

### Choice Metadata
Choices can contain their own metadata.

Metadata must be indented one level further than the choice declaration.
```
Alice: What should we do?
    choice:
        choice-1: Apologize.
            trust: +10
        choice-2: Ignore her.
            anger: +10
```
Only the selected choice's metadata is applied when advancing dialogue.

### Choice Text
Choice text supports multiline content.
```
Alice: What do you think?
    choice:
        choice-1: I think we should
        explore the forest together.
```
The compiler joins the continuation text using spaces.

## Core Data
VNCore has a collection of reserved core data flags used to define special dialogue behavior.

The compiler recognizes:
| Flag | Description |
|---|---|
| switch | Specifies a dialogue destination. |
| precondition | Defines a condition that must be satisfied for an element to be available. |
| switch-condition | Reserved core data flag. |
| condition | Reserved core data flag. |
| postcondition | Reserved core data flag. |

Core data is stored separately from custom metadata.

### Preconditions
Preconditions determine whether dialogue or choices are available.
```
Alice: You seem upset.
    precondition: anger >= 10
```
If the condition evaluates to false, the dialogue line is skipped.

Preconditions can also be applied to choices:
```
Alice: What would you like?
    choice:
        choice-1: Apologize.
            precondition: anger >= 10

        choice-2: Continue talking.
            precondition: anger < 10
```
Only choices whose preconditions evaluate to true are made available.

Preconditions use SaveData's comparison functionality.

See [Save System](save-system.md) for supported comparison expressions.

## Branching and Switching
VNCore supports branching through the switch core data flag.

Switch instructions change the current dialogue position.

Switching to a Line in the Initial Script

A numeric switch destination refers to a line in branch 0.
```
Alice: Let's start over.
    switch: 5
```
This switches to branch `0`, line `5`.

### Switching to Another Script
A script can reference another script file through its filename.
```
Alice: Let's go somewhere else.
    switch: OtherScene.txt
```

The compiler queues the referenced script for compilation and assigns it a branch identifier.

The switch destination resolves to line `0` of that branch.

### Specifying a Script and Line
The path and line parameters can be used together.
```
Alice: Let's continue elsewhere.
    switch: path:OtherScene.txt line:5
```
This switches to line `5` of the referenced script.

### Switching Within the Current Branch
A line can also be specified without providing a path.
```
Alice: Let's revisit that moment.
    switch: line:5
```
This targets line 5 of the current branch.
### Branch Destination Format
Internally, compiled switch destinations use the format:
`branch:line`
For example:
```
0:5
1:0
2:10
```
These values represent the target branch and line index.

## Dynamic Text
VNCore supports variable interpolation using FString-style syntax.

Dialogue or choice text beginning with $" is treated as dynamic text.

Example:
```
Narrator: $"Your current anger is {anger}.
```
If the SaveData dictionary contains:
```
anger = 20
```
The resulting dialogue text becomes:
```
Your current anger is 20.
```
Multiple variables can be interpolated:
```
Narrator: $"Hello {player_name}, you have {money} coins.
```
Dynamic text is processed when the dialogue line or choice is loaded.

Variables that cannot be resolved are left unchanged.

Note: Unlike standard FString-style syntax, the text should not be closed with `"`.

## Complete Example
The following example combines dialogue, metadata, choices, conditions, and dynamic text.
```
Narrator: Welcome to the story.

Alice: $"Hello {player_name}! How are you?"

Bob: I have something to ask you.

Alice: What would you like to do?
    choice:
        choice-1: Apologize.
            trust: +10
            anger: -5

        choice-2: Ignore him.
            anger: +10

        choice-3: $"Offer him {money} coins."
            precondition: money >= 10
            money: -10
            trust: +5

Narrator: $"Your current trust is {trust}."

Alice: I see.
    precondition: trust >= 10

Bob: Perhaps we should start over.
    switch: line:0
```
## Compilation
Scripts are compiled using ScriptCompiler.
```
ScriptCompiler compiler = new ScriptCompiler();

CompileResult result = compiler.Compile(
    "Scripts/Intro.vn",
    "Compiled/Intro.json"
);
```
The compiler converts the source script into JSON scene data.

The resulting JSON contains branches, dialogue lines, choices, metadata, and core data.

DialogueManager loads this compiled data at runtime.

## Syntax Guidelines
- Use a colon to separate declarations from their values.
- Use four spaces per indentation level.
- Keep metadata nested under its associated dialogue line or choice.
- Declare choices inside a `choice:` block.
- Use `choice-` declarations for individual choices.
- Separate dialogue elements with blank lines.
- Use `precondition` for conditional dialogue and choices.
- Use `switch` to change dialogue destinations.
- Use `$"...` for dynamic text interpolation.
