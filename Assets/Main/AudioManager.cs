using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioSource bgmSource;
    public AudioSource effectSource;  // 효과음을 재생할 오디오 소스
    public static AudioManager Instance;

    public AudioClip mainSceneBGM;
    public AudioClip trySceneBGM;
    public AudioClip buttonClickSound;  // 버튼 클릭 사운드 클립
    public SettingsManager settingsManager;
    private float _currentBgmVolume = 1.0f;

    private Slider _bgmVolumeSlider;
    private Slider _effectVolumeSlider;
    private Slider _masterVolumeSlider;

    public float _currentMasterVolume = 1.0f; // 마스터 볼륨을 저장할 변수
    private float _currentEffectVolume = 1.0f;
    private float _currentVolume = 1.0f;  // 현재 볼륨을 저장할 변수

    private AudioClip previousBgmClip; // 이전에 재생 중이었던 배경 음악 클립


    public AudioClip arrowShootSound; // 화살 발사 소리 클립
    public AudioClip monsterhitSound; // 바리케이트 충돌 소리
    public AudioClip secondHitSound;  // 두 번째 충돌 소리 클립

    public AudioClip levelUpSound;  // 레벨 업 효과음 클립

    public AudioClip explosionSound; // 폭발 사운드 클립

    private void Start()
    {
        // 이전에 재생 중이었던 배경 음악 클립을 초기화
        previousBgmClip = mainSceneBGM;
        bgmSource.clip = previousBgmClip;
        bgmSource.Play();
    }
    // 이전에 재생 중이었던 배경 음악 클립을 설정합니다.
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

    // 마스터 볼륨 슬라이더의 프로퍼티
    public Slider MasterVolumeSlider
    {
        get { return _masterVolumeSlider; }
        set
        {
            _masterVolumeSlider = value;
            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
                _masterVolumeSlider.value = _currentMasterVolume; // 현재 마스터 볼륨으로 초기화
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
            DontDestroyOnLoad(gameObject);  // 오브젝트 파괴 방지
            SceneManager.sceneLoaded += OnSceneLoaded;  // 씬 로드 시 호출될 이벤트 추가
        }
        else if (Instance != this)
        {
            Destroy(gameObject);  // 중복 인스턴스 제거
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;  // 이벤트 해제
    }




    // 씬이 로드될 때 호출됩니다.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 이전에 재생 중이었던 배경 음악 클립을 설정합니다.
        SetPreviousBgmClip();

        // 이전에 재생 중이었던 배경 음악 클립과 현재 배경 음악 클립이 다르다면
        // 이전에 재생 중이었던 배경 음악 클립을 중지하고 현재 배경 음악 클립을 재생합니다.
        if (bgmSource.clip != previousBgmClip)
        {
            bgmSource.Stop();
            bgmSource.clip = previousBgmClip;
            bgmSource.Play();
        }
    }
    // 재생 중인 배경 음악을 중지하고 이전에 재생 중이었던 배경 음악 클립을 재생합니다.
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
        _currentVolume = volume;  // 볼륨 값 저장
        bgmSource.volume = _currentBgmVolume * volume;  // 배경 음악 볼륨 설정
        effectSource.volume = _currentEffectVolume * volume;  // 효과음 볼륨 설정
    }


    public void PlayButtonClickSound()
    {
        effectSource.PlayOneShot(buttonClickSound);
    }
    // 마스터 볼륨 설정 메서드
    public void SetMasterVolume(float volume)
    {
        _currentMasterVolume = volume; // 마스터 볼륨 값 저장

        // 마스터 볼륨에 따라 BGM 및 효과음 볼륨 슬라이더 설정
        if (_bgmVolumeSlider != null)
        {
            _bgmVolumeSlider.SetValueWithoutNotify(volume); // 슬라이더 값 직접 설정
        }

        if (_effectVolumeSlider != null)
        {
            _effectVolumeSlider.SetValueWithoutNotify(volume); // 슬라이더 값 직접 설정
        }
        if (_masterVolumeSlider != null)
        {
            _masterVolumeSlider.SetValueWithoutNotify(volume); // 슬라이더 값 직접 설정
        }

        // 모든 볼륨 업데이트
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
        bgmSource.volume = _currentBgmVolume * _currentMasterVolume; // 마스터 볼륨과 연계된 계산
    }

    public void SetEffectVolume(float volume)
    {
        _currentEffectVolume = volume;
        effectSource.volume = _currentEffectVolume * _currentMasterVolume;// 마스터 볼륨과 연계된 계산
        
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
            effectSource.PlayOneShot(explosionSound, 2f);  // 볼륨을 150%로 증가
        }
    }

    // 하정 추가
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
