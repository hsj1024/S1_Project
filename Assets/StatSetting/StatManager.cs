using UnityEngine;
using UnityEngine.UI; // UI 관련 기능을 사용하기 위해 필요
using TMPro; // TextMeshPro 네임스페이스 추가
using static LevelManager;
using UnityEngine.SceneManagement;

public class StatManager : MonoBehaviour
{
    public TMP_Text DmgText; //  피해량 
    public TMP_Text RtText;  // 재장전 시간
    public TMP_Text xpmText; // 경험치 배수
    public TMP_Text TurretDmgText;  // 터렛 피해량
    public TMP_Text pointsText;    // 사용자가 가진 포인트를 표시

    public TMP_Text DmgCostText; // 피해량 업그레이드 비용 표시
    public TMP_Text RtCostText;
    public TMP_Text xpmCostText;
    public TMP_Text TurretDmgCostText;
    public TMP_Text pointRefundText;


    public Button dmgUpButton; // 피해량 업그레이드 버튼
    public Button RtUpgradeButton;
    public Button xpmUpgradeButton;
    public Button TurretDmgUpgradeButton;
    public Button PointRefundButton;

    // 각 스탯마다 업그레이드 비용을 저장할 변수 추가
    public float dmgUpgradeCost = 3;
    public float rtUpgradeCost = 3;
    public float xpmUpgradeCost = 4;
    public float turretDmgUpgradeCost = 7;

    private Bal playerStats; // GameManager에서 참조할 Bal 클래스
    public float points = 100; // 사용자가 초기에 가지고 시작하는 포인트
    public float pointsUsed = 0; // 스탯 증가에 사용된 포인트

    public float dmgUpgradeCount = 0; // 피해량 업그레이드 횟수
    public float rtUpgradeCount = 0;
    public float xpmUpgradeCount = 0;
    public float turretDmgUpgradeCount = 0;

    private static StatManager instance;

    public GameObject buttonContainer; // 버튼이 배치될 컨테이너


    // 테스트
    private bool isPointsLoaded = false; // 포인트가 이미 로드되었는지 여부를 체크하는 플래그


    void Start()
    {
        // Scene 내에서 Bal 컴포넌트를 가진 객체를 찾아서 참조
        playerStats = FindObjectOfType<Bal>();
        if (playerStats == null)
        {
            Debug.LogError("Bal 컴포넌트를 찾을 수 없습니다. Bal 컴포넌트가 씬 내에 있는지 확인하세요.");
            return;
        }
        // 처음 시작 시 초기화
        ResetAllPlayerPrefs();
        
    }
    private void ResetAllPlayerPrefs()
    {
        PlayerPrefs.DeleteKey("dmgUpgradeCount");
        PlayerPrefs.DeleteKey("RtUpgradeCount");
        PlayerPrefs.DeleteKey("XpmUpgradeCount");
        PlayerPrefs.DeleteKey("TurretDmgUpgradeCount");
        PlayerPrefs.DeleteKey("PointsUsed");
        PlayerPrefs.DeleteKey("Points");
        points = 100;
    }

    public static StatManager Instance
    {
        get
        {
            // 인스턴스가 없으면 새로 생성
            if (instance == null)
            {
                // StatManager GameObject가 씬에 없으면 생성
                GameObject statManagerObject = new GameObject("StatManager");
                // StatManager 컴포넌트 추가
                instance = statManagerObject.AddComponent<StatManager>();
            }
            return instance;
        }
    }

