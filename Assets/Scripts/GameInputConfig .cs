using UnityEngine;

public class GameInputConfig : MonoBehaviour
{
    public static GameInputConfig Instance { get; private set; }

    private bool isTypingInInputField = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetInputFieldFocused(bool isFocused)
    {
        isTypingInInputField = isFocused;
    }

    public bool ShouldBlockInput()
    {
        return isTypingInInputField;
    }
}
