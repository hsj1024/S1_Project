using UnityEngine;
using UnityEngine.UI; // UI ���� ����� ����ϱ� ���� �ʿ�

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Bal playerStats; // Bal Ŭ������ ���� ����

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

        // ���⿡�� Bal Ŭ������ �ν��Ͻ��� �ʱ�ȭ�ϰų� ã�Ƽ� �Ҵ��� �� �ֽ��ϴ�.
        // ��: playerStats = FindObjectOfType<Bal>();
    }

    // �÷��̾� ���� ���� �޼��� (���÷� TurretDmg, TurretRt, XPM ����)
    public void IncreasePlayerStats(int amount)
    {
        if (playerStats != null)
        {
            playerStats.Dmg += amount; // �ͷ� ���ط� ����
            playerStats.Rt += amount; // �ͷ� ������ �ð� ����
            playerStats.XPM += amount; // ����ġ ��� ����
        }
    }
}
