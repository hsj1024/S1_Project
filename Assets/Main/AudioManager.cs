using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioSource bgmSource;
    public AudioSource effectSource;  // ȿ������ ����� ����� �ҽ�
    public static AudioManager Instance;

    public AudioClip mainSceneBGM;
    public AudioClip trySceneBGM;
    public AudioClip buttonClickSound;  // ��ư Ŭ�� ���� Ŭ��
    public SettingsManager settingsManager;
    private float _currentBgmVolume = 1.0f;

    private Slider _bgmVolumeSlider;
    private Slider _effectVolumeSlider;
    private Slider _masterVolumeSlider;

    public float _currentMasterVolume = 1.0f; // ������ ������ ������ ����
    private float _currentEffectVolume = 1.0f;
    private float _currentVolume = 1.0f;  // ���� ������ ������ ����

    private AudioClip previousBgmClip; // ������ ��� ���̾��� ��� ���� Ŭ��


    public AudioClip arrowShootSound; // ȭ�� �߻� �Ҹ� Ŭ��
    public AudioClip monsterhitSound; // �ٸ�����Ʈ �浹 �Ҹ�
    public AudioClip secondHitSound;  // �� ��° �浹 �Ҹ� Ŭ��

    public AudioClip levelUpSound;  // ���� �� ȿ���� Ŭ��

    public AudioClip explosionSound; // ���� ���� Ŭ��

    private void Start()
    {
        // ������ ��� ���̾��� ��� ���� Ŭ���� �ʱ�ȭ
        previousBgmClip = mainSceneBGM;
        bgmSource.clip = previousBgmClip;
        bgmSource.Play();
    }
    // ������ ��� ���̾��� ��� ���� Ŭ���� �����մϴ�.
    private void SetPreviousBgmClip()
    {
        if (SceneManager.GetActiveScene().name == "Try")
        {
            previousBgmClip = trySceneBGM;
        }
        else
        {
            previousBgmClip = mainSceneBGM;
        }
    }
    public void FindAndAssignSliders()
    {
        _bgmVolumeSlider = GameObject.Find("VolumeSlider")?.GetComponent<Slider>();
        _effectVolumeSlider = GameObject.Find("EffectVolumeSlider")?.GetComponent<Slider>();
        _masterVolumeSlider = GameObject.Find("MasterVolumeSlider")?.GetComponent<Slider>();

        if (_bgmVolumeSlider != null)
        {
            _bgmVolumeSlider.onValueChanged.RemoveAllListeners();
            _bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
            _bgmVolumeSlider.value = _currentBgmVolume;
        }
        else
        {
            Debug.LogError("Failed to find BGM Volume Slider.");
        }

        if (_effectVolumeSlider != null)
        {
            _effectVolumeSlider.onValueChanged.RemoveAllListeners();
            _effectVolumeSlider.onValueChanged.AddListener(SetEffectVolume);
            _effectVolumeSlider.value = _currentEffectVolume;
        }
        else
        {
            Debug.LogError("Failed to find Effect Volume Slider.");
        }

        if (_masterVolumeSlider != null)
        {
            _masterVolumeSlider.onValueChanged.RemoveAllListeners();
            _masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            _masterVolumeSlider.value = _currentMasterVolume;
        }
        else
        {
            Debug.LogError("Failed to find Master Volume Slider.");
        }
    }

    public float BgmVolume
    {
        get { return _currentBgmVolume; }
        set
        {
            _currentBgmVolume = value;
            bgmSource.volume = _currentBgmVolume * _currentMasterVolume;
        }
    }
    public float EffectVolume
    {
        get { return _currentEffectVolume; }
        set
        {
            _currentEffectVolume = value;
            effectSource.volume = _currentEffectVolume * _currentMasterVolume;
        }
    }

    public float MasterVolume
    {
        get { return _currentMasterVolume; }
        set
        {
            _currentMasterVolume = value;
            UpdateAllVolumes(_currentMasterVolume);
        }
    }

    // ������ ���� �����̴��� ������Ƽ
    public Slider MasterVolumeSlider
    {
        get { return _masterVolumeSlider; }
        set
        {
            _masterVolumeSlider = value;
            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
                _masterVolumeSlider.value = _currentMasterVolume; // ���� ������ �������� �ʱ�ȭ
            }
        }
    }

    public Slider BgmVolumeSlider
    {
        get { return _bgmVolumeSlider; }
        set
        {
            _bgmVolumeSlider = value;
            if (_bgmVolumeSlider != null)
            {
                _bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
                _bgmVolumeSlider.value = _currentBgmVolume;
            }
        }
    }

    public Slider EffectVolumeSlider
    {
        get { return _effectVolumeSlider; }
        set
        {
            _effectVolumeSlider = value;
            if (_effectVolumeSlider != null)
            {
                _effectVolumeSlider.onValueChanged.AddListener(SetEffectVolume);
                _effectVolumeSlider.value = _currentEffectVolume;
            }
        }
    }

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // ������Ʈ �ı� ����
            SceneManager.sceneLoaded += OnSceneLoaded;  // �� �ε� �� ȣ��� �̺�Ʈ �߰�
        }
        else if (Instance != this)
        {
            Destroy(gameObject);  // �ߺ� �ν��Ͻ� ����
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;  // �̺�Ʈ ����
    }




    // ���� �ε�� �� ȣ��˴ϴ�.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ������ ��� ���̾��� ��� ���� Ŭ���� �����մϴ�.
        SetPreviousBgmClip();

        // ������ ��� ���̾��� ��� ���� Ŭ���� ���� ��� ���� Ŭ���� �ٸ��ٸ�
        // ������ ��� ���̾��� ��� ���� Ŭ���� �����ϰ� ���� ��� ���� Ŭ���� ����մϴ�.
        if (bgmSource.clip != previousBgmClip)
        {
            bgmSource.Stop();
            bgmSource.clip = previousBgmClip;
            bgmSource.Play();
        }
    }
    // ��� ���� ��� ������ �����ϰ� ������ ��� ���̾��� ��� ���� Ŭ���� ����մϴ�.
    public void ResumePreviousBgm()
    {
        bgmSource.Stop();
        bgmSource.clip = previousBgmClip;
        bgmSource.Play();
    }


    private void ApplyVolumeSettings()
    {
        bgmSource.volume = _currentBgmVolume * _currentMasterVolume;
        effectSource.volume = _currentEffectVolume * _currentMasterVolume;
    }
    public void SetVolume(float volume)
    {
        _currentVolume = volume;  // ���� �� ����
        bgmSource.volume = _currentBgmVolume * volume;  // ��� ���� ���� ����
        effectSource.volume = _currentEffectVolume * volume;  // ȿ���� ���� ����
    }


    public void PlayButtonClickSound()
    {
        effectSource.PlayOneShot(buttonClickSound);
    }
    // ������ ���� ���� �޼���
    public void SetMasterVolume(float volume)
    {
        _currentMasterVolume = volume; // ������ ���� �� ����

        // ������ ������ ���� BGM �� ȿ���� ���� �����̴� ����
        if (_bgmVolumeSlider != null)
        {
            _bgmVolumeSlider.SetValueWithoutNotify(volume); // �����̴� �� ���� ����
        }

        if (_effectVolumeSlider != null)
        {
            _effectVolumeSlider.SetValueWithoutNotify(volume); // �����̴� �� ���� ����
        }
        if (_masterVolumeSlider != null)
        {
            _masterVolumeSlider.SetValueWithoutNotify(volume); // �����̴� �� ���� ����
        }

        // ��� ���� ������Ʈ
        UpdateAllVolumes(volume);
    }

    private void UpdateAllVolumes(float volume)
    {
        if (bgmSource != null && effectSource != null)
        {
            bgmSource.volume = _currentBgmVolume * _currentMasterVolume;
            effectSource.volume = _currentEffectVolume * _currentMasterVolume;
            
        }
    }

    private void UpdateVolume()
    {
        if (bgmSource != null && effectSource != null)
        {
            bgmSource.volume = _currentBgmVolume * _currentMasterVolume;
            effectSource.volume = _currentEffectVolume * _currentMasterVolume;
           
        }
    }


    public void SetBgmVolume(float volume)
    {
        _currentBgmVolume = volume;
        bgmSource.volume = _currentBgmVolume * _currentMasterVolume; // ������ ������ ����� ���
    }

    public void SetEffectVolume(float volume)
    {
        _currentEffectVolume = volume;
        effectSource.volume = _currentEffectVolume * _currentMasterVolume;// ������ ������ ����� ���
        
    }

    public void PlayTrySceneBGM(bool play)
    {
        if (play)
        {
            bgmSource.clip = trySceneBGM;
            bgmSource.Play();
        }
        else
        {
            bgmSource.Stop();
        }
    }
    public void PlayArrowShootSound()
    {
        if (effectSource != null && arrowShootSound != null)
        {
            effectSource.PlayOneShot(arrowShootSound);
        }
    }

    public void PlayLevelUpSound()
    {
        Debug.Log($"Level Up Sound Volume: {effectSource.volume * 0.1f}");
        if (effectSource != null && levelUpSound != null)
        {
            effectSource.PlayOneShot(levelUpSound, 0.4f);
        }
    }

    public void PlayExplosionSound()
    {
        if (effectSource != null && explosionSound != null)
        {
            effectSource.PlayOneShot(explosionSound, 2f);  // ������ 150%�� ����
        }
    }

    // ���� �߰�
    public void PlayMonsterHitSound()
    {
        if (effectSource != null  && monsterhitSound != null)
                   {
            effectSource.PlayOneShot(monsterhitSound);
        }
    }
    public void PlaySecondMonsterHitSound()
    {
        if (effectSource != null && secondHitSound != null)
        {
            effectSource.PlayOneShot(secondHitSound);
        }
    }

}
