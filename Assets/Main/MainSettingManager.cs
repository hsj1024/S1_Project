using UnityEngine;
using UnityEngine.UI;

public class MainSettingManager : MonoBehaviour
{
    public GameObject mainSettingsPanel; // ���� ���� ���� �г�
    public Slider volumeSlider; // ������� ���� ���� �����̴�
    public Slider effectVolumeSlider; // ȿ���� ���� ���� �����̴�
    public Slider masterVolumeSlider; // ������ ���� �����̴�

    private void Start()
    {
        // ��� ��ư�� ã�Ƽ� ButtonSoundManager�� ���
        Button[] buttons = FindObjectsOfType<Button>();


        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager instance is not found!");
            return;
        }

        // ������� ���� �����̴� ����
        SetupVolumeSlider(volumeSlider, AudioManager.Instance.bgmSource, AudioManager.Instance.SetBgmVolume);

        // ȿ���� ���� �����̴� ����
        SetupVolumeSlider(effectVolumeSlider, AudioManager.Instance.effectSource, AudioManager.Instance.SetEffectVolume);

        // ������ ���� �����̴�
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
            AudioManager.Instance.FindAndAssignSliders(); // �����̴� ����
        }
        else
        {
            Debug.LogError("MainSettingsPanel reference is not set in the Inspector!");
        }
    }

    public void CloseMainSettingsPanel()
    {
        // mainSettingsPanel�� null üũ
        if (mainSettingsPanel == null)
        {
            Debug.LogError("MainSettingsPanel reference is not set in the Inspector!");
            return;
        }
        mainSettingsPanel.SetActive(false);
    }
}
