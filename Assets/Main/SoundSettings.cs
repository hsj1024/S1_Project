using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundSettings : MonoBehaviour
{
    public Slider volumeSlider; // 배경 음악 볼륨 조절 슬라이더
    public Slider buttonSoundSlider; // 버튼 소리 볼륨 조절 슬라이더
    public Slider masterVolumeSlider; // 마스터 볼륨 조절 슬라이더

    public AudioSource bgmSource; // 배경 음악을 재생할 AudioSource
    public AudioSource buttonSoundSource; // 버튼 소리를 재생할 AudioSource

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
        // 현재 씬에 따라 슬라이더의 필요 여부를 판단
        if (SceneManager.GetActiveScene().name == "Try")
        {
            // 슬라이더를 직접 찾기
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
        masterVolumeSlider.value = volume; // 마스터 볼륨 값 저장
        SetBgmVolume(volumeSlider.value);
        SetButtonSoundVolume(buttonSoundSlider.value);
    }
}
