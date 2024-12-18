using UnityEngine;
using UnityEngine.UI;

public class TrySettingManager : MonoBehaviour
{
    public GameObject trySettingsPanel; // Try 씬의 설정창 패널
    public Slider volumeSlider; // 배경음악 볼륨 조절 슬라이더
    public Slider effectVolumeSlider; // 효과음 볼륨 조절 슬라이더
    public Slider masterVolumeSlider; // 마스터 볼륨 슬라이더

    public SettingsManager settingsManager;
    
    // 설정 패널을 열 때 호출됩니다.
    public void OpenTrySettingsPanel()
    {
        if (trySettingsPanel != null)
        {
            trySettingsPanel.SetActive(true);
            if (AudioManager.Instance != null)
            {
                // 슬라이더 값을 현재 AudioManager의 볼륨 값으로 설정
                SetupVolumeSlider(volumeSlider, AudioManager.Instance.bgmSource, AudioManager.Instance.SetBgmVolume);
                SetupVolumeSlider(effectVolumeSlider, AudioManager.Instance.effectSource, AudioManager.Instance.SetEffectVolume);
                SetupMasterVolumeSlider(masterVolumeSlider, AudioManager.Instance._currentMasterVolume, AudioManager.Instance.SetMasterVolume);
            }
            else
            {
                Debug.LogError("AudioManager instance is not found!");
            }
        }
        else
        {
            Debug.LogError("MainSettingsPanel reference is not set in the Inspector!");
        }
    }

    // 설정 패널을 닫을 때 호출됩니다.
    public void CloseTrySettingsPanel()
    {
        if (trySettingsPanel != null)
        {
            trySettingsPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Try Settings Panel is not assigned in the inspector!");
        }
    }

    // 슬라이더를 초기화하는 메소드
    // 슬라이더를 초기화하는 메소드
    private void InitializeSliders()
    {
        //Debug.Log("Initializing sliders...");
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager instance is not found!");
            return;
        }

        SetupVolumeSlider(volumeSlider, AudioManager.Instance.bgmSource, AudioManager.Instance.SetBgmVolume);
        SetupVolumeSlider(effectVolumeSlider, AudioManager.Instance.effectSource, AudioManager.Instance.SetEffectVolume);
    }



    // 슬라이더 설정 메소드
    private void SetupVolumeSlider(Slider slider, AudioSource source, UnityEngine.Events.UnityAction<float> listener)
    {
        if (slider == null)
        {
            Debug.LogError("Slider is not assigned in the inspector! Make sure to assign the slider in the Unity Inspector.");
            return;
        }

        if (source == null)
        {
            Debug.LogError("AudioSource is not assigned in AudioManager! Make sure the audio source is correctly set.");
            return;
        }

        //Debug.Log("Adding listener to slider: " + slider.gameObject.name);
        slider.value = source.volume;
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(listener);
    }

    private void SetupMasterVolumeSlider(Slider slider, float volume, UnityEngine.Events.UnityAction<float> listener)
    {
        if (slider == null)
        {
            Debug.LogError("MasterVolumeSlider is not assigned in the inspector!");
            return;
        }

        slider.value = volume;
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(listener);
    }

}
