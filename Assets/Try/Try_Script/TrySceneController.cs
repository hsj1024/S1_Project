using UnityEngine;
using UnityEngine.UI;

public class TrySceneController : MonoBehaviour
{
    public Slider bgmVolumeSlider; // 배경음악 볼륨 조절 슬라이더
    public Slider effectVolumeSlider; // 효과음 볼륨 조절 슬라이더
    public Slider masterVolumeSlider; // 마스터 볼륨 조절 슬라이더

    private AudioManager audioManager;

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager instance is not found!");
            return;
        }

        audioManager = AudioManager.Instance;

        // 배경음악 재생 설정
        audioManager.PlayTrySceneBGM(true);

        // 볼륨 슬라이더 값 설정
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.value = audioManager.BgmVolumeSlider.value;
            bgmVolumeSlider.onValueChanged.RemoveAllListeners();
            bgmVolumeSlider.onValueChanged.AddListener(audioManager.SetBgmVolume);
        }
        else
        {
            Debug.LogError("BGM volume slider is not assigned!");
        }

        if (effectVolumeSlider != null)
        {
            effectVolumeSlider.value = audioManager.EffectVolumeSlider.value;
            effectVolumeSlider.onValueChanged.RemoveAllListeners();
            effectVolumeSlider.onValueChanged.AddListener(audioManager.SetEffectVolume);
        }
        else
        {
            Debug.LogError("Effect volume slider is not assigned!");
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = audioManager.MasterVolumeSlider.value;
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.onValueChanged.AddListener(audioManager.SetMasterVolume);
        }
        else
        {
            Debug.LogError("Master volume slider is not assigned!");
        }
    }
}
