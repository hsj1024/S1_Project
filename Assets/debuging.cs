using UnityEngine;
using UnityEngine.UI;

public class debugging : MonoBehaviour
{
    public static debugging Instance { get; private set; }
    public Text debugText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
            debugText.text += message + "\n";
        }
    }
}
