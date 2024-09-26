using System.Collections;
using UnityEngine;

public class SpecialMonster3 : Monster
{
    public GameObject healEffectPrefab; // �� ����Ʈ �������� �߰�
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

            // �� ����Ʈ �������� ȣ���Ͽ� �ð��� ȿ���� �߰�
            if (healEffectPrefab != null)
            {
                // ���� ������ ��ġ�� �� ����Ʈ�� ����
                GameObject healEffectInstance = Instantiate(healEffectPrefab, transform.position, Quaternion.identity);

                // ���� �ð� �Ŀ� ����Ʈ�� ����
                Destroy(healEffectInstance, 1.0f);
            }
            else
            {
                Debug.LogWarning("Heal effect prefab is not assigned!");
            }

            yield return new WaitForSeconds(1.0f); // ����Ʈ�� ����Ǵ� �ð���ŭ ���
            isHealing = false;
        }
    }
}
