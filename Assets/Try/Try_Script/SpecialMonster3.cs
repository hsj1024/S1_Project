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

        // �� ����Ʈ�� ���� ���, �� �����Ӹ��� ��ġ�� ������Ʈ�Ͽ� ���͸� ��������� ����
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

            // �� ����Ʈ �������� ȣ���Ͽ� �ð��� ȿ���� �߰�
            if (healEffectPrefab != null)
            {
                // �� ����Ʈ �ν��Ͻ��� ������ �����ϰ� �θ� ����
                if (healEffectInstance == null)
                {
                    healEffectInstance = Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
                    healEffectInstance.transform.SetParent(transform); // �� ����Ʈ�� ������ �ڽ����� ����
                }

                // �� ����Ʈ�� ���̾� �� ���� ���� ����
                SpriteRenderer monsterRenderer = GetComponent<SpriteRenderer>();
                SpriteRenderer healEffectRenderer = healEffectInstance.GetComponent<SpriteRenderer>();

                if (healEffectRenderer != null && monsterRenderer != null)
                {
                    healEffectRenderer.sortingLayerName = monsterRenderer.sortingLayerName;
                    healEffectRenderer.sortingOrder = monsterRenderer.sortingOrder + 1;
                }

                // ���� �ð� �Ŀ� �� ����Ʈ ǥ�� ����
                yield return new WaitForSeconds(1.0f);
                Destroy(healEffectInstance); // ����Ʈ�� ����
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
