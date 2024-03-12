using UnityEngine;
using UnityEngine.UI; // UI 네임스페이스를 사용하기 위해 필요

public class SoundSettings : MonoBehaviour
{
    public Slider volumeSlider; // 볼륨 조절 슬라이더
    public AudioSource bgmSource; // 배경 음악을 재생할 AudioSource

    void Start()
    {
        // 초기 볼륨 값을 슬라이더의 값으로 설정
        volumeSlider.value = bgmSource.volume;
        // 슬라이더 값이 변할 때 호출될 메서드를 추가
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        // AudioSource의 볼륨을 슬라이더의 값으로 설정
        bgmSource.volume = volume;
    }
}
