using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Bal playerStats;

    public int totalMonstersKilled = 0;
    public int levelReached = 1;

 

    // 터렛 활성화 상태를 저장하기 위한 변수
    public bool isTurretActive = false;

    void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;  // 씬이 로드될 때마다 호출되는 이벤트에 메서드 연결
       
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 로드가 완료되면 필요한 컴포넌트를 다시 찾습니다.
        playerStats = FindObjectOfType<Bal>();
        // 씬이 로드될 때마다 터렛 활성화 상태를 확인합니다.
        if (playerStats != null)
        {
            playerStats.isTurretActive = isTurretActive;
        }
    }


    // 터렛 활성화 상태를 토글하는 메서드
    public void ToggleTurret()
    {
        // 터렛 활성화 상태를 반전시킵니다.
        isTurretActive = !isTurretActive;
        Debug.Log("Turret active state: " + isTurretActive);

        // 플레이어 스탯의 터렛 활성화 상태도 함께 업데이트합니다.
        if (playerStats != null)
        {
            playerStats.ToggleTurret();
        }
    }
    public void IncreasePlayerStats(int amount)
    {
        if (playerStats != null)
        {
            playerStats.Dmg += amount;
            playerStats.Rt += amount;
            playerStats.XPM += amount;
        }
    }

    void OnDestroy()
    {
        // 객체가 파괴될 때 이벤트에서 메서드 연결 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

  
}
