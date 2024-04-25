using UnityEngine;
using UnityEngine.UI;

public class MainSettingManager : MonoBehaviour
{
    public GameObject mainSettingsPanel; // 메인 씬의 설정 패널
    public Slider volumeSlider; // 배경음악 볼륨 조절 슬라이더
    public Slider effectVolumeSlider; // 효과음 볼륨 조절 슬라이더
    public Slider masterVolumeSlider; // 마스터 볼륨 슬라이더

    private void Start()
    {
        // 모든 버튼을 찾아서 ButtonSoundManager에 등록
        Button[] buttons = FindObjectsOfType<Button>();


        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager instance is not found!");
            return;
        }

        // 배경음악 볼륨 슬라이더 설정
        SetupVolumeSlider(volumeSlider, AudioManager.Instance.bgmSource, AudioManager.Instance.SetBgmVolume);

        // 효과음 볼륨 슬라이더 설정
        SetupVolumeSlider(effectVolumeSlider, AudioManager.Instance.effectSource, AudioManager.Instance.SetEffectVolume);

        // 마스터 볼륨 슬라이더
        SetupMasterVolumeSlider(masterVolumeSlider, AudioManager.Instance._currentMasterVolume, AudioManager.Instance.SetMasterVolume);


    }
    private void SetupVolumeSlider(Slider slider, AudioSource source, UnityEngine.Events.UnityAction<float> listener)
    {
        if (slider == null)
        {
            Debug.LogError("Slider is not assigned in the inspector!");
            return;
        }

        if (source == null)
        {
            Debug.LogError("AudioSource is not assigned in AudioManager!");
            return;
        }

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

    public void OpenMainSettingsPanel()
    {
        if (mainSettingsPanel != null)
        {
            mainSettingsPanel.SetActive(true);
            AudioManager.Instance.FindAndAssignSliders(); // 슬라이더 설정
        }
        else
        {
            Debug.LogError("MainSettingsPanel reference is not set in the Inspector!");
        }
    }

    public void CloseMainSettingsPanel()
    {
        // mainSettingsPanel의 null 체크
        if (mainSettingsPanel == null)
        {
            Debug.LogError("MainSettingsPanel reference is not set in the Inspector!");
            return;
        }
        mainSettingsPanel.SetActive(false);
    }
}
