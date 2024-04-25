using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundManager : MonoBehaviour
{
    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlayButtonClickSound);
        }
    }

    public void PlayButtonClickSound()
    {
        AudioManager.Instance.PlayButtonClickSound();
    }
}
