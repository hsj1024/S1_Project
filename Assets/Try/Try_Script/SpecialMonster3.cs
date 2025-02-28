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

        // hp가 0 이하일 때 강제 사망 처리 보완
        if (hp <= 0 && !isFadingOut)
        {
            FadeOut(true, true, false);
            return;
        }

        // Heal 이펙트가 존재하면, 정렬 순서만 지속 업데이트 (위치는 부모에 맡김)
        if (healEffectInstance != null)
        {
            UpdateHealEffectSortingOrder();
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

            if (healEffectPrefab != null)
            {
                if (healEffectInstance == null)
                {
                    healEffectInstance = Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
                    healEffectInstance.transform.SetParent(transform);

                    // 몬스터 아래로 오프셋 적용 (로컬 좌표 기준)
                    healEffectInstance.transform.localPosition = new Vector3(0, -0.5f, 0);
                }

                UpdateHealEffectSortingOrder();

                yield return new WaitForSeconds(1.0f);

                Destroy(healEffectInstance);
                healEffectInstance = null;
            }
            else
            {
                Debug.LogWarning("Heal effect prefab is not assigned!");
            }

            isHealing = false;
        }
    }

    private void UpdateHealEffectSortingOrder()
    {
        if (healEffectInstance != null)
        {
            SpriteRenderer monsterRenderer = GetComponent<SpriteRenderer>();
            SpriteRenderer healEffectRenderer = healEffectInstance.GetComponent<SpriteRenderer>();

            if (healEffectRenderer != null && monsterRenderer != null)
            {
                healEffectRenderer.sortingLayerName = monsterRenderer.sortingLayerName;
                healEffectRenderer.sortingOrder = monsterRenderer.sortingOrder + 1;
            }
        }
    }
}
