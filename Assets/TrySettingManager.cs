using UnityEngine;
using UnityEngine.UI;

public class TrySettingManager : MonoBehaviour
{
    public GameObject trySettingsPanel; // Try ���� ����â �г�
    public Slider volumeSlider; // ������� ���� ���� �����̴�
    public Slider effectVolumeSlider; // ȿ���� ���� ���� �����̴�
    public Slider masterVolumeSlider; // ������ ���� �����̴�

    public SettingsManager settingsManager;
    
    // ���� �г��� �� �� ȣ��˴ϴ�.
    public void OpenTrySettingsPanel()
    {
        if (trySettingsPanel != null)
        {
            trySettingsPanel.SetActive(true);
            if (AudioManager.Instance != null)
            {
                // �����̴� ���� ���� AudioManager�� ���� ������ ����
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

    // ���� �г��� ���� �� ȣ��˴ϴ�.
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

    // �����̴��� �ʱ�ȭ�ϴ� �޼ҵ�
    // �����̴��� �ʱ�ȭ�ϴ� �޼ҵ�
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



    // �����̴� ���� �޼ҵ�
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
