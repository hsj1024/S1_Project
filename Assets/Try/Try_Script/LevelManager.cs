    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using static StatManager;

    public class LevelManager : MonoBehaviour
    {
        public int Level = 1;
        public float NextLevelXP = 2.5f; // 다음 레벨까지 필요한 경험치
        public Text levelUpText; // 레벨업 텍스트 UI
        public Text currentLevel;
        public GameObject levelUpPopup; // 레벨업 팝업 UI
        public Button closeButton; // 팝업의 닫기 버튼
        public GameObject overlayPanel; // 오버레이 패널 참조
        public Bal balInstance; // Bal 클래스의 인스턴스 참조

        private bool isGamePaused = false; // 게임 일시정지 여부
        public List<StatUpgrade> statUpgrades = new List<StatUpgrade>();

        public GameObject specialLevelUpPanel; // 특별 레벨업 패널
        public List<StatUpgrade> specialStatUpgrades = new List<StatUpgrade>(); // 특별 레벨업 스탯 업그레이드 리스트 추가
        public Button[] specialUpgradeButtons; // 특별 레벨업에 사용될 버튼 배열

        public List<GameObject> specialButtonPrefabs; // 특별 스탯 업그레이드 버튼 프리팹을 저장할 리스트
        public BallistaController ballistaController; // BallistaController 참조 추가

        private StatManager statManager; // StatManager에 대한 참조를 저장할 필드
        public static LevelManager Instance { get; private set; }
        // LevelManager 내에 버튼 연결 로직 추가
        public Button[] upgradeButtons; // 이 배열은 Inspector에서 초기화

        public int totalMonstersKilled = 0; // 총 몬스터 처치 수를 저장할 변수
        public GameObject gameOverPanel;

        public Canvas canvas; // Canvas 참조
        public Camera mainCamera; // Camera 참조

        public GameObject panelToActivate; // 패널 활성화를 위한 변수
        public GameObject popupToDeactivate; // 팝업 비활성화를 위한 변수

        private bool isLevelUpPopupActive = false; // 레벨업 팝업 활성화 여부

        //버튼 

        public Transform[] buttonPositions; // 버튼 위치를 저장할 배열
        public List<GameObject> buttonPrefabs; // 스탯별로 다른 버튼 프리팹을 저장할 리스트

        public GameObject[] normalCardObjects; // 일반 레벨업 카드 게임 오브젝트 배열
        public GameObject turretObject; // 터렛 오브젝트를 설정할 수 있는 변수 추가


        public GameObject settingsPanel; // 설정 패널
        public Button settingButton; // 설정 버튼

        public Button quitButton; // 끝내기 버튼

        public Slider xpSlider; // 경험치 바 UI
        public Image barImage;
        private HashSet<string> obtainedSpecialUpgrades = new HashSet<string>(); // 이미 획득한 특별 업그레이드를 추적하는 HashSet



        [System.Serializable]
        public class StatUpgrade
        {
            public string name;
            public float effect;
            public int probability;
            public GameObject buttonPrefab; // 각 스탯 업그레이드에 해당하는 버튼 프리팹

            public StatUpgrade(string name, float effect, int probability, GameObject buttonPrefab)
            {
                this.name = name;
                this.effect = effect;
                this.probability = probability;
                this.buttonPrefab = buttonPrefab;
            }
        }
        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 씬 로드 완료 후 필요한 컴포넌트 재검색 및 리스너 재설정
            statManager = FindObjectOfType<StatManager>();


            // currentLevel 텍스트 찾기.
            GameObject currentLevelObject = GameObject.Find("currentLevel");
            if (currentLevelObject != null)
            {
                currentLevel = currentLevelObject.GetComponent<Text>();
                if (currentLevel == null)
                {
                    Debug.LogError("currentLevel 텍스트 컴포넌트를 찾을 수 없습니다.");
                }
                else
                {
                    UpdateLevelDisplay(); // currentLevel 텍스트 컴포넌트를 찾았으면 레벨 표시를 업데이트합니다.
                }
            }
            if (scene.name == "Main/Main" || scene.name == "StatSetting")
            {
                levelUpPopup.SetActive(false);
                overlayPanel.SetActive(false);
                specialLevelUpPanel.SetActive(false);
                isGamePaused = false;
                xpSlider.gameObject.SetActive(false); // XP Slider 비활성화
            }
            else if (scene.name == "Try")
            {
                xpSlider.gameObject.SetActive(true); // XP Slider 활성화
            }
            else
            {
                xpSlider.gameObject.SetActive(false); // 다른 모든 씬에서 XP Slider 비활성화
            }

            // GameOver 패널 초기화 및 위치 설정
            GameOverUI gameOverUI = gameOverPanel.GetComponent<GameOverUI>();
            if (gameOverUI != null)
            {
                gameOverUI.ResetPanelSizeAndPosition();
            }
            mainCamera = Camera.main;
            AssignCameraToCanvas();

            
            // 중복 초기화를 방지하기 위해 초기화 전에 기존의 버튼들을 제거
            foreach (Transform buttonPosition in buttonPositions)
            {
                if (buttonPosition.childCount > 0)
                {
                    foreach (Transform child in buttonPosition)
                    {
                        Destroy(child.gameObject); // 기존 버튼 제거
                    }
                }
            }

            // 세팅 패널 초기화
            InitializeSettingsPanel();

            // 버튼 초기화 및 씬에 따른 설정
            InitializeButtons(scene.name);

            InitializeXpSlider();   

            
    }

    private void InitializeButtons(string sceneName)
        {
            // 특정 씬에서만 버튼을 활성화
            if (sceneName == "Try")
            {
                if (settingButton != null)
                {
                    settingButton.gameObject.SetActive(true);
                    xpSlider.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError("Setting button is not assigned.");

                }
            }
            else
            {
                if (settingButton != null)
                {
                    settingButton.gameObject.SetActive(false);
                    xpSlider.gameObject.SetActive(false);
                }
            }
        }
        private void InitializeSettingsPanel()
        {
            // 세팅 패널 및 버튼 재설정
            if (settingsPanel == null)
            {
                settingsPanel = GameObject.Find("Setting_panel");
            }
            if (settingButton == null)
            {
                GameObject settingButtonObject = GameObject.Find("Setting_button");
                if (settingButtonObject != null)
                {
                    settingButton = settingButtonObject.GetComponent<Button>();
                    settingButton.onClick.RemoveAllListeners();
                    settingButton.onClick.AddListener(ToggleSettingsPanel);
                }
            }

            // 끝내기 버튼 이벤트 설정
            if (quitButton != null)
            {
                quitButton.onClick.RemoveAllListeners(); // 기존 리스너 제거
                quitButton.onClick.AddListener(GameOver); // 새로운 리스너 추가
            }
            else
            {
                Debug.LogError("QuitButton is not assigned in the inspector.");
            }
        }
        private void ToggleSettingsPanel()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf); // 패널 활성화/비활성화 토글
                if (settingsPanel.activeSelf)
                {
                    PauseGame(); // 패널이 활성화되면 게임을 일시 정지
                }
                else
                {
                    ResumeGame(); // 패널이 비활성화되면 게임을 재개
                }
            }
        }
        private void AssignCameraToCanvas()
        {
            if (canvas == null)
            {
                Debug.LogError("Canvas가 할당되지 않았습니다.");
                canvas = FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    Debug.LogError("씬 내에 Canvas를 찾을 수 없습니다.");
                    return;
                }
            }

            if (canvas.renderMode != RenderMode.ScreenSpaceCamera)
            {
                Debug.LogWarning("Canvas의 Render Mode가 ScreenSpaceCamera가 아닙니다. 설정을 변경합니다.");
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
            }

            if (mainCamera == null)
            {
                Debug.Log("Main Camera를 찾는 중...");
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogError("Main Camera를 찾을 수 없습니다.");
                    return;
                }
                else
                {
                    Debug.Log("Main Camera가 할당되었습니다: " + mainCamera.name);
                }
            }
            canvas.worldCamera = mainCamera;
            Debug.Log("Camera가 Canvas에 할당되었습니다: " + mainCamera.name);
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);  // 씬 전환 시 파괴되지 않음
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }

            // 세팅 패널 초기화
            InitializeSettingsPanel();
        }
        private List<StatUpgrade> selectedUpgrades = new List<StatUpgrade>(); // selectedUpgrades를 클래스 수준으로 이동

        void Start()
        {
            UpdateLevelDisplay(); // 게임 시작 시 레벨 표시를 업데이트
            InitializeXpSlider(); // 경험치 바 초기화

            // 끝내기 버튼 이벤트 설정
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(GameOver);
            }

            // StatManager에 대한 싱글톤 인스턴스 찾기.
            statManager = StatManager.Instance;
            InitializeStatUpgrades();
            InitializeSpecialStatUpgrades();

            // Bal 클래스의 인스턴스를 찾아 참조.
            balInstance = FindObjectOfType<Bal>();
            if (balInstance == null)
            {
                Debug.LogError("Bal 클래스의 인스턴스를 찾을 수 없습니다.");
                return;
            }
            // BallistaController의 인스턴스를 찾아 참조.
            ballistaController = FindObjectOfType<BallistaController>();
            if (ballistaController == null)
            {
                Debug.LogError("BallistaController 클래스를 찾을 수 없습니다.");
                return;
            }
            // StatManager의 인스턴스를 찾아 참조를 저장.
            statManager = FindObjectOfType<StatManager>();
            if (statManager == null)
            {
                Debug.LogError("StatManager 컴포넌트가 씬에 존재하지 않습니다.");
                return;
            }
            // LevelUpPopup이 있는 Canvas 설정
            Canvas popupCanvas = levelUpPopup.GetComponentInParent<Canvas>();
            if (popupCanvas)
            {
                popupCanvas.sortingOrder = 5000;  // 다른 모든 Canvas보다 높은 값을 설정
            }

            // 각 버튼에 대한 이벤트 핸들러 추가
            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                int buttonIndex = i; // 클로저에 전달하기 위해 인덱스 변수를 별도로 저장
                upgradeButtons[i].onClick.AddListener(() => ApplyStatUpgrade(selectedUpgrades[buttonIndex]));
            }


            if (SceneManager.GetActiveScene().name == "StatSetting")
            {
                StatManager.Instance.SetupButtons();
        
        
            }
            if (SceneManager.GetActiveScene().name == "StatSetting" || SceneManager.GetActiveScene().name == "Main/Main")
            {
                xpSlider.gameObject.SetActive(false); // StatSetting 및 Main/Main 씬에서 XP Slider 비활성화
            }
            else if (SceneManager.GetActiveScene().name == "Try")
            {
                xpSlider.gameObject.SetActive(true); // Try 씬에서 XP Slider 활성화
            }
            else
            {
                xpSlider.gameObject.SetActive(false); // 다른 모든 씬에서 XP Slider 비활성화
            }

            // 초기 레벨업 요구 경험치 설정
            UpdateLevelUpRequirement();

            // 초기에는 레벨업 팝업을 비활성화
            levelUpPopup.SetActive(false);
            overlayPanel.SetActive(false);

            // 닫기 버튼에 이벤트 리스너 추가
            //closeButton.onClick.AddListener(CloseLevelUpPopup);
        }

        void InitializeXpSlider()
        {
            if (xpSlider != null)
            {
                xpSlider.maxValue = NextLevelXP;
                xpSlider.value = balInstance.totalExperience; // 현재 경험치로 초기화
                if (barImage != null)
                {
                    barImage.fillAmount = xpSlider.value / xpSlider.maxValue;
                    // balInstance.totalExperience; // 현재 경험치로 초기화
                }
                Canvas.ForceUpdateCanvases();
                //Debug.Log("InitializeXpSlider: 경험치 바가 초기화되었습니다."); // 디버깅 메시지 추가
            }
        }

        

        void UpdateExperienceBar()
        {
            if (xpSlider != null)
            {
                xpSlider.maxValue = NextLevelXP; // 최대값을 현재 레벨의 기준 경험치로 설정
                xpSlider.value = balInstance.totalExperience;
                if (barImage != null)
                {
                    barImage.fillAmount = xpSlider.value / xpSlider.maxValue;
                }
            }
        }

        public void ApplyStatUpgrade(StatUpgrade upgrade)
        {
            // StatManager의 인스턴스를 통해 업그레이드 적용
            if (StatManager.Instance != null)
            {
                StatManager.Instance.ApplyUpgrade(upgrade);
            }
            // Bal 클래스의 특정 변수를 활성화하는 로직 추가
            if (balInstance != null)
            {
                switch (upgrade.name)
                {
                    case "지속 피해 부여":
                        balInstance.isDotActive = true;
                        break;
                    case "범위 피해 부여":
                        balInstance.isAoeActive = true;
                        break;
                    case "투사체 수 증가(중복)":
                        if (ballistaController != null)
                        {
                            ballistaController.IncreaseArrowCount(); // 투사체 수 증가
                            //balInstance.numberOfArrows = ballistaController.numberOfArrows; // 동기화

                    }
                    break;
                    case "투사체 관통 부여":
                        balInstance.isPdActive = true;
                        break;
                    case "자동 터렛 생성":
                        balInstance.isTurretActive = true;
                        ActivateTurret(); // 터렛 오브젝트 활성화
                        break;
                    case "투사체 넉백 부여":
                        balInstance.knockbackEnabled = true;
                        break;
                    case "조준 경로 표시":
                        if (ballistaController != null)
                        {
                            ballistaController.SetLineRendererEnabled(true); // LineRenderer 활성화
                        }
                        break;

                }
            }
            // 이 부분 약간 의심 됨 투사체 수 증가 안먹는 원인
            if (Level % 10 == 1 && upgrade.name != "투사체 수 증가(중복)")
            {
                obtainedSpecialUpgrades.Add(upgrade.name); // 특별 업그레이드를 획득한 것으로 표시
            }


            // 패널을 닫습니다.
            CloseLevelUpPopup();


            // 다음 레벨업 요구 경험치를 업데이트합니다.
            UpdateLevelUpRequirement();

            /*// 경험치 바 초기화
            if (xpSlider != null)
            {
                xpSlider.value = 0;
                Debug.Log("ApplyStatUpgrade: 경험치 바가 초기화되었습니다."); // 디버깅 메시지 추가
            }*/
            // 경험치 바 초기화
            //balInstance.totalExperience = 0f;



            UpdateExperienceBar();


            // 특별 레벨업 이후 일반 레벨업을 방지
            if ((Level - 1) % 10 == 0)
            {
                levelUpPopup.SetActive(false);
                overlayPanel.SetActive(false);
                // 바로 다음 레벨로 넘어가지 않도록 설정
                return;
            }

        }

        private void ActivateTurret()
        {
            // 터렛 오브젝트를 활성화합니다.
            if (turretObject != null)
            {
                turretObject.SetActive(true);
            }
            else
            {
                Debug.LogError("Turret object is not assigned in the inspector.");
            }
        }
        void InitializeStatUpgrades()
        {
            statUpgrades.Add(new StatUpgrade("피해량 증가 1", 2, 11, buttonPrefabs[0]));
            statUpgrades.Add(new StatUpgrade("피해량 증가 2", 3, 9, buttonPrefabs[1]));
            statUpgrades.Add(new StatUpgrade("피해량 증가 3", 5, 5, buttonPrefabs[2]));

            statUpgrades.Add(new StatUpgrade("재장전 시간 감소", -0.05f, 7, buttonPrefabs[3]));
            statUpgrades.Add(new StatUpgrade("투사체 속도 증가", 2, 7, buttonPrefabs[4])); // 공속 증가

            statUpgrades.Add(new StatUpgrade("치명타 확률 증가", 5, 9, buttonPrefabs[5]));

            statUpgrades.Add(new StatUpgrade("치명타 피해량 증가 1", 15, 8, buttonPrefabs[6]));

            statUpgrades.Add(new StatUpgrade("치명타 피해량 증가 2", 25, 5, buttonPrefabs[7]));

            statUpgrades.Add(new StatUpgrade("지속 피해량 증가", 3, 7, buttonPrefabs[8]));
            statUpgrades.Add(new StatUpgrade("범위 피해량 증가", 5, 7, buttonPrefabs[9]));
            statUpgrades.Add(new StatUpgrade("관통 피해량 증가", 10, 7, buttonPrefabs[10]));

            statUpgrades.Add(new StatUpgrade("자동 터렛 재장전 시간 감소", -0.3f, 4, buttonPrefabs[11]));
            statUpgrades.Add(new StatUpgrade("자동 터렛 피해량 증가", 5, 4, buttonPrefabs[12]));
            statUpgrades.Add(new StatUpgrade("경험치 배수 증가 1", 0.05f, 7, buttonPrefabs[13]));
            statUpgrades.Add(new StatUpgrade("경험치 배수 증가 2", 0.1f, 3, buttonPrefabs[14]));


        }

        void InitializeSpecialStatUpgrades()
        {
            // 특별 업그레이드 추가
            specialStatUpgrades.Add(new StatUpgrade("지속 피해 부여", 15, 100, specialButtonPrefabs[0])); // dot
            specialStatUpgrades.Add(new StatUpgrade("범위 피해 부여", 15, 0, specialButtonPrefabs[1])); // doa
            specialStatUpgrades.Add(new StatUpgrade("투사체 수 증가(중복)", 15, 100, specialButtonPrefabs[2])); // 투사체 수 증가
            
        /* 여기 주석 풀어야 함. 테스트 때문에 주석 
            specialStatUpgrades.Add(new StatUpgrade("투사체 관통 부여", 15, 0, specialButtonPrefabs[3])); // pd
            specialStatUpgrades.Add(new StatUpgrade("조준 경로 표시", 15, 0, specialButtonPrefabs[4])); // 조준경로
            specialStatUpgrades.Add(new StatUpgrade("자동 터렛 생성", 15, 0, specialButtonPrefabs[5]));  // turret
            specialStatUpgrades.Add(new StatUpgrade("투사체 넉백 부여", 15, 0, specialButtonPrefabs[6])); // knockback */

        }
        void Update()
        {
            // 게임이 일시정지 상태일 때는 레벨업을 확인하지 않습니다.
            if (!isGamePaused && !gameOverPanel.activeSelf)
            {
                // 경험치를 주기적으로 확인하여 레벨업을 처리합니다.
                if (balInstance.totalExperience >= NextLevelXP)
                {
                    LevelUp(); // 레벨 업 처리
                    //Debug.Log($"Level Up! New level: {Level}, New XP Requirement: {NextLevelXP}");
                }

                // 경험치 바 업데이트
                UpdateExperienceBar();
            }
        }

        public void AddExperience(float xp)
        {
            if (balInstance != null)
            {
                float currentXp = balInstance.totalExperience;
                float newXp = currentXp + xp;
                //.Log($"AddExperience: Current XP: {currentXp}, Added XP: {xp}, New XP: {newXp}"); // 디버그 로그 추가

                balInstance.totalExperience = newXp;

                // 경험치가 다음 레벨업 요구치를 초과하면 초과된 경험치를 넘겨줌
                while (balInstance.totalExperience >= NextLevelXP)
                {
                    LevelUp(); // 레벨 업 처리
                }

                // 경험치 바 업데이트
                UpdateExperienceBar();
            }
        }


        private void LevelUp()
        {
            float remainingXP = balInstance.totalExperience - NextLevelXP;
            if (remainingXP > 0)
            {
                balInstance.totalExperience = remainingXP;
            }
            else
            {
                balInstance.totalExperience = 0f;
            }

            Level++; // 레벨 증가
            UpdateLevelUpRequirement(); // 다음 레벨업 요구 경험치 업데이트
            PauseGame(); // 게임 일시정지

            ShowLevelUpPopup(); // 레벨업 팝업 표시

            // 경험치 바 초기화
            UpdateExperienceBar();

        }

        void UpdateLevelUpRequirement()
        {
            UpdateLevelDisplay(); // 레벨업 요구 조건이 업데이트될 때 레벨 표시도 업데이트

            if (Level == 1)
            {
                NextLevelXP = 3f; // 레벨 1에서 레벨 2로 가는 경우
            }
            else if (Level == 2)
            {
                NextLevelXP = 7f; // 레벨 2에서 레벨 3로 가는 경우
            }
            else if (Level >= 3 && Level < 20)
            {
                // 레벨 3에서 레벨 20까지 요구 경험치 설정
                // 13, 18, 23, 28, ... (+5씩 증가)
                NextLevelXP = 13 + (Level - 3) * 5;
            }
       
            else if (Level >= 20 && Level < 40)
            {
                // 레벨 20에서 레벨 40까지 요구 경험치 설정
                // 143, 150, 157, 164, ... (+7씩 증가)
                //NextLevelXP = 143 + (Level - 20) * 7;
                NextLevelXP = 3f;

            }
            else if (Level >= 40)
            {
                // 레벨 40 이상 요구 경험치 설정
                // 336, 344, 352, 360, ... (+8씩 증가)
                NextLevelXP = 336 + (Level - 40) * 8;
            }
            // 경험치 바 최대값 업데이트
            InitializeXpSlider();

        }


        public void ShowLevelUpPopup()
        {

            // 레벨업 팝업이 이미 활성화되어 있으면 반환
            if (isLevelUpPopupActive)
            {
                return;
            }
            isLevelUpPopupActive = true; // 레벨업 팝업 활성화
                                         // 특별 레벨업 후 일반 레벨업 팝업이 나오지 않도록 확인
            if (specialLevelUpPanel.activeSelf)
            {
                return;
            }

            List<StatUpgrade> selectedUpgrades = SelectRandomStatUpgrades();
            UpdateLevelDisplay(); // 레벨업 팝업을 보여줄 때 레벨 표시 업데이트

            // 모든 기존 버튼을 비활성화 및 제거
            for (int i = 0; i < buttonPositions.Length; i++)
            {
                if (buttonPositions[i].childCount > 0)
                {
                    
                    // 기존 코드
                    //Destroy(buttonPositions[i].GetChild(0).gameObject);  // 기존에 있던 버튼 제거
                }
            }

            // 특별 레벨업인 경우
            if ((Level - 1) % 10 == 0)  // 특별 레벨업 조건
            {
                specialLevelUpPanel.SetActive(true); // 특별 레벨업 패널 활성화
                overlayPanel.SetActive(true);

                levelUpPopup.SetActive(false); // 일반 레벨업 패널 비활성화

                for (int i = 0; i < specialUpgradeButtons.Length; i++)
                {
                    if (i < selectedUpgrades.Count)
                    {
                        GameObject buttonPrefab = Instantiate(selectedUpgrades[i].buttonPrefab, specialUpgradeButtons[i].transform.position, Quaternion.identity, specialUpgradeButtons[i].transform);
                        
                        /*buttonPrefab.transform.localPosition = Vector3.zero; // 위치 조정
                        //buttonPrefab.transform.localRotation = Quaternion.identity; // 회전 조정
                        //buttonPrefab.transform.localScale = Vector3.one; // 크기 조정 */

                        SetupButton(buttonPrefab, selectedUpgrades[i]); // 버튼 초기화

                        RectTransform rectTransform = buttonPrefab.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            rectTransform.sizeDelta = new Vector2(250, 375); // 원하는 크기로 설정
                        }
                        // 텍스트를 빈 문자열로 설정해 텍스트가 보이지 않게 합니다.
                        TMP_Text buttonText = buttonPrefab.GetComponentInChildren<TMP_Text>();
                        if (buttonText != null)
                        {
                            buttonText.text = ""; // 텍스트를 비움
                        }

                    //// 텍스트 업데이트
                    //TMP_Text buttonText = buttonPrefab.GetComponentInChildren<TMP_Text>();
                    //if (buttonText != null)
                    //{
                    //    buttonText.text = $"{selectedUpgrades[i].name} (+{selectedUpgrades[i].effect})";
                    //    buttonText.fontSize = 24; // 원하는 텍스트 크기로 설정

                    //    // 텍스트의 RectTransform 조정
                    //    RectTransform textRectTransform = buttonText.GetComponent<RectTransform>();
                    //    if (textRectTransform != null)
                    //    {
                    //        textRectTransform.sizeDelta = new Vector2(200, 100); // 텍스트 영역 크기 조정
                    //        textRectTransform.anchoredPosition = Vector2.zero; // 텍스트 위치 조정
                    //    }
                    //}
                    //else
                    //{
                    //    Debug.LogError("TMP_Text component not found in the button prefab.");
                    //}

                    // 버튼에 리스너 추가
                    Button btn = buttonPrefab.GetComponent<Button>();
                        btn.onClick.RemoveAllListeners();
                        int captureIndex = i; // 클로저에 사용될 인덱스 복사
                        btn.onClick.AddListener(() => ApplyStatUpgrade(selectedUpgrades[captureIndex]));
                        buttonPrefab.SetActive(true);

                        // 카드 애니메이션 재생
                        CardAnimation cardAnim = buttonPrefab.GetComponent<CardAnimation>();
                        if (cardAnim != null)
                        {
                            cardAnim.card = buttonPrefab;
                            string animationName = GetAnimationName(selectedUpgrades[i].name);
                            StartCoroutine(PlayCardAnimation(cardAnim, animationName));
                        }
                    }
                    else
                    {
                        specialUpgradeButtons[i].gameObject.SetActive(false);
                    }
                }
            }
            else // 일반 레벨업 처리
            {
                // 특별 레벨업 후에 일반 레벨업 팝업이 나오지 않도록 확인
                if (specialLevelUpPanel.activeSelf)
                {
                    return;
                }

                overlayPanel.SetActive(true);

                levelUpPopup.SetActive(true);           // 일반 레벨업 패널 활성화
                specialLevelUpPanel.SetActive(false);   // 특별 레벨업 패널 비활성화

                // 선택된 업그레이드에 해당하는 새로운 버튼을 생성하고 정보 설정
                for (int i = 0; i < selectedUpgrades.Count; i++)
                {
                    if (i < buttonPositions.Length)
                    {
                        GameObject buttonObject = Instantiate(selectedUpgrades[i].buttonPrefab, buttonPositions[i].position, Quaternion.identity, buttonPositions[i]);
                        buttonObject.transform.localPosition = Vector3.zero;            // 위치 조정
                        buttonObject.transform.localRotation = Quaternion.identity;     // 회전 조정
                        buttonObject.transform.localScale = Vector3.one;                // 크기 조정
                        SetupButton(buttonObject, selectedUpgrades[i]);                 // 버튼 초기화

                        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            rectTransform.sizeDelta = new Vector2(200, 300); // 원하는 크기로 설정
                        }

                        // 애니메이션 트리거 추가
                        CardAnimation cardAnim = buttonObject.GetComponent<CardAnimation>();
                        if (cardAnim != null)
                        {
                            cardAnim.card = buttonObject;
                            string animationName = GetAnimationName(selectedUpgrades[i].name);
                            StartCoroutine(PlayCardAnimation(cardAnim, animationName));
                        }

                        Button button = buttonObject.GetComponent<Button>();
                        button.gameObject.SetActive(true);
                        button.interactable = true; // 버튼 활성화
                        button.GetComponentInChildren<TMP_Text>().text = $"{selectedUpgrades[i].name} (+{selectedUpgrades[i].effect})";
                        button.GetComponentInChildren<TMP_Text>().fontSize = 18; // 원하는 텍스트 크기로 설정

                        // 텍스트의 RectTransform 조정
                        RectTransform textRectTransform = button.GetComponentInChildren<TMP_Text>().GetComponent<RectTransform>();
                        if (textRectTransform != null)
                        {
                            textRectTransform.anchorMin = new Vector2(0.1f, 0.1f);
                            textRectTransform.anchorMax = new Vector2(0.9f, 0.3f);
                            textRectTransform.offsetMin = new Vector2(0, 0);
                            textRectTransform.offsetMax = new Vector2(0, 0);
                        }

                        // 이벤트 리스너 설정
                        button.onClick.RemoveAllListeners();
                        int captureIndex = i; // 클로저에 사용될 인덱스 복사
                        button.onClick.AddListener(() => ApplyStatUpgrade(selectedUpgrades[captureIndex]));
                    }
                }
                overlayPanel.SetActive(true); // 오버레이 패널 활성화
            }

        }
        void SetupButton(GameObject buttonObject, StatUpgrade upgrade)
        {
            buttonObject.transform.localPosition = Vector3.zero;
            buttonObject.transform.localRotation = Quaternion.identity;
            buttonObject.transform.localScale = Vector3.one;


            Button button = buttonObject.GetComponent<Button>();
            button.gameObject.SetActive(true);
            button.interactable = true;

            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = $"{upgrade.name} (+{upgrade.effect})";
                buttonText.fontSize = 24; // 원하는 텍스트 크기로 설정
                                          // 텍스트의 RectTransform 조정
                                          // 텍스트의 RectTransform 조정
                                          // 텍스트의 RectTransform 조정
                RectTransform textRectTransform = buttonText.GetComponent<RectTransform>();
                if (textRectTransform != null)
                {
                    textRectTransform.anchorMin = new Vector2(0.1f, 0.1f);
                    textRectTransform.anchorMax = new Vector2(0.9f, 0.3f);
                    textRectTransform.offsetMin = new Vector2(0, 0);
                    textRectTransform.offsetMax = new Vector2(0, 0);
                }
            }
            else
            {
                Debug.LogError("TMP_Text component not found in the button object.");
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ApplyStatUpgrade(upgrade));

            // 애니메이션 트리거 추가
            CardAnimation cardAnim = buttonObject.GetComponent<CardAnimation>();
            if (cardAnim != null)
            {
                cardAnim.card = buttonObject;
                string animationName = GetAnimationName(upgrade.name);
                StartCoroutine(PlayCardAnimation(cardAnim, animationName));
            }
            else
            {
                Debug.LogError("CardAnimation component not found on the button object.");
            }
        }
        // 현재 레벨 화면에 표시
        void UpdateLevelDisplay()
        {
            if (currentLevel != null)
            {
                currentLevel.text = "Lvl " + Level;
            }
            else
            {
                Debug.LogError("currentLevel text component is not assigned!");
            }
        }

        List<StatUpgrade> SelectRandomStatUpgrades()
        {
            List<StatUpgrade> selected = new List<StatUpgrade>();
            HashSet<int> usedIndices = new HashSet<int>();
            int numUpgrades = Level % 10 == 1 ? 2 : 3; // 특별 레벨업이면 2개, 아니면 3개
            List<StatUpgrade> upgradeList = Level % 10 == 1 ? specialStatUpgrades : statUpgrades;

            while (selected.Count < numUpgrades && selected.Count < upgradeList.Count)
            {
                int index = UnityEngine.Random.Range(0, upgradeList.Count);
                if (!usedIndices.Contains(index))
                {
                    StatUpgrade upgrade = upgradeList[index];
                    if (Level % 10 == 1 && upgrade.name != "투사체 수 증가(중복)" && obtainedSpecialUpgrades.Contains(upgrade.name))
                    {
                        continue; // 이미 획득한 스페셜 업그레이드이면 건너뜁니다.
                    }

                    selected.Add(upgrade);
                    usedIndices.Add(index);
                }
            }
            return selected;
        }


        public void CloseLevelUpPopup()
        {
            overlayPanel.SetActive(false); // 오버레이 패널 비활성화
            levelUpPopup.SetActive(false);
            specialLevelUpPanel.SetActive(false); // 특별 레벨업 패널 비활성화
            isLevelUpPopupActive = false; // 레벨업 팝업 비활성화

            UpdateLevelDisplay(); // 레벨 표시를 업데이트합니다.
                                  // 경험치 바 최대값 업데이트


       

            // 스페셜 레벨업 패널의 각 버튼의 자식 오브젝트 제거
            foreach (Button button in specialUpgradeButtons)
            {
                foreach (Transform child in button.transform)
                {
                Destroy(child.gameObject);
                }
            }

            // 경험치 바 업데이트
            UpdateExperienceBar();

            if (!gameOverPanel.activeSelf)
            {
                ResumeGame(); // 게임 재개
            }
            UpdateLevelUpRequirement();

        }

        void PauseGame()
        {
            Time.timeScale = 0f; // 시간을 멈춥니다.
            isGamePaused = true;
        }

        void ResumeGame()
        {
            Time.timeScale = 1f; // 시간을 다시 시작합니다.
            isGamePaused = false;
            UpdateLevelDisplay(); // 레벨 표시를 업데이트합니다.

        }

        public void IncrementMonsterKillCount()
        {
            totalMonstersKilled++;
        }


        public void GameOver()
        {
            // 세팅 패널 비활성화
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            // 플레이어의 몬스터 처치 수, 도달한 레벨, 보너스 스탯을 PlayerPrefs에 저장
            PlayerPrefs.SetInt("TotalMonstersKilled", totalMonstersKilled);
            PlayerPrefs.SetInt("LevelReached", Level);
            float bonusStats = Mathf.Floor(Level * 0.1f);
            PlayerPrefs.SetFloat("BonusStats", bonusStats);

            // PlayTime 저장
            int playTime = (int)Time.timeSinceLevelLoad;
            PlayerPrefs.SetInt("PlayTime", playTime);

            // STATMANAGER의 스탯 정보도 저장
            StatManager.Instance.SaveStatsToPlayerPrefs();

            // GameOverUI 초기화 및 활성화
            GameOverUI gameOverUI = gameOverPanel.GetComponent<GameOverUI>();
            if (gameOverUI != null)
            {
                gameOverUI.Initialize();
                gameOverUI.ResetPanelSizeAndPosition(); // 패널 크기와 위치를 명시적으로 설정
            }
            else
            {
                Debug.LogError("GameOverUI 컴포넌트를 찾을 수 없습니다.");
            }

            // 게임 오버 패널을 활성화
            gameOverPanel.SetActive(true);

            // 모든 동작 멈추기
            PauseGame();

            // 모든 몬스터의 스프라이트 비활성화
            Monster[] monsters = FindObjectsOfType<Monster>();  // Monster로 변경
            foreach (Monster monster in monsters)
            {
                SpriteRenderer sr = monster.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.enabled = false;
                }
            }

            GameObject[] effects = GameObject.FindGameObjectsWithTag("Effect");
            foreach (GameObject effect in effects)
            {
                effect.SetActive(false);
            }

            // 기능 초기화
            ResetAppliedUpgrades();

            // BGM 중지
            if (AudioManager.Instance != null && AudioManager.Instance.bgmSource != null)
            {
                AudioManager.Instance.bgmSource.Stop();
            }
        }
        public void ReturnToMainScene()
        {
            Level = 20;
            NextLevelXP = 2.5f;
            balInstance.totalExperience = 0f; // 경험치 초기화

            StatManager.Instance.ResetUpgrades();
            StatManager.Instance.LoadStatsFromPlayerPrefs();

            Time.timeScale = 1f;
            SceneManager.LoadScene("Main/Main");

            // DontDestroyOnLoad 오브젝트 다시 설정
            DontDestroyOnLoad(gameObject);

            gameOverPanel.SetActive(false);

            // 레벨업 패널과 오버레이 패널 비활성화
            levelUpPopup.SetActive(false);
            overlayPanel.SetActive(false);
            specialLevelUpPanel.SetActive(false);

            isGamePaused = false; // 게임 일시정지 상태 해제

    }

    // 기능 초기화 메서드 추가
    private void ResetAppliedUpgrades()
        {
            if (balInstance != null)
            {
                balInstance.isDotActive = false;
                balInstance.isAoeActive = false;
                balInstance.isPdActive = false;
                balInstance.isTurretActive = false;
                balInstance.knockbackEnabled = false;

                balInstance.Dot = 5; 
                balInstance.Aoe = 10; 
                balInstance.Pd = 50.0f;
                balInstance.Chc = 0;
                balInstance.Chd = 120;
            }

            if (ballistaController != null)
            {
                ballistaController.numberOfArrows = 1; // 초기 투사체 수로 설정
            }

            if (ballistaController != null)
            {
                ballistaController.SetLineRendererEnabled(false);
            }

            if (turretObject != null)
            {
                turretObject.SetActive(false);
            }
        }

        private string GetAnimationName(string statName)
        {
            switch (statName)
            {
                case "피해량 증가 1":
                case "피해량 증가 2":
                case "피해량 증가 3":
                    return "UI_Dmg";
                case "재장전 시간 감소":
                    return "UI_Rt";
                case "투사체 속도 증가":
                    return "UI_As";
                case "치명타 확률 증가":
                    return "UI_Chc";
                case "치명타 피해량 증가 1":
                case "치명타 피해량 증가 2":
                    return "UI_Chd";
                case "지속 피해량 증가":
                    return "UI_Dot";
                case "범위 피해량 증가":
                    return "UI_Doa";
                case "관통 피해량 증가":
                    return "UI_Pd";
                case "자동 터렛 재장전 시간 감소":
                    return "UI_Tur_Rt";
                case "자동 터렛 피해량 증가":
                    return "UI_Tur_Dmg";
                case "경험치 배수 증가 1":
                case "경험치 배수 증가 2":
                    return "UI_Xpm";

                case "지속 피해 부여":
                    return "UI_20_Dot";
                case "범위 피해 부여":
                    return "UI_20_Doa";
                case "투사체 수 증가(중복)":
                    return "UI_20_Arr_Plus";
                case "투사체 관통 부여":
                    return "UI_20_Pd";
                case "조준 경로 표시":
                    return "UI_20_Direction";
                case "자동 터렛 생성":
                    return "UI_20_Tur_Spawn";
                case "투사체 넉백 부여":
                    return "UI_20_Knockback";
                default:
                    return "UI_Chc";
            }
        }

        private IEnumerator PlayCardAnimation(CardAnimation cardAnim, string animationName, float animationSpeed = 1.0f)
        {
            // 애니메이션 시작 전에 객체가 유효한지 확인
            if (cardAnim != null && cardAnim.card != null)
            {
                //AudioManager.Instance.effectSource.PlayOneShot(AudioManager.Instance.levelUpSound); // 효과음 재생
                // 레벨업 효과음 재생
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayLevelUpSound();
                }
                cardAnim.PlayAnimation(animationName, animationSpeed);
                yield return new WaitForSecondsRealtime(cardAnim.animationDuration);

                // 애니메이션이 끝난 후에 카드를 활성화합니다.
                if (cardAnim.card != null) // 다시 객체가 유효한지 확인
                {
                    cardAnim.card.SetActive(true);
                }
            }
        }

        public void ActivatePanel()
        {
            if (panelToActivate != null)
            {
                panelToActivate.SetActive(true); // 지정된 패널을 활성화
                PauseGame(); // 게임 일시 중지
            }
        }
        public void DeactivatePopup()
        {
            if (popupToDeactivate != null)
            {
                popupToDeactivate.SetActive(false); // 지정된 팝업을 비활성화
                ResumeGame(); // 게임 재개
            }
        }

    }
