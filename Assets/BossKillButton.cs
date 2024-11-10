using UnityEngine;
using UnityEngine.UI;

public class BossKillButton : MonoBehaviour
{
    public BossMonster bossMonsterInstance; // ���� ���� ������ ����

    void Start()
    {
        if (bossMonsterInstance == null)
        {
            Debug.LogError("BossMonster �ν��Ͻ��� �Ҵ���� �ʾҽ��ϴ�! Inspector���� �Ҵ��� �ּ���.");
        }
    }

    // ��ư Ŭ�� �� ȣ��� �޼���
    public void KillBoss()
    {
        if (bossMonsterInstance != null)
        {
            bossMonsterInstance.hp = 0; // ������ ü���� 0���� �����Ͽ� ���� Ʈ����
            bossMonsterInstance.OnBossDeath(); // ���� ��� �޼��� ȣ��
        }
    }
}
