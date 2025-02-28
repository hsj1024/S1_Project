using System.Collections;
using UnityEngine;

public class SpecialMonster3 : Monster
{
    public GameObject healEffectPrefab; // �� ����Ʈ ������
    private GameObject healEffectInstance; // �� ����Ʈ �ν��Ͻ�
    private bool isHealing = false;

    private new void Start()
    {
        base.Start();
        StartCoroutine(HealOverTime());
    }

    private void Update()
    {
        base.Update();

        // hp�� 0 ������ �� ���� ��� ó�� ����
        if (hp <= 0 && !isFadingOut)
        {
            FadeOut(true, true, false);
            return;
        }

        // Heal ����Ʈ�� �����ϸ�, ���� ������ ���� ������Ʈ (��ġ�� �θ� �ñ�)
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

                    // ���� �Ʒ��� ������ ���� (���� ��ǥ ����)
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
