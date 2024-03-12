using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource bgmSource; // ��� ������ ����� ����� �ҽ�
    public static AudioManager Instance; // �̱��� �ν��Ͻ�

    private void Awake()
    {
        // �̱��� ���� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ���� ����Ǿ ������Ʈ �ı� ����
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ToggleBGM(bool isOn)
    {
        // ��� ���� ON/OFF
        bgmSource.mute = !isOn;
    }

    public void SetVolume(float volume)
    {
        // ���� ����
        bgmSource.volume = volume;
    }
}
