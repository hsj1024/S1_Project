using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Bal playerStats;

    public int totalMonstersKilled = 0;
    public int levelReached = 1;

    private int currentRound = 1; // ���� ȸ��


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

        AdjustMonsterHp();
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

    //�����߰�
    // ȸ�� ����
    public void IncrementRound()
    {
        currentRound++;
        Debug.Log($"Current Round: {currentRound}");
    }

    // ȸ���� ���� ���� HP ����
    private void AdjustMonsterHp()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        float multiplier = CalculateHpMultiplier(currentRound);

        foreach (GameObject monsterObject in monsters)
        {
            Monster monster = monsterObject.GetComponent<Monster>();
            if (monster != null)
            {
                // ��� ����(������ Ŭ�� ����) ü�� ����
                monster.AdjustHp(multiplier);
            }
        }

        PlayerPrefs.SetInt("CurrentRound", currentRound);
        PlayerPrefs.Save();
    }


    // ȸ���� HP ���� ���
    private float CalculateHpMultiplier(int round)
    {
        float multiplier;
        if (round == 1) multiplier = 1.0f;
        else if (round == 2) multiplier = 1.2f;
        else if (round == 3) multiplier = 1.4f;
        else if (round == 4) multiplier = 1.6f;
        else multiplier = 2.0f;

        return multiplier;
    }

}