using System.Collections;
using UnityEngine;

public class SpecialMonster3 : Monster
{
    public GameObject healEffectPrefab; // 힐 이펙트 프리팹
    private GameObject healEffectInstance; // 힐 이펙트 인스턴스
    private bool isHealing = false;

    private new void Start()
    {
        base.Start();
        StartCoroutine(HealOverTime());
    }

    private void Update()
    {
        base.Update();

        // 힐 이펙트가 있을 경우, 매 프레임마다 위치를 업데이트하여 몬스터를 따라오도록 설정
        if (healEffectInstance != null)
        {
            healEffectInstance.transform.position = transform.position;
        }
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
                // 힐 이펙트 인스턴스가 없으면 생성하고 부모를 설정
                if (healEffectInstance == null)
                {
                    healEffectInstance = Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
                    healEffectInstance.transform.SetParent(transform); // 힐 이펙트를 몬스터의 자식으로 설정
                }

                // 힐 이펙트의 레이어 및 렌더 순서 설정
                SpriteRenderer monsterRenderer = GetComponent<SpriteRenderer>();
                SpriteRenderer healEffectRenderer = healEffectInstance.GetComponent<SpriteRenderer>();

                if (healEffectRenderer != null && monsterRenderer != null)
                {
                    healEffectRenderer.sortingLayerName = monsterRenderer.sortingLayerName;
                    healEffectRenderer.sortingOrder = monsterRenderer.sortingOrder + 1;
                }

                // 일정 시간 후에 힐 이펙트 표시 해제
                yield return new WaitForSeconds(1.0f);
                Destroy(healEffectInstance); // 이펙트를 제거
                healEffectInstance = null;
            }
            else
            {
                Debug.LogWarning("Heal effect prefab is not assigned!");
            }

            isHealing = false;
        }
    }
}
