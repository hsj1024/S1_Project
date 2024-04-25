using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundSettings : MonoBehaviour
{
    public Slider volumeSlider; // ��� ���� ���� ���� �����̴�
    public Slider buttonSoundSlider; // ��ư �Ҹ� ���� ���� �����̴�
    public Slider masterVolumeSlider; // ������ ���� ���� �����̴�

    public AudioSource bgmSource; // ��� ������ ����� AudioSource
    public AudioSource buttonSoundSource; // ��ư �Ҹ��� ����� AudioSource

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        InitializeSliders();
    }


    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Try")
        {
            InitializeSliders();
        }
    }


    private void InitializeSliders()
    {
        // ���� ���� ���� �����̴��� �ʿ� ���θ� �Ǵ�
        if (SceneManager.GetActiveScene().name == "Try")
        {
            // �����̴��� ���� ã��
            GameObject volumeSliderGO = GameObject.Find("VolumeSlider");
            if (volumeSliderGO != null)
            {
                volumeSlider = volumeSliderGO.GetComponent<Slider>();
                volumeSlider.onValueChanged.AddListener(SetBgmVolume);
            }
            else
            {
                Debug.LogError("Volume slider is not found.");
            }

            GameObject buttonSoundSliderGO = GameObject.Find("EffectVolumeSlider");
            if (buttonSoundSliderGO != null)
            {
                buttonSoundSlider = buttonSoundSliderGO.GetComponent<Slider>();
                buttonSoundSlider.onValueChanged.AddListener(SetButtonSoundVolume);
            }
            else
            {
                Debug.LogError("Button sound slider is not found.");
            }

            GameObject masterVolumeSliderGO = GameObject.Find("MasterVolumeSlider");
            if (masterVolumeSliderGO != null)
            {
                masterVolumeSlider = masterVolumeSliderGO.GetComponent<Slider>();
                masterVolumeSlider.SetValueWithoutNotify(1f);
                masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            }
            else
            {
                Debug.LogError("Master volume slider is not found.");
            }
        }
    }


    public void SetBgmVolume(float volume)
    {
        bgmSource.volume = volume;
    }

    public void SetButtonSoundVolume(float volume)
    {
        buttonSoundSource.volume = volume;
    }

    public void SetMasterVolume(float volume)
    {
        masterVolumeSlider.value = volume; // ������ ���� �� ����
        SetBgmVolume(volumeSlider.value);
        SetButtonSoundVolume(buttonSoundSlider.value);
    }
}
