    using UnityEngine.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;


    public class AudioManager : MonoBehaviour
        {
            public AudioSource bgmSource;
            public AudioSource effectSource;  // ȿ������ ����� ����� �ҽ�
            public static AudioManager Instance;

            //public AudioClip mainSceneBGM;
            //public AudioClip trySceneBGM;


            public AudioClip BGM_Main;        // ���� �� BGM
            public AudioClip BGM_Intro;       // ��Ʈ�� BGM
            public AudioClip BGM_Outro;       // ���� BGM

            public AudioClip[] BGM_Try;       // Ʈ���� �� BGM �迭 (Try_1, Try_2)
            public AudioClip[] BGM_Boss;      // ���� �� BGM �迭 (Boss_1, Boss_2)

            private float fadeDuration = 0.5f;  // ���̵� ��/�ƿ� �ð�

            public AudioClip buttonClickSound;  // ��ư Ŭ�� ���� Ŭ��
            public SettingsManager settingsManager;
            private float _currentBgmVolume = 1.0f;



            // ==============================================================================
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
                //if (SceneManager.GetActiveScene().name == "Intro")
                //{
                //    PlayBGM(BGM_Intro);
                //}
                // ������ ��� ���̴� BGM�� �����ϰ�, ��Ʈ�� �������� Ȯ���� ��Ʈ�� BGM�� ����ϵ��� ����
                bgmSource.Stop();  // � BGM�� ����ǰ� �ִٸ� ����
                PlayBGM(BGM_Intro);  // Intro BGM ���
            }


        // ������ ��� ���̾��� ��� ���� Ŭ���� �����մϴ�.
        private void SetPreviousBgmClip()
        {
            if (SceneManager.GetActiveScene().name == "Try")
            {
                previousBgmClip = BGM_Try[0];  // �迭���� ù ��° �� ����
            }
            else
            {
                previousBgmClip = BGM_Main;  // ���� �� BGM
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

            //private void Awake()
            //{

            //    if (Instance == null)
            //    {
            //        Instance = this;
            //        DontDestroyOnLoad(gameObject);  // ������Ʈ �ı� ����
            //        SceneManager.sceneLoaded += OnSceneLoaded;  // �� �ε� �� ȣ��� �̺�Ʈ �߰�
            //    }
            //    else if (Instance != this)
            //    {
            //        Destroy(gameObject);  // �ߺ� �ν��Ͻ� ����
            //    }
            //}

            private void Awake()
            {
                if (Instance == null)
                {
                    Instance = this;
                    DontDestroyOnLoad(gameObject);
                    SceneManager.sceneLoaded += OnSceneLoaded;
                }
                else
                {
                    Destroy(gameObject);
                }
            }

            private void OnDestroy()
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;  // �̺�Ʈ ����
            }




            // ���� �ε�� �� ȣ��˴ϴ�.
            //private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
            //{
            //    // ������ ��� ���̾��� ��� ���� Ŭ���� �����մϴ�.
            //    SetPreviousBgmClip();

            //    // ������ ��� ���̾��� ��� ���� Ŭ���� ���� ��� ���� Ŭ���� �ٸ��ٸ�
            //    // ������ ��� ���̾��� ��� ���� Ŭ���� �����ϰ� ���� ��� ���� Ŭ���� ����մϴ�.
            //    if (bgmSource.clip != previousBgmClip)
            //    {
            //        bgmSource.Stop();
            //        bgmSource.clip = previousBgmClip;
            //        bgmSource.Play();
            //    }
            //}
            private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
            {

                switch (scene.name)
                {
                    case "Intro":
                    case "Title":
                        PlayBGM(BGM_Intro);
                        break;
                    case "Main":
                        PlayBGM(BGM_Main);
                        break;
                    case "Try":
                        PlayRandomBGM(BGM_Try);
                        break;

                    case "Ending/Ending":
                        PlayBGM(BGM_Outro);
                        break;
                }
            }

            // �������� BGM ����
            private void PlayRandomBGM(AudioClip[] bgmArray)
            {
                int index = Random.Range(0, bgmArray.Length);
                PlayBGM(bgmArray[index]);
            }

            // BGM ���
            public void PlayBGM(AudioClip bgm)
            {
                if (bgmSource.clip == bgm)
                    return;

                StartCoroutine(FadeOutAndChangeBGM(bgm));
            }

            // BGM ���̵� �ƿ� �� ���� �� ���̵� ��
            private IEnumerator FadeOutAndChangeBGM(AudioClip newBGM)
            {
                if (bgmSource.isPlaying)
                {
                    // ���̵� �ƿ�
                    float startVolume = bgmSource.volume;
                    for (float t = 0; t < fadeDuration; t += Time.deltaTime)
                    {
                        bgmSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                        yield return null;
                    }

                    bgmSource.Stop();
                    bgmSource.volume = startVolume;
                }

                // ���ο� BGM ���� �� ���
                bgmSource.clip = newBGM;
                bgmSource.loop = true;
                bgmSource.Play();

                // ���̵� ��
                for (float t = 0; t < fadeDuration; t += Time.deltaTime)
                {
                    bgmSource.volume = Mathf.Lerp(0, _currentBgmVolume * _currentMasterVolume, t / fadeDuration);
                    yield return null;
                }
            }


            // TRY ������ �������� ���۵� �� ȣ��
            public void StartBossBattle()
            {
                PlayRandomBossBGM(); // ������ ���� �� BGM ����
            }

            // ���� BGM�� 50% Ȯ���� �����Ͽ� ���
            private void PlayRandomBossBGM()
            {
                int randomValue = Random.Range(0, 2); // 0 �Ǵ� 1�� �������� ����
                AudioClip selectedBGM = (randomValue == 0) ? BGM_Boss[0] : BGM_Boss[1]; // BGM_Boss[0]�� BGM_Boss_1, BGM_Boss[1]�� BGM_Boss_2

                PlayBGM(selectedBGM); // ���õ� BGM ���
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
                    // Try ���� ù ��° BGM�� ���
                    bgmSource.clip = BGM_Try[0];  // Ʈ���� �� BGM �迭���� ù ��° ���� �����Ͽ� ���
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
