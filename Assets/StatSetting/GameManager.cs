using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Bal playerStats;

    public int totalMonstersKilled = 0;
    public int levelReached = 1;

 

    // �ͷ� Ȱ��ȭ ���¸� �����ϱ� ���� ����
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

        SceneManager.sceneLoaded += OnSceneLoaded;  // ���� �ε�� ������ ȣ��Ǵ� �̺�Ʈ�� �޼��� ����
       
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // �� �ε尡 �Ϸ�Ǹ� �ʿ��� ������Ʈ�� �ٽ� ã���ϴ�.
        playerStats = FindObjectOfType<Bal>();
        // ���� �ε�� ������ �ͷ� Ȱ��ȭ ���¸� Ȯ���մϴ�.
        if (playerStats != null)
        {
            playerStats.isTurretActive = isTurretActive;
        }
    }


    // �ͷ� Ȱ��ȭ ���¸� ����ϴ� �޼���
    public void ToggleTurret()
    {
        // �ͷ� Ȱ��ȭ ���¸� ������ŵ�ϴ�.
        isTurretActive = !isTurretActive;
        Debug.Log("Turret active state: " + isTurretActive);

        // �÷��̾� ������ �ͷ� Ȱ��ȭ ���µ� �Բ� ������Ʈ�մϴ�.
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
        // ��ü�� �ı��� �� �̺�Ʈ���� �޼��� ���� ����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

  
}
