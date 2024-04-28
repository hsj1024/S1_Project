using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Bal playerStats;

    public int totalMonstersKilled = 0;
    public int levelReached = 1;


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

        SceneManager.sceneLoaded += OnSceneLoaded;  // 씬이 로드될 때마다 호출되는 이벤트에 메서드 연결
       
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 로드가 완료되면 필요한 컴포넌트를 다시 찾습니다.
        playerStats = FindObjectOfType<Bal>();
    
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
