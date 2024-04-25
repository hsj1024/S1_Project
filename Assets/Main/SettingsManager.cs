using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public GameObject mainSettingsPanel;  // 메인 씬의 설정창 패널
    public GameObject trySettingsPanel;   // Try 씬의 설정창 패널

    public Slider bgmVolumeSlider;           // 배경음악 볼륨 조절 슬라이더
    public Slider effectVolumeSlider;        // 효과음 볼륨 조절 슬라이더
    public Slider masterVolumeSlider;        // 마스터 볼륨 조절 슬라이더

    public AudioClip mainBgmSource;         // 메인 씬 배경 음악
    public AudioClip tryBgmSource;          // Try 씬 배경 음악
    public AudioSource bgmSource;           // 배경 음악을 재생할 AudioSource

    public AudioSource effectSource;        // 효과음을 재생할 AudioSource

    public AudioManager audioManager;       // AudioManager 참조
    public TrySettingManager trySettingManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);  // 중복 인스턴스 제거
        }

        // 배경 음악을 재생할 AudioSource 생성
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true; // 반복 재생 설정
    }
    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // 씬 로드 시 이벤트 등록
    }

    private void SetupVolumeSlider()
    {
        // 배경음악 볼륨 슬라이더 설정
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.value = audioManager.BgmVolume;
            bgmVolumeSlider.onValueChanged.RemoveAllListeners();
            bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
        }

        // 효과음 볼륨 슬라이더 설정
        if (effectVolumeSlider != null)
        {
            effectVolumeSlider.value = audioManager.EffectVolume;
            effectVolumeSlider.onValueChanged.RemoveAllListeners();
            effectVolumeSlider.onValueChanged.AddListener(SetEffectVolume);
        }

        // 마스터 볼륨 슬라이더 설정
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = audioManager.MasterVolume;
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }
    }

    public void OpenSettingsPanel(bool isMainScene)
    {
        GameObject panelToUse = isMainScene ? mainSettingsPanel : trySettingsPanel;
        if (panelToUse != null)
        {
            panelToUse.SetActive(true); // 패널 활성화
            SetupVolumeSlider(); // 슬라이더 설정
        }
        else
        {
            Debug.LogError($"Panel not found for the scene {(isMainScene ? "Main" : "Try")}");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Try 씬인 경우 TrySettingManager를 찾아 설정 수행
        if (scene.name == "Try")
        {
            trySettingManager = FindObjectOfType<TrySettingManager>();
            if (trySettingManager != null)
            {
                // TrySettingManager에서 슬라이더 찾아 설정
                bgmVolumeSlider = trySettingManager.volumeSlider;
                effectVolumeSlider = trySettingManager.effectVolumeSlider;
                masterVolumeSlider = trySettingManager.masterVolumeSlider;

                // 슬라이더 설정
                SetupVolumeSlider();
            }
            else
            {
                Debug.LogError("TrySettingManager not found in the scene!");
            }
        }
    }


    public void SetBgmVolume(float volume)
    {
        audioManager.BgmVolume = volume;
    }

    public void SetEffectVolume(float volume)
    {
        audioManager.EffectVolume = volume;
    }

    public void SetMasterVolume(float volume)
    {
        audioManager.MasterVolume = volume;
    }

    public void SwitchToMainBGM()
    {
        bgmSource.clip = mainBgmSource;
        bgmSource.Play();
    }

    public void SwitchToTryBGM()
    {
        bgmSource.clip = tryBgmSource;
        bgmSource.Play();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;  // 오브젝트 파괴 시 이벤트 해제
    }
}
