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

    private StatManager statManager; // StatManager에 대한 참조를 저장할 필드
    public static LevelManager Instance { get; private set; }
    // LevelManager 내에 버튼 연결 로직 추가
    public Button[] upgradeButtons; // 이 배열은 Inspector에서 초기화

    public int totalMonstersKilled = 0; // 총 몬스터 처치 수를 저장할 변수
    public GameObject gameOverPanel;

    
    //버튼 

    public Transform[] buttonPositions; // 버튼 위치를 저장할 배열
    public List<GameObject> buttonPrefabs; // 스탯별로 다른 버튼 프리팹을 저장할 리스트

    public GameObject[] normalCardObjects; // 일반 레벨업 카드 게임 오브젝트 배열



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
        
        
            // currentLevel 텍스트를 찾아야 합니다.
        GameObject currentLevelObject = GameObject.Find("currentLevel");
        if (currentLevelObject != null)
        {
            currentLevel = currentLevelObject.GetComponent<Text>();
            if (currentLevel == null)
            {
                Debug.LogError("currentLevel 텍스트 컴포넌트를 찾을 수 없습니다.");
            }
        }
            
    

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
    }
    private List<StatUpgrade> selectedUpgrades = new List<StatUpgrade>(); // selectedUpgrades를 클래스 수준으로 이동

    void Start()
    {
        UpdateLevelDisplay(); // 게임 시작 시 레벨 표시를 업데이트


        // StatManager에 대한 싱글톤 인스턴스를 먼저 찾습니다.
        statManager = StatManager.Instance;
        InitializeStatUpgrades();
        InitializeSpecialStatUpgrades();

        // Bal 클래스의 인스턴스를 찾아 참조합니다.
        balInstance = FindObjectOfType<Bal>();
        if (balInstance == null)
        {
            Debug.LogError("Bal 클래스의 인스턴스를 찾을 수 없습니다.");
            return;
        }

        // StatManager의 인스턴스를 찾아 참조를 저장합니다.
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
        // 초기 레벨업 요구 경험치 설정
        UpdateLevelUpRequirement();

        // 초기에는 레벨업 팝업을 비활성화
        levelUpPopup.SetActive(false);
        overlayPanel.SetActive(false);

        // 닫기 버튼에 이벤트 리스너 추가
        //closeButton.onClick.AddListener(CloseLevelUpPopup);
    }
    public void ApplyStatUpgrade(StatUpgrade upgrade)
    {
        // StatManager의 인스턴스를 통해 업그레이드 적용
        if (StatManager.Instance != null)
        {
            StatManager.Instance.ApplyUpgrade(upgrade);
        }

        // 패널을 닫습니다.
        CloseLevelUpPopup();
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
        statUpgrades.Add(new StatUpgrade("경험치 배수 증가 1", 0.2f, 7, buttonPrefabs[13]));
        statUpgrades.Add(new StatUpgrade("경험치 배수 증가 2", 0.4f, 3, buttonPrefabs[14]));


    }

    void InitializeSpecialStatUpgrades()
    {
        // 특별 업그레이드 추가
        specialStatUpgrades.Add(new StatUpgrade("지속 피해 부여", 15, 15, specialButtonPrefabs[0]));
        specialStatUpgrades.Add(new StatUpgrade("범위 피해 부여", 15, 15, specialButtonPrefabs[1]));
        specialStatUpgrades.Add(new StatUpgrade("투사체 수 증가(중복)", 15, 15, specialButtonPrefabs[2]));
        specialStatUpgrades.Add(new StatUpgrade("투사체 관통 부여", 15, 15, specialButtonPrefabs[3]));
        specialStatUpgrades.Add(new StatUpgrade("조준 경로 표시", 15, 15, specialButtonPrefabs[4]));
        specialStatUpgrades.Add(new StatUpgrade("자동 터렛 생성", 15, 15, specialButtonPrefabs[5]));
        specialStatUpgrades.Add(new StatUpgrade("투사체 넉백 부여", 15, 15, specialButtonPrefabs[6]));

    }
    void Update()
    {
        // 게임이 일시정지 상태일 때는 레벨업을 확인하지 않습니다.
        if (!isGamePaused)
        {

            // 경험치를 주기적으로 확인하여 레벨업을 처리합니다.
            if (balInstance.totalExperience >= NextLevelXP)
            {


                Level++; // 레벨 증가
                UpdateLevelUpRequirement(); // 다음 레벨업 요구 경험치 업데이트
                PauseGame(); // 게임 일시정지

               
                ShowLevelUpPopup(); // 레벨업 팝업 표시
                Debug.Log($"Level Up! New level: {Level}, New XP Requirement: {NextLevelXP}");
            }


        }

    }

    void UpdateLevelUpRequirement()
    {
        UpdateLevelDisplay(); // 레벨업 요구 조건이 업데이트될 때 레벨 표시도 업데이트

        // 레벨별 레벨업 요구 경험치 설정
        if (Level == 1)
        {
            NextLevelXP = 2.5f; // 레벨 1에서 레벨 2로 가는 경우
        }
        else if (Level == 2)
        {
            NextLevelXP = 7.5f; // 레벨 2에서 레벨 3로 가는 경우
        }
        else if (Level >= 3 && Level < 20)
        {
            NextLevelXP = (5 + (10 * Level)) / 2.0f; // 레벨 3에서 레벨 20까지
        }
        else if (Level >= 20 && Level < 40)
        {
            NextLevelXP = 2.5f; // 레벨 20에서 레벨 40까지 테스트 하느라 수정 다시 고쳐야함
        }
        else if (Level >= 40)
        {
            NextLevelXP = (16 + (16 * Level)) / 2.0f; // 레벨 40 이상
        }
    }

    

    

    public void ShowLevelUpPopup()
    {
        List<StatUpgrade> selectedUpgrades = SelectRandomStatUpgrades();
        UpdateLevelDisplay(); // 레벨업 팝업을 보여줄 때 레벨 표시 업데이트


        // 모든 기존 버튼을 비활성화 및 제거
        for (int i = 0; i < buttonPositions.Length; i++)
        {
            if (buttonPositions[i].childCount > 0)
            {
                Destroy(buttonPositions[i].GetChild(0).gameObject);  // 기존에 있던 버튼 제거
            }
        }

        // 특별 레벨업인 경우
        if (Level % 20 == 1) // 특별 레벨업 조건
        {
            specialLevelUpPanel.SetActive(true); // 특별 레벨업 패널 활성화
            overlayPanel.SetActive(true);

            levelUpPopup.SetActive(false); // 일반 레벨업 패널 비활성화

            for (int i = 0; i < specialUpgradeButtons.Length; i++)
            {
                if (i < selectedUpgrades.Count)
                {
                    GameObject buttonPrefab = Instantiate(selectedUpgrades[i].buttonPrefab, specialUpgradeButtons[i].transform.position, Quaternion.identity, specialUpgradeButtons[i].transform);
                    buttonPrefab.transform.localPosition = Vector3.zero; // 위치 조정
                    buttonPrefab.transform.localRotation = Quaternion.identity; // 회전 조정
                    buttonPrefab.transform.localScale = Vector3.one; // 크기 조정

                    RectTransform rectTransform = buttonPrefab.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.sizeDelta = new Vector2(499, 600); // 원하는 크기로 설정
                    }

                    // 텍스트 업데이트
                    TMP_Text buttonText = buttonPrefab.GetComponentInChildren<TMP_Text>();
                    if (buttonText != null)
                    {
                        buttonText.text = $"{selectedUpgrades[i].name} (+{selectedUpgrades[i].effect})";
                    }
                    else
                    {
                        Debug.LogError("TMP_Text component not found in the button prefab.");
                    }

                    // 버튼에 리스너 추가
                    Button btn = buttonPrefab.GetComponent<Button>();
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => ApplyStatUpgrade(selectedUpgrades[i]));
                    buttonPrefab.SetActive(true);

                   /* // 카드 애니메이션 재생
                    CardAnimation cardAnim = buttonPrefab.GetComponent<CardAnimation>();
                    if (cardAnim != null)
                    {
                        cardAnim.PlayAnimation();
                    }*/
                }
                else
                {
                    specialUpgradeButtons[i].gameObject.SetActive(false);
                }
            }
        }
        else // 일반 레벨업 처리
        {
            overlayPanel.SetActive(true);

            levelUpPopup.SetActive(true); // 일반 레벨업 패널 활성화
            specialLevelUpPanel.SetActive(false); // 특별 레벨업 패널 비활성화

            // 선택된 업그레이드에 해당하는 새로운 버튼을 생성하고 정보 설정
            for (int i = 0; i < selectedUpgrades.Count; i++)
            {
                if (i < buttonPositions.Length)
                {
                    GameObject buttonObject = Instantiate(selectedUpgrades[i].buttonPrefab, buttonPositions[i].position, Quaternion.identity, buttonPositions[i]);
                    SetupButton(buttonObject, selectedUpgrades[i]); // 버튼 초기화
                    //buttonObject.transform.localPosition = Vector3.zero; // 위치 조정
                    //buttonObject.transform.localRotation = Quaternion.identity; // 회전 조정
                    //buttonObject.transform.localScale = Vector3.one; // 크기 조정

                    RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.sizeDelta = new Vector2(414, 600); // 원하는 크기로 설정
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

                    // 이벤트 리스너 설정
                    button.onClick.RemoveAllListeners();
                    int captureIndex = i; // 클로저에 사용될 인덱스 복사

                    button.onClick.AddListener(() => ApplyStatUpgrade(selectedUpgrades[captureIndex]));

                    
                }
            }

            overlayPanel.SetActive(true);
            levelUpPopup.SetActive(true);
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
        button.GetComponentInChildren<TMP_Text>().text = $"{upgrade.name} (+{upgrade.effect})";
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
            currentLevel.text = "Lvl: " + Level;
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
        int numUpgrades = Level % 20 == 1 ? 2 : 3; // 특별 레벨업이면 2개, 아니면 3개
        List<StatUpgrade> upgradeList = Level % 20 == 1 ? specialStatUpgrades : statUpgrades;

        while (selected.Count < numUpgrades && selected.Count < upgradeList.Count)
        {
            int index = UnityEngine.Random.Range(0, upgradeList.Count);
            if (!usedIndices.Contains(index))
            {
                selected.Add(upgradeList[index]);
                usedIndices.Add(index);
            }
        }

        return selected;
    }


    public void CloseLevelUpPopup()
    {
        overlayPanel.SetActive(false); // 오버레이 패널 비활성화
        levelUpPopup.SetActive(false);

        ResumeGame(); // 게임 재개
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
    }

    //하정 추가

    public void IncrementMonsterKillCount()
    {
        totalMonstersKilled++;
    }


    public void GameOver()
    {
        // 플레이어의 몬스터 처치 수, 도달한 레벨, 보너스 스탯을 PlayerPrefs에 저장
        PlayerPrefs.SetInt("TotalMonstersKilled", totalMonstersKilled);
        PlayerPrefs.SetInt("LevelReached", Level);
        float bonusStats = Mathf.Floor(Level * 0.1f);
        PlayerPrefs.SetFloat("BonusStats", bonusStats);

        // STATMANAGER의 스탯 정보도 저장
        PlayerPrefs.SetInt("DmgUpgradeCount", StatManager.Instance.dmgUpgradeCount);
        PlayerPrefs.SetInt("RtUpgradeCount", StatManager.Instance.rtUpgradeCount);
        PlayerPrefs.SetInt("XpmUpgradeCount", StatManager.Instance.xpmUpgradeCount);
        PlayerPrefs.SetInt("TurretDmgUpgradeCount", StatManager.Instance.turretDmgUpgradeCount);
        PlayerPrefs.SetInt("Points", StatManager.Instance.points);

        // 게임 오버 패널을 활성화
        gameOverPanel.SetActive(true);

        // 지정된 시간이 지난 후 메인 씬으로 돌아가는 코루틴 시작
        StartCoroutine(ReturnToMainAfterDelay(5f));
    }
    IEnumerator ReturnToMainAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        StatManager.Instance.LoadStatsFromPlayerPrefs();

        Time.timeScale = 1f;
        SceneManager.LoadScene("Main/Main");
        gameOverPanel.SetActive(false);
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
            default:
                return "UI_Chc";
        }
    }

    private IEnumerator PlayCardAnimation(CardAnimation cardAnim, string animationName, float animationSpeed = 1.0f)
    {
        // 애니메이션 시작 전에 객체가 유효한지 확인
        if (cardAnim != null && cardAnim.card != null)
        {
            cardAnim.PlayAnimation(animationName, animationSpeed);
            yield return new WaitForSecondsRealtime(cardAnim.animationDuration);

            // 애니메이션이 끝난 후에 카드를 활성화합니다.
            if (cardAnim.card != null) // 다시 객체가 유효한지 확인
            {
                cardAnim.card.SetActive(true);
            }
        }
    }



}
