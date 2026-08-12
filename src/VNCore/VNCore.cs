namespace VNCore;

public class Engine
{
    public GameState State { get; }
    public DialogueManager Dialogue { get; }

    public Engine()
    {
        State = new GameState();
        Dialogue = new DialogueManager();
    }

    public void Start()
    {
        
    }

    public void Update()
    {
        
    }
}