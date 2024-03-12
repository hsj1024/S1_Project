using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource bgmSource; // 배경 음악을 재생할 오디오 소스
    public static AudioManager Instance; // 싱글톤 인스턴스

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 변경되어도 오브젝트 파괴 방지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ToggleBGM(bool isOn)
    {
        // 배경 음악 ON/OFF
        bgmSource.mute = !isOn;
    }

    public void SetVolume(float volume)
    {
        // 볼륨 조절
        bgmSource.volume = volume;
    }
}
