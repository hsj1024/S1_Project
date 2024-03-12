using UnityEngine;
using UnityEngine.UI; // UI 관련 기능을 사용하기 위해 필요

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Bal playerStats; // Bal 클래스에 대한 참조

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        // 여기에서 Bal 클래스의 인스턴스를 초기화하거나 찾아서 할당할 수 있습니다.
        // 예: playerStats = FindObjectOfType<Bal>();
    }

    // 플레이어 스탯 증가 메서드 (예시로 TurretDmg, TurretRt, XPM 증가)
    public void IncreasePlayerStats(int amount)
    {
        if (playerStats != null)
        {
            playerStats.Dmg += amount; // 터렛 피해량 증가
            playerStats.Rt += amount; // 터렛 재장전 시간 증가
            playerStats.XPM += amount; // 경험치 배수 증가
        }
    }
}
