using System.Collections;
using UnityEngine;

public class SpecialMonster3 : Monster
{
    private bool isHealing = false;
    private Color originalColor;
    private Color healColor = new Color(103f / 255f, 255f / 255f, 192f / 255f); // �̹������� ������ ����

    private new void Start()
    {
        base.Start();
        originalColor = spriteRenderer.color;
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
            float duration = 0.5f; // �׶��̼� ���� �ð�
            float elapsedTime = 0f;

            // ������ healColor�� ������ ����
            while (elapsedTime < duration)
            {
                spriteRenderer.color = Color.Lerp(originalColor, healColor, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            spriteRenderer.color = healColor;

            yield return new WaitForSeconds(0.5f);

            elapsedTime = 0f;

            // ������ ���� �������� ������ ����
            while (elapsedTime < duration)
            {
                spriteRenderer.color = Color.Lerp(healColor, originalColor, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            spriteRenderer.color = originalColor;
            isHealing = false;
        }


    }
}
