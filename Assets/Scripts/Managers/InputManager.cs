using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    public InputReader Reader { get; private set; }

    private void Awake()
    {
        Reader = new InputReader();
    }

    private void OnEnable()
    {
        Reader.EnableEvents();
    }

    private void OnDisable()
    {
        Reader.DisableEvents();
    }
}
