using TMPro;
using UnityEngine;

public class DebugDisplay : MonoBehaviour
{
    public static DebugDisplay Instance { get; private set; }
    public TextMeshProUGUI debugText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Log(string message)
    {
        if (debugText != null)
        {
            debugText.text += message + "  ";
        }
    }
}
