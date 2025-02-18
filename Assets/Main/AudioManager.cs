using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class AudioManager : MonoBehaviour
{
    public AudioSource bgmSource;
    public AudioSource effectSource;
    public static AudioManager Instance;

    public AudioClip BGM_Main;
    public AudioClip BGM_Intro;
    public AudioClip BGM_Outro;
    public AudioClip[] BGM_Try;
    public AudioClip[] BGM_Boss;

    private float fadeDuration = 0.5f;

    public AudioClip buttonClickSound;
    public SettingsManager settingsManager;

    public float _currentBgmVolume = 1.0f;
    public float _currentMasterVolume = 1.0f;
    public float _currentEffectVolume = 1.0f;

    private AudioClip previousBgmClip;

    public AudioClip arrowShootSound;
    public AudioClip monsterhitSound;
    public AudioClip secondHitSound;
    public AudioClip levelUpSound;
    public AudioClip explosionSound;

    public Slider BgmVolumeSlider { get; set; }
    public Slider EffectVolumeSlider { get; set; }
    public Slider MasterVolumeSlider { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (Instance != this)
        {
            Debug.Log("Duplicate AudioManager destroyed");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        bgmSource.Stop();
        bgmSource.volume = _currentBgmVolume * _currentMasterVolume; //  페이드 인 없이 즉시 볼륨 설정
        bgmSource.clip = BGM_Intro;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene Loaded: " + scene.name);

        switch (scene.name)
        {
            case "Intro":
            case "Title":
                PlayBGM(BGM_Intro, false); //  페이드 인 없이 재생
                break;
            case "Main":
                PlayBGM(BGM_Main, false); //  페이드 인 없이 즉시 실행
                break;
            case "Try":
                PlayRandomBGM(BGM_Try);
                break;
            case "Ending":
                PlayBGM(BGM_Outro);
                break;
        }
    }

    public void PlayBGM(AudioClip bgm, bool useFade = true)
    {
        if (bgmSource.clip == bgm && bgmSource.isPlaying)
            return;

        if (useFade)
            StartCoroutine(FadeOutAndChangeBGM(bgm));
        else
        {
            bgmSource.Stop();
            bgmSource.clip = bgm;
            bgmSource.loop = true;
            bgmSource.volume = _currentBgmVolume * _currentMasterVolume;
            bgmSource.Play();
        }
    }

    private IEnumerator FadeOutAndChangeBGM(AudioClip newBGM)
    {
        if (bgmSource.clip == newBGM)
            yield break;

        if (bgmSource.isPlaying)
        {
            float startVolume = bgmSource.volume;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                bgmSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                yield return null;
            }

            bgmSource.Stop();
        }

        bgmSource.clip = newBGM;
        bgmSource.loop = true;
        bgmSource.volume = _currentBgmVolume * _currentMasterVolume;
        bgmSource.Play();
    }

    private void PlayRandomBGM(AudioClip[] bgmArray)
    {
        int index = UnityEngine.Random.Range(0, bgmArray.Length);
        PlayBGM(bgmArray[index]);
    }

    public void FindAndAssignSliders()
    {
        BgmVolumeSlider = GameObject.Find("VolumeSlider")?.GetComponent<Slider>();
        EffectVolumeSlider = GameObject.Find("EffectVolumeSlider")?.GetComponent<Slider>();
        MasterVolumeSlider = GameObject.Find("MasterVolumeSlider")?.GetComponent<Slider>();

        if (BgmVolumeSlider != null)
        {
            BgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
            BgmVolumeSlider.value = _currentBgmVolume;
        }

        if (EffectVolumeSlider != null)
        {
            EffectVolumeSlider.onValueChanged.AddListener(SetEffectVolume);
            EffectVolumeSlider.value = _currentEffectVolume;
        }

        if (MasterVolumeSlider != null)
        {
            MasterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            MasterVolumeSlider.value = _currentMasterVolume;
        }
    }

    public void SetBgmVolume(float volume)
    {
        BgmVolume = volume;
    }

    public void SetEffectVolume(float volume)
    {
        EffectVolume = volume;
    }

    public void SetMasterVolume(float volume)
    {
        MasterVolume = volume;
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
            UpdateAllVolumes();
        }
    }

    private void UpdateAllVolumes()
    {
        bgmSource.volume = _currentBgmVolume * _currentMasterVolume;
        effectSource.volume = _currentEffectVolume * _currentMasterVolume;
    }

    public void PlayTrySceneBGM(bool play)
    {
        if (play)
        {
            bgmSource.clip = BGM_Try[0];
            bgmSource.Play();
        }
        else
        {
            bgmSource.Stop();
        }
    }

    public void StartBossBattle()
    {
        PlayRandomBGM(BGM_Boss);
    }

    public void StopBossBattleBGM()
    {
        if (bgmSource.isPlaying && Array.Exists(BGM_Boss, clip => clip == bgmSource.clip))
        {
            bgmSource.Stop();
            PlayBGM(previousBgmClip, false);
        }
    }

    public void PlayButtonClickSound()
    {
        effectSource.PlayOneShot(buttonClickSound);
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
            effectSource.PlayOneShot(explosionSound, 2f);
        }
    }

    public void PlayMonsterHitSound()
    {
        if (effectSource != null && monsterhitSound != null)
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
