using UnityEngine;
using UnityEngine.UI;

public class BossKillButton : MonoBehaviour
{
    public BossMonster bossMonsterInstance; // 죽일 보스 몬스터의 참조

    void Start()
    {
        if (bossMonsterInstance == null)
        {
            Debug.LogError("BossMonster 인스턴스가 할당되지 않았습니다! Inspector에서 할당해 주세요.");
        }
    }

    // 버튼 클릭 시 호출될 메서드
    public void KillBoss()
    {
        if (bossMonsterInstance != null)
        {
            bossMonsterInstance.hp = 0; // 보스의 체력을 0으로 설정하여 죽음 트리거
            bossMonsterInstance.OnBossDeath(); // 보스 사망 메서드 호출
        }
    }
}
