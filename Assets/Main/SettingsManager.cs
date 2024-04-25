using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public GameObject mainSettingsPanel;  // ���� ���� ����â �г�
    public GameObject trySettingsPanel;   // Try ���� ����â �г�

    public Slider bgmVolumeSlider;           // ������� ���� ���� �����̴�
    public Slider effectVolumeSlider;        // ȿ���� ���� ���� �����̴�
    public Slider masterVolumeSlider;        // ������ ���� ���� �����̴�

    public AudioClip mainBgmSource;         // ���� �� ��� ����
    public AudioClip tryBgmSource;          // Try �� ��� ����
    public AudioSource bgmSource;           // ��� ������ ����� AudioSource

    public AudioSource effectSource;        // ȿ������ ����� AudioSource

    public AudioManager audioManager;       // AudioManager ����
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
            Destroy(gameObject);  // �ߺ� �ν��Ͻ� ����
        }

        // ��� ������ ����� AudioSource ����
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true; // �ݺ� ��� ����
    }
    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // �� �ε� �� �̺�Ʈ ���
    }

    private void SetupVolumeSlider()
    {
        // ������� ���� �����̴� ����
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.value = audioManager.BgmVolume;
            bgmVolumeSlider.onValueChanged.RemoveAllListeners();
            bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
        }

        // ȿ���� ���� �����̴� ����
        if (effectVolumeSlider != null)
        {
            effectVolumeSlider.value = audioManager.EffectVolume;
            effectVolumeSlider.onValueChanged.RemoveAllListeners();
            effectVolumeSlider.onValueChanged.AddListener(SetEffectVolume);
        }

        // ������ ���� �����̴� ����
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
            panelToUse.SetActive(true); // �г� Ȱ��ȭ
            SetupVolumeSlider(); // �����̴� ����
        }
        else
        {
            Debug.LogError($"Panel not found for the scene {(isMainScene ? "Main" : "Try")}");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Try ���� ��� TrySettingManager�� ã�� ���� ����
        if (scene.name == "Try")
        {
            trySettingManager = FindObjectOfType<TrySettingManager>();
            if (trySettingManager != null)
            {
                // TrySettingManager���� �����̴� ã�� ����
                bgmVolumeSlider = trySettingManager.volumeSlider;
                effectVolumeSlider = trySettingManager.effectVolumeSlider;
                masterVolumeSlider = trySettingManager.masterVolumeSlider;

                // �����̴� ����
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
        SceneManager.sceneLoaded -= OnSceneLoaded;  // ������Ʈ �ı� �� �̺�Ʈ ����
    }
}
