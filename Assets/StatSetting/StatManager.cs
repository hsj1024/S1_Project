using UnityEngine;
using UnityEngine.UI; // UI 관련 기능을 사용하기 위해 필요
using TMPro; // TextMeshPro 네임스페이스 추가

public class StatManager : MonoBehaviour
{
    public TMP_Text DmgText; //  피해량 
    public TMP_Text RtText;  // 재장전 시간
    public TMP_Text xpmText; // 경험치 배수
    public TMP_Text TurretDmgText;  // 터렛 피해량
    public TMP_Text pointsText;    // 사용자가 가진 포인트를 표시

    private Bal playerStats; // GameManager에서 참조할 Bal 클래스
    private int points = 100; // 사용자가 초기에 가지고 시작하는 포인트
    private int pointsUsed = 0; // 스탯 증가에 사용된 포인트
    private int dmgUpgradeCount = 0; // 피해량 업그레이드 횟수
    void Start()
    {
        // Scene 내에서 Bal 컴포넌트를 가진 객체를 찾아서 참조
        playerStats = FindObjectOfType<Bal>();
        if (playerStats == null)
        {
            Debug.LogError("Bal 컴포넌트를 찾을 수 없습니다. Bal 컴포넌트가 씬 내에 있는지 확인하세요.");
            return;
        }
        UpdateUI();
    }


    // UI 업데이트 메서드
    void UpdateUI()
    {
        DmgText.text = "피해량: " + playerStats.Dmg.ToString();
        RtText.text = "재장전 시간: " + playerStats.Rt.ToString() + "s";
        xpmText.text = "경험치 배수: " + playerStats.XPM.ToString();
        TurretDmgText.text = "터렛 피해량: " + playerStats.TurretDmg.ToString();
        pointsText.text = "포인트: " + (points - pointsUsed);
    }

    // 피해량 증가 


    public void IncreaseDmg()
    {
        // 첫 번째 업그레이드부터 비용이 3이 되도록 조정
        int cost = 1 + ((dmgUpgradeCount + 1) * 2);
        if (playerStats != null && (points - pointsUsed) >= cost)
        {
            playerStats.Dmg += 1;
            points -= cost; // 포인트 사용 감소
            dmgUpgradeCount++; // 업그레이드 횟수 증가
            UpdateUI();
        }
    }


    // 재장전 시간 감소
    public void DecreaseRt()
    {
        int cost = 1 + ((dmgUpgradeCount + 1) * 2);
        if (playerStats != null && (points - pointsUsed) >= cost)
        {
            playerStats.Rt -= 0.05f; // 터렛 피해량을 1 감소
            points -= cost; // 포인트 사용 감소
            dmgUpgradeCount++; // 업그레이드 횟수 증가
            UpdateUI(); // UI 업데이트
        }
    }
    // 경험치 배수 증가
    public void IncreaseXPM()
    {
        int cost = 1 + ((dmgUpgradeCount + 1) * 3);
        if (playerStats != null && (points - pointsUsed) >= cost)
        {
            playerStats.XPM += 1;
            points -= cost; // 포인트 사용 감소
            dmgUpgradeCount++; // 업그레이드 횟수 증가
            UpdateUI();
        }
    }
    // 터렛 피해량 증가 
    public void IncreaseTurretDmg()
    {
        int cost = 3 + ((dmgUpgradeCount + 1) * 4);
        if (playerStats != null && (points - pointsUsed) >= cost)
        {
            playerStats.TurretDmg += 1;
            points -= cost; // 포인트 사용 감소
            dmgUpgradeCount++; // 업그레이드 횟수 증가
            UpdateUI();
        }
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
