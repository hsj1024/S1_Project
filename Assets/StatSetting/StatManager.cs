using UnityEngine;
using UnityEngine.UI; // UI ���� ����� ����ϱ� ���� �ʿ�
using TMPro; // TextMeshPro ���ӽ����̽� �߰�

public class StatManager : MonoBehaviour
{
    public TMP_Text DmgText; //  ���ط� 
    public TMP_Text RtText;  // ������ �ð�
    public TMP_Text xpmText; // ����ġ ���
    public TMP_Text TurretDmgText;  // �ͷ� ���ط�
    public TMP_Text pointsText;    // ����ڰ� ���� ����Ʈ�� ǥ��

    private Bal playerStats; // GameManager���� ������ Bal Ŭ����
    private int points = 100; // ����ڰ� �ʱ⿡ ������ �����ϴ� ����Ʈ
    private int pointsUsed = 0; // ���� ������ ���� ����Ʈ
    private int dmgUpgradeCount = 0; // ���ط� ���׷��̵� Ƚ��
    void Start()
    {
        // Scene ������ Bal ������Ʈ�� ���� ��ü�� ã�Ƽ� ����
        playerStats = FindObjectOfType<Bal>();
        if (playerStats == null)
        {
            Debug.LogError("Bal ������Ʈ�� ã�� �� �����ϴ�. Bal ������Ʈ�� �� ���� �ִ��� Ȯ���ϼ���.");
            return;
        }
        UpdateUI();
    }


    // UI ������Ʈ �޼���
    void UpdateUI()
    {
        DmgText.text = "���ط�: " + playerStats.Dmg.ToString();
        RtText.text = "������ �ð�: " + playerStats.Rt.ToString() + "s";
        xpmText.text = "����ġ ���: " + playerStats.XPM.ToString();
        TurretDmgText.text = "�ͷ� ���ط�: " + playerStats.TurretDmg.ToString();
        pointsText.text = "����Ʈ: " + (points - pointsUsed);
    }

    // ���ط� ���� 


    public void IncreaseDmg()
    {
        // ù ��° ���׷��̵���� ����� 3�� �ǵ��� ����
        int cost = 1 + ((dmgUpgradeCount + 1) * 2);
        if (playerStats != null && (points - pointsUsed) >= cost)
        {
            playerStats.Dmg += 1;
            points -= cost; // ����Ʈ ��� ����
            dmgUpgradeCount++; // ���׷��̵� Ƚ�� ����
            UpdateUI();
        }
    }


    // ������ �ð� ����
    public void DecreaseRt()
    {
        int cost = 1 + ((dmgUpgradeCount + 1) * 2);
        if (playerStats != null && (points - pointsUsed) >= cost)
        {
            playerStats.Rt -= 0.05f; // �ͷ� ���ط��� 1 ����
            points -= cost; // ����Ʈ ��� ����
            dmgUpgradeCount++; // ���׷��̵� Ƚ�� ����
            UpdateUI(); // UI ������Ʈ
        }
    }
    // ����ġ ��� ����
    public void IncreaseXPM()
    {
        int cost = 1 + ((dmgUpgradeCount + 1) * 3);
        if (playerStats != null && (points - pointsUsed) >= cost)
        {
            playerStats.XPM += 1;
            points -= cost; // ����Ʈ ��� ����
            dmgUpgradeCount++; // ���׷��̵� Ƚ�� ����
            UpdateUI();
        }
    }
    // �ͷ� ���ط� ���� 
    public void IncreaseTurretDmg()
    {
        int cost = 3 + ((dmgUpgradeCount + 1) * 4);
        if (playerStats != null && (points - pointsUsed) >= cost)
        {
            playerStats.TurretDmg += 1;
            points -= cost; // ����Ʈ ��� ����
            dmgUpgradeCount++; // ���׷��̵� Ƚ�� ����
            UpdateUI();
        }
    }

    // ����Ʈ ��ȯ�ϱ� ���

    public void ResetStatsAndRefundPoints()
    {
        if (playerStats != null)
        {
            playerStats.Dmg = 10;
            playerStats.Rt = 1.0f;
            playerStats.XPM = 1;
            playerStats.TurretDmg = 5;

            // ��� ���׷��̵� ��� ī���� �ʱ�ȭ
            dmgUpgradeCount = 0;

            // ���� ����Ʈ�� ����Ʈ �ʱ�ȭ
            pointsUsed = 0;
            points = 100 + pointsUsed; // ����Ʈ�� �ʱ� ���·� ����

            UpdateUI();
        }
    }

}
