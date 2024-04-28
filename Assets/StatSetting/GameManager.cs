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

        SceneManager.sceneLoaded += OnSceneLoaded;  // ���� �ε�� ������ ȣ��Ǵ� �̺�Ʈ�� �޼��� ����
       
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // �� �ε尡 �Ϸ�Ǹ� �ʿ��� ������Ʈ�� �ٽ� ã���ϴ�.
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
        // ��ü�� �ı��� �� �̺�Ʈ���� �޼��� ���� ����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
