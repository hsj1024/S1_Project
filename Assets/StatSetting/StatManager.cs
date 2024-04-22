using UnityEngine;
using UnityEngine.UI; // UI 관련 기능을 사용하기 위해 필요
using TMPro; // TextMeshPro 네임스페이스 추가
using static LevelManager;

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


    public Button DmgUpgradeButton; // 피해량 업그레이드 버튼
    public Button RtUpgradeButton;
    public Button xpmUpgradeButton;
    public Button TurretDmgUpgradeButton;

    // 각 스탯마다 업그레이드 비용을 저장할 변수 추가
    private int dmgUpgradeCost;
    private int rtUpgradeCost;
    private int xpmUpgradeCost;
    private int turretDmgUpgradeCost;

    private Bal playerStats; // GameManager에서 참조할 Bal 클래스
    private int points = 100; // 사용자가 초기에 가지고 시작하는 포인트
    private int pointsUsed = 0; // 스탯 증가에 사용된 포인트


    private int dmgUpgradeCount = 0; // 피해량 업그레이드 횟수
    private int rtUpgradeCount = 0;
    private int xpmUpgradeCount = 0;
    private int turretDmgUpgradeCount = 0;

    private static StatManager instance;

    void Start()
    {
        // Scene 내에서 Bal 컴포넌트를 가진 객체를 찾아서 참조
        playerStats = FindObjectOfType<Bal>();
        if (playerStats == null)
        {
            Debug.LogError("Bal 컴포넌트를 찾을 수 없습니다. Bal 컴포넌트가 씬 내에 있는지 확인하세요.");
            return;
        }
        CalculateUpgradeCosts(); // 초기 비용 계산

        UpdateUI();
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
        // 현재 인스턴스가 존재하지 않으면
        if (instance == null)
        {
            // 이 객체를 인스턴스로 설정하고
            instance = this;
            // 씬 전환 시 파괴되지 않도록 설정합니다.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 이미 다른 인스턴스가 있으면 이 인스턴스를 파괴합니다.
            Destroy(gameObject);
        }
    }


    // UI 업데이트 메서드
    void UpdateUI()
    {
        DmgText.text = $"피해량: {playerStats.Dmg} -> {playerStats.Dmg + 1}";
        RtText.text = $"재장전 시간: {playerStats.Rt}s -> {(playerStats.Rt - 0.05f):F2}s";
        xpmText.text = $"경험치 배수: {playerStats.XPM:F1} -> {(playerStats.XPM + 0.2f):F1}";
        TurretDmgText.text = $"터렛 피해량: {playerStats.TurretDmg} -> {playerStats.TurretDmg + 2}";
        pointsText.text = "포인트: " + (points - pointsUsed);

        // 업그레이드 비용 텍스트 업데이트
        DmgCostText.text = dmgUpgradeCost.ToString();
        RtCostText.text = rtUpgradeCost.ToString(); // RtCostText는 UI에 추가해야 함
        xpmCostText.text = xpmUpgradeCost.ToString(); // xpmCostText는 UI에 추가해야 함
        TurretDmgCostText.text = turretDmgUpgradeCost.ToString(); // TurretDmgCostText는 UI에 추가해야 함

        // 버튼 활성화 상태 업데이트
        DmgUpgradeButton.interactable = (points - pointsUsed) >= dmgUpgradeCost;
        RtUpgradeButton.interactable = (points - pointsUsed) >= rtUpgradeCost; // RtUpgradeButton은 UI에 추가해야 함
        xpmUpgradeButton.interactable = (points - pointsUsed) >= xpmUpgradeCost; // xpmUpgradeButton은 UI에 추가해야 함
        TurretDmgUpgradeButton.interactable = (points - pointsUsed) >= turretDmgUpgradeCost; // TurretDmgUpgradeButton은 UI에 추가해야 함
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
            /*case "자동 터렛 개수 증가":
                playerStats.Chc += (int)upgrade.effect; break;
                break;*/
            case "자동 터렛 재장전 시간 감소":
                playerStats.TurretRt += (int)upgrade.effect; break;
                break;
            case "자동 터렛 피해량 증가":
                playerStats.TurretDmg += (int)upgrade.effect; break;
                break;
            case "경험치 배수 증가 1":
                playerStats.XPM += (int)upgrade.effect; break;
                break;
            case "경험치 배수 증가 2":
                playerStats.XPM += (int)upgrade.effect; break;
                break;
            // 추가적인 업그레이드에 대한 case 문을 추가하세요
            default:
                Debug.LogError("해당하는 업그레이드가 없습니다.");
                break;
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
            playerStats.Rt -= 0.05f;
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

    // CalculateUpgradeCosts 함수도 각각의 비용을 제대로 계산하도록 수정합니다.
    void CalculateUpgradeCosts()
    {
        dmgUpgradeCost = 1 + ((dmgUpgradeCount + 1) * 2);
        rtUpgradeCost = 1 + ((rtUpgradeCount + 1) * 2);
        xpmUpgradeCost = 1 + ((xpmUpgradeCount + 1) * 3);
        turretDmgUpgradeCost = 3 + ((turretDmgUpgradeCount + 1) * 4);
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

            // 사용된 포인트와 포인트 초기화
            pointsUsed = 0;
            points = 100 + pointsUsed; // 포인트를 초기 상태로 복원

            UpdateUI();
        }
    }

}