    // Start 메서드는 싱글톤이기 때문에 Awake 메서드에서 초기화
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "StatSetting")
        {
            playerStats = FindObjectOfType<Bal>();

            // 필요한 경우 여기서 다시 데이터를 로드할 수 있습니다.
            //LoadStatsFromPlayerPrefs();

            CalculateUpgradeCosts();
            SetupButtons();
            SetupTexts();
            UpdateUI();
        }
    }
    public void SaveStatsToPlayerPrefs()
    {
        // PlayerPrefs에 스탯 정보를 저장
        PlayerPrefs.SetFloat("dmgUpgradeCount", dmgUpgradeCount);
        PlayerPrefs.SetFloat("RtUpgradeCount", rtUpgradeCount);
        PlayerPrefs.SetFloat("XpmUpgradeCount", xpmUpgradeCount);
        PlayerPrefs.SetFloat("TurretDmgUpgradeCount", turretDmgUpgradeCount);
        PlayerPrefs.SetFloat("PointsUsed", pointsUsed);
        PlayerPrefs.SetFloat("Points", points);
        PlayerPrefs.Save();

    }

    /*public void LoadStatsFromPlayerPrefs()
    {
        // PlayerPrefs에서 스탯 정보를 불러와서 변수에 설정
        dmgUpgradeCount = PlayerPrefs.GetFloat("dmgUpgradeCount", 0);
        rtUpgradeCount = PlayerPrefs.GetFloat("RtUpgradeCount", 0);
        xpmUpgradeCount = PlayerPrefs.GetFloat("XpmUpgradeCount", 0);
        turretDmgUpgradeCount = PlayerPrefs.GetFloat("TurretDmgUpgradeCount", 0);
        pointsUsed = PlayerPrefs.GetFloat("PointsUsed", 0);

        if (!PlayerPrefs.HasKey("Points"))
        {
            // 처음 로드하는 경우 기본 포인트를 100으로 설정
            points = PlayerPrefs.GetInt("Points", 100);
        }
        else
        {
            // 두 번째 이후 로드하는 경우 저장된 포인트를 불러옴
            points = PlayerPrefs.GetFloat("Points", 100) + PlayerPrefs.GetFloat("BonusStats", 0); // 추가된 부분
        }
        isPointsLoaded = true; // 포인트가 로드되었음을 표시

    }*/

    public void LoadStatsFromPlayerPrefs()
    {
        // PlayerPrefs에서 스탯 정보를 불러와서 변수에 설정
        dmgUpgradeCount = PlayerPrefs.GetFloat("dmgUpgradeCount", 0);
        rtUpgradeCount = PlayerPrefs.GetFloat("RtUpgradeCount", 0);
        xpmUpgradeCount = PlayerPrefs.GetFloat("XpmUpgradeCount", 0);
        turretDmgUpgradeCount = PlayerPrefs.GetFloat("TurretDmgUpgradeCount", 0);
        pointsUsed = PlayerPrefs.GetFloat("PointsUsed", 0);

        // 기본 포인트 설정 및 BonusStats 추가
        points = PlayerPrefs.GetFloat("Points", 100);

        // 보너스 스탯이 있다면 추가
        float bonusStats = PlayerPrefs.GetFloat("BonusStats", 0);
        points += bonusStats;  // 보너스 스탯을 포인트에 더함

        isPointsLoaded = true; // 포인트가 로드되었음을 표시
    }

    public void SetupButtons()
    {
        // 기존 버튼 찾기
        dmgUpButton = GameObject.Find("stat1_point").GetComponent<Button>();
        RtUpgradeButton = GameObject.Find("stat2_point").GetComponent<Button>();
        xpmUpgradeButton = GameObject.Find("stat3_point").GetComponent<Button>();
        TurretDmgUpgradeButton = GameObject.Find("stat4_point").GetComponent<Button>();
        PointRefundButton = GameObject.Find("포인트반환").GetComponent<Button>();

        // 각 버튼에 대한 이벤트 핸들러 추가
        dmgUpButton.onClick.AddListener(IncreaseDmg);
        RtUpgradeButton.onClick.AddListener(DecreaseRt);
        xpmUpgradeButton.onClick.AddListener(IncreaseXPM);
        TurretDmgUpgradeButton.onClick.AddListener(IncreaseTurretDmg);
        PointRefundButton.onClick.AddListener(ResetStatsAndRefundPoints);
    }
    public void SetupTexts()
    {
        DmgText = GameObject.Find("stat1Text").GetComponent<TMP_Text>();
        RtText = GameObject.Find("stat2Text").GetComponent<TMP_Text>();
        xpmText = GameObject.Find("stat3Text").GetComponent<TMP_Text>();
        TurretDmgText = GameObject.Find("stat4Text").GetComponent<TMP_Text>();
        pointsText = GameObject.Find("point_Text").GetComponent<TMP_Text>();

        DmgCostText = GameObject.Find("stat1Text1").GetComponent<TMP_Text>();
        RtCostText = GameObject.Find("stat2Text2").GetComponent<TMP_Text>();
        xpmCostText = GameObject.Find("stat3Text3").GetComponent<TMP_Text>();
        TurretDmgCostText = GameObject.Find("stat4Text4").GetComponent<TMP_Text>();
        pointRefundText = GameObject.Find("point_Text").GetComponent<TMP_Text>();
    }



    // UI 업데이트 메서드
    public void UpdateUI()
    {
        // 현재 씬의 이름을 확인하여 스탯 씬인 경우에만 UI 업데이트
        if (SceneManager.GetActiveScene().name == "StatSetting")
        {
            // 스탯 씬일 때만 UI 업데이트 수행
            /* DmgText.text = $"피해량: {playerStats.Dmg} -> {playerStats.Dmg + 1}";
             RtText.text = $"재장전 시간: {playerStats.Rt}s -> {(playerStats.Rt - 0.05f):F2}s";
             xpmText.text = $"경험치 배수: {playerStats.XPM:F1} -> {(playerStats.XPM + 0.2f):F1}";
             TurretDmgText.text = $"터렛 피해량: {playerStats.TurretDmg} -> {playerStats.TurretDmg + 2}";
             pointsText.text = "포인트: " + (points - pointsUsed);*/
            DmgText.text = $"피해량: {playerStats.Dmg} -> {playerStats.Dmg + 1}";
            RtText.text = $"재장전 시간: {playerStats.Rt.ToString("G")}s -> {(playerStats.Rt - 0.05f).ToString("G")}s";
            xpmText.text = $"경험치 배수: {playerStats.XPM.ToString("G")} -> {(playerStats.XPM + 0.2f).ToString("G")}";
            TurretDmgText.text = $"터렛 피해량: {playerStats.TurretDmg} -> {playerStats.TurretDmg + 2}";
            pointsText.text = "포인트: " + (points - pointsUsed).ToString("G");
            // 업그레이드 비용 텍스트 업데이트
            /* DmgCostText.text = dmgUpgradeCost.ToString();
             RtCostText.text = rtUpgradeCost.ToString();
             xpmCostText.text = xpmUpgradeCost.ToString();
             TurretDmgCostText.text = turretDmgUpgradeCost.ToString();*/

            DmgCostText.text = dmgUpgradeCost.ToString("G");
            RtCostText.text = rtUpgradeCost.ToString("G");
            xpmCostText.text = xpmUpgradeCost.ToString("G");
            TurretDmgCostText.text = turretDmgUpgradeCost.ToString("G");

            // 버튼 활성화 상태 업데이트
            dmgUpButton.interactable = (points - pointsUsed) >= dmgUpgradeCost;
            RtUpgradeButton.interactable = (points - pointsUsed) >= rtUpgradeCost;
            xpmUpgradeButton.interactable = (points - pointsUsed) >= xpmUpgradeCost;
            TurretDmgUpgradeButton.interactable = (points - pointsUsed) >= turretDmgUpgradeCost;
        }
    }

    public void ApplyUpgrade(StatUpgrade upgrade)
    {
        switch (upgrade.name)
        {
            case "피해량 증가 1":
                playerStats.Dmg += (int)upgrade.effect;
                break;
            case "피해량 증가 2":
                playerStats.Dmg += (int)upgrade.effect;
                break;
            case "피해량 증가 3":
                playerStats.Dmg += (int)upgrade.effect;
                break;
            case "재장전 시간 감소":
                playerStats.Rt += upgrade.effect;
                break;
            case "투사체 속도 증가":
                playerStats.As += (int)upgrade.effect; break;
            case "치명타 확률 증가":
                playerStats.Chc += (int)upgrade.effect; break;
                break;
            case "치명타 피해량 증가 1":
                playerStats.Chd += (int)upgrade.effect; break;
                break;
            case "치명타 피해량 증가 2":
                playerStats.Chd += (int)upgrade.effect; break;
                break;
            case "지속 피해량 증가":
                playerStats.Dot += (int)upgrade.effect; break;
                break;
            case "범위 피해량 증가":
                playerStats.Aoe += (int)upgrade.effect; break;
                break;
            case "관통 피해량 증가":
                playerStats.Pd += (int)upgrade.effect; break;
                break;

            case "자동 터렛 재장전 시간 감소":
                playerStats.TurretRt += (int)upgrade.effect; break;
                break;
            case "자동 터렛 피해량 증가":
                playerStats.TurretDmg += (int)upgrade.effect; break;
                break;
            case "경험치 배수 증가 1":
                playerStats.XPM += (float)upgrade.effect; break;
                break;
            case "경험치 배수 증가 2":
                playerStats.XPM += (int)upgrade.effect; break;
                break;
                // 추가적인 업그레이드에 대한 case 문을 추가하세요
              
        }

        // UI 업데이트
        UpdateUI();
    }


    // 피해량 증가 
    public void IncreaseDmg()
    {
        if (playerStats != null && (points - pointsUsed) >= dmgUpgradeCost)
        {
            // 업그레이드를 적용하기 전에 예상 값을 미리 보여줍니다.
            playerStats.Dmg += 1;


            pointsUsed += dmgUpgradeCost; // 포인트 사용 증가

            dmgUpgradeCount++; // 업그레이드 횟수 증가
            CalculateUpgradeCosts(); // 새로운 비용 계산

            UpdateUI();


        }
    }


    // 재장전 시간 감소
    public void DecreaseRt()
    {
        if (playerStats != null && (points - pointsUsed) >= rtUpgradeCost)
        {
            //playerStats.Rt -= 0.05f;
            playerStats.Rt = Mathf.Round((playerStats.Rt - 0.05f) * 100f) / 100f;

            pointsUsed += rtUpgradeCost;
            rtUpgradeCount++;
            CalculateUpgradeCosts();
            UpdateUI();
        }
    }
    // 경험치 배수 증가
    public void IncreaseXPM()
    {
        if (playerStats != null && (points - pointsUsed) >= xpmUpgradeCost)
        {
            playerStats.XPM += 0.2f;
            pointsUsed += xpmUpgradeCost;
            xpmUpgradeCount++;
            CalculateUpgradeCosts();
            UpdateUI();
        }
    }

    // 터렛 피해량 증가 
    public void IncreaseTurretDmg()
    {
        if (playerStats != null && (points - pointsUsed) >= turretDmgUpgradeCost)
        {
            playerStats.TurretDmg += 2;
            pointsUsed += turretDmgUpgradeCost;
            turretDmgUpgradeCount++;
            CalculateUpgradeCosts();
            UpdateUI();
        }
    }
    // 버튼 클릭 이벤트에 연결할 함수들
    public void OnIncreaseDmgButtonClick()
    {
        IncreaseDmg();
    }
    public void OnDecreaseRtButtonClick()
    {
        DecreaseRt();
    }

    public void OnIncreaseXPMButtonClick()
    {
        IncreaseXPM();
    }

    public void OnIncreaseTurretDmgButtonClick()
    {
        IncreaseTurretDmg();
    }
    public void OnResetStatscost()
    {
        ResetStatsAndRefundPoints();
    }
    // CalculateUpgradeCosts 함수도 각각의 비용을 제대로 계산하도록 수정합니다.
    void CalculateUpgradeCosts()
    {
        dmgUpgradeCost = 1 + ((dmgUpgradeCount + 1) * 2);
        rtUpgradeCost = 1 + ((rtUpgradeCount + 1) * 2);
        xpmUpgradeCost = 1 + ((xpmUpgradeCount + 1) * 3);
        turretDmgUpgradeCost = 3 + ((turretDmgUpgradeCount + 1) * 4);
    }


    public void ResetUpgrades()
    {


    }
    // 포인트 반환하기 기능

    public void ResetStatsAndRefundPoints()
    {
        if (playerStats != null)
        {
            playerStats.Dmg = 10;
            playerStats.Rt = 1.0f;
            playerStats.XPM = 1;
            playerStats.TurretDmg = 5;

            // 모든 업그레이드 비용 카운터 초기화
            dmgUpgradeCount = 0;
            rtUpgradeCount = 0;
            xpmUpgradeCount = 0;
            turretDmgUpgradeCount = 0;
            dmgUpgradeCost = 3;
            rtUpgradeCost = 3;
            xpmUpgradeCost = 4;
            turretDmgUpgradeCost = 7;

            // 사용된 포인트와 포인트 초기화
            pointsUsed = 0;
            points += pointsUsed;

            // PlayerPrefs 초기화
            PlayerPrefs.DeleteKey("dmgUpgradeCount");
            PlayerPrefs.DeleteKey("RtUpgradeCount");
            PlayerPrefs.DeleteKey("XpmUpgradeCount");
            PlayerPrefs.DeleteKey("TurretDmgUpgradeCount");
            PlayerPrefs.DeleteKey("PointsUsed");
            PlayerPrefs.DeleteKey("Points");
            UpdateUI();
        }
    }

    
}