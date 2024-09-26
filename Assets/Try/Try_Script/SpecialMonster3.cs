using System.Collections;
using UnityEngine;

public class SpecialMonster3 : Monster
{
    public GameObject healEffectPrefab; // 힐 이펙트 프리팹을 추가
    private bool isHealing = false;

    private new void Start()
    {
        base.Start();
        StartCoroutine(HealOverTime());
    }

    private IEnumerator HealOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(2.0f);

            if (hp < 30)
            {
                Heal(5);
            }
        }
    }

    private void Heal(float amount)
    {
        hp += amount;
        if (hp > 30)
        {
            hp = 30;
        }

        StartCoroutine(ShowHealEffect());
    }

    private IEnumerator ShowHealEffect()
    {
        if (!isHealing)
        {
            isHealing = true;

            // 힐 이펙트 프리팹을 호출하여 시각적 효과를 추가
            if (healEffectPrefab != null)
            {
                // 현재 몬스터의 위치에 힐 이펙트를 생성
                GameObject healEffectInstance = Instantiate(healEffectPrefab, transform.position, Quaternion.identity);

                // 일정 시간 후에 이펙트를 제거
                Destroy(healEffectInstance, 1.0f);
            }
            else
            {
                Debug.LogWarning("Heal effect prefab is not assigned!");
            }

            yield return new WaitForSeconds(1.0f); // 이펙트가 재생되는 시간만큼 대기
            isHealing = false;
        }
    }
}
