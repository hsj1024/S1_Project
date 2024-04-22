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
    public GameObject levelUpPopup; // 레벨업 팝업 UI
    public Button closeButton; // 팝업의 닫기 버튼
    public GameObject overlayPanel; // 오버레이 패널 참조
    public Bal balInstance; // Bal 클래스의 인스턴스 참조

    private bool isGamePaused = false; // 게임 일시정지 여부
    public List<StatUpgrade> statUpgrades = new List<StatUpgrade>();
    private StatManager statManager; // StatManager에 대한 참조를 저장할 필드
    public static LevelManager Instance { get; private set; }
    // LevelManager 내에 버튼 연결 로직 추가
    public Button[] upgradeButtons; // 이 배열은 Inspector에서 초기화
    [System.Serializable]
    public class StatUpgrade
    {
        public string name;
        public float effect;
        public int probability;

        public StatUpgrade(string name, float effect, int probability)
        {
            this.name = name;
            this.effect = effect;
            this.probability = probability;
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
        if (statManager == null)
        {
            Debug.LogError("StatManager 컴포넌트를 찾을 수 없습니다.");
        }
        else
        {
            SetupButtons();
        }
    }
    void SetupButtons()
    {
        Debug.Log("SetupButtons called."); // 함수가 호출되었는지 확인하기 위한 로그

        List<StatUpgrade> selectedUpgrades = SelectRandomStatUpgrades();

        // 모든 버튼을 일단 활성화
        foreach (var button in upgradeButtons)
        {
            button.gameObject.SetActive(true);
            button.interactable = false; // 초기에는 비활성화
        }

        // 유효한 업그레이드가 있는 버튼만 활성화
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < selectedUpgrades.Count)
            {
                var upgrade = selectedUpgrades[i];
                upgradeButtons[i].GetComponentInChildren<TMP_Text>().text = $"{upgrade.name} (+{upgrade.effect})";
                upgradeButtons[i].onClick.RemoveAllListeners();
                //upgradeButtons[i].onClick.AddListener(() => ApplyStatUpgrade(upgrade));
                upgradeButtons[i].interactable = true; // 버튼을 활성화
            }
            else
            {
                upgradeButtons[i].GetComponentInChildren<TMP_Text>().text = "No Upgrade Available";
                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].interactable = false;  // 버튼을 비활성화
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
        /*SceneManager.LoadScene("Try", LoadSceneMode.Single);
        SceneManager.LoadScene("StatSetting", LoadSceneMode.Additive);*/

        // StatManager에 대한 싱글톤 인스턴스를 먼저 찾습니다.
        statManager = StatManager.Instance;
        InitializeStatUpgrades();

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

        // 초기 레벨업 요구 경험치 설정
        UpdateLevelUpRequirement();

        // 초기에는 레벨업 팝업을 비활성화
        levelUpPopup.SetActive(false);
        overlayPanel.SetActive(false);

        // 닫기 버튼에 이벤트 리스너 추가
        closeButton.onClick.AddListener(CloseLevelUpPopup);
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
        statUpgrades.Add(new StatUpgrade("피해량 증가 1", 2, 11));
        statUpgrades.Add(new StatUpgrade("피해량 증가 2", 3, 9));
        statUpgrades.Add(new StatUpgrade("피해량 증가 3", 5, 5));
        statUpgrades.Add(new StatUpgrade("재장전 시간 감소", -0.05f, 7));
        statUpgrades.Add(new StatUpgrade("투사체 속도 증가", 2, 7));

        statUpgrades.Add(new StatUpgrade("치명타 확률 증가", 5, 9));

        statUpgrades.Add(new StatUpgrade("치명타 피해량 증가 1", 15, 8));

        statUpgrades.Add(new StatUpgrade("치명타 피해량 증가 2", 25, 5));

        statUpgrades.Add(new StatUpgrade("지속 피해량 증가", 3, 7));
        statUpgrades.Add(new StatUpgrade("범위 피해량 증가", 5, 7));
        statUpgrades.Add(new StatUpgrade("관통 피해량 증가", 10, 7));
        
        statUpgrades.Add(new StatUpgrade("자동 터렛 재장전 시간 감소", -0.3f, 4));
        statUpgrades.Add(new StatUpgrade("자동 터렛 피해량 증가", 5, 4));
        statUpgrades.Add(new StatUpgrade("경험치 배수 증가 1", 0.2f, 7));
        statUpgrades.Add(new StatUpgrade("경험치 배수 증가 2", 0.4f, 3));


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
            NextLevelXP = (13 + (13 * Level)) / 2.0f; // 레벨 20에서 레벨 40까지
        }
        else if (Level >= 40)
        {
            NextLevelXP = (16 + (16 * Level)) / 2.0f; // 레벨 40 이상
        }
    }
    void ShowLevelUpPopup()
    {
        selectedUpgrades = SelectRandomStatUpgrades(); // selectedUpgrades를 초기화

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < selectedUpgrades.Count)
            {
                StatUpgrade upgrade = selectedUpgrades[i];
                upgradeButtons[i].GetComponentInChildren<TMP_Text>().text = $"{upgrade.name} (+{upgrade.effect})";
                upgradeButtons[i].onClick.RemoveAllListeners(); // 이전에 할당된 모든 리스너를 제거

                // 클로저 문제를 해결하기 위해 임시 변수에 업그레이드를 저장한 후 이를 클릭 이벤트에 전달
                StatUpgrade selectedUpgrade = upgrade;
                upgradeButtons[i].onClick.AddListener(() => ApplyStatUpgrade(selectedUpgrade));
                upgradeButtons[i].interactable = true; // 버튼 활성화
            }
            else
            {
                upgradeButtons[i].GetComponentInChildren<TMP_Text>().text = "No Upgrade Available";
                upgradeButtons[i].onClick.RemoveAllListeners(); // 이전에 할당된 모든 리스너를 제거
                upgradeButtons[i].interactable = false;  // 버튼 비활성화
            }
        }

        overlayPanel.SetActive(true);
        levelUpPopup.SetActive(true);
    }




    /*   public void ApplyStatUpgrade(StatUpgrade upgrade)
       {
           // StatManager의 인스턴스를 통해 업그레이드 적용
           if (StatManager.Instance != null)
           {
               StatManager.Instance.ApplyUpgrade(upgrade);
           }
           CloseLevelUpPopup();
       }*/


    List<StatUpgrade> SelectRandomStatUpgrades()
    {
        List<StatUpgrade> selected = new List<StatUpgrade>();
        HashSet<int> usedIndices = new HashSet<int>();  // 중복 선택 방지를 위한 HashSet

        while (selected.Count < 3 && selected.Count < statUpgrades.Count) // 더 많은 업그레이드를 선택할 수 없을 때 루프 종료 조건 추가
        {
            int index = UnityEngine.Random.Range(0, statUpgrades.Count);
            if (!usedIndices.Contains(index))
            {
                selected.Add(statUpgrades[index]);
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
}
