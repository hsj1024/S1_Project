using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Arr : MonoBehaviour
{
    public float damage;
    private Rigidbody2D rb;
    private Bal balista;

    private List<Monster> penetratedMonsters = new List<Monster>();

    public GameObject aoeSpritePrefab; // ���� ���� ��������Ʈ ������
    public float aoeAnimationDuration = 0.4f; // �ִϸ��̼� ����

    private void Start()
    {
        balista = Bal.Instance;
        if (balista != null)
        {
            damage = balista.Dmg;
        }

        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            Monster monster = collision.gameObject.GetComponent<Monster>();
            if (monster != null)
            {
                HandleMonsterHit(monster);
            }
        }
    }

    private void HandleMonsterHit(Monster monster)
    {
        if (!penetratedMonsters.Contains(monster))
        {
            bool knockbackEnabled = balista.knockbackEnabled;
            Vector2 knockbackDirection = rb.velocity.normalized;

            float finalDamage = damage;
            bool isCritical = false;

            if (balista.Chc > 0 && Random.value < balista.Chc * 0.01f)
            {
                finalDamage = balista.Chd * 0.1f;
                isCritical = true;
            }

            if (penetratedMonsters.Count == 0)
            {
                // ù ��° ����
                if (!isCritical && balista.isPdActive)
                {
                    finalDamage = balista.Pd * 0.01f * damage;
                }
                monster.TakeDamageFromArrow(finalDamage, knockbackEnabled, knockbackDirection);
            }
            else
            {
                // �� ��° ���ͺ���
                float penetrationDamage = isCritical ? balista.Pd * 0.01f * balista.Chd * 0.1f : balista.Pd * 0.01f * damage;
                monster.TakeDamageFromArrow(penetrationDamage, knockbackEnabled, knockbackDirection);
            }

            penetratedMonsters.Add(monster);

            if (balista.isDotActive)
            {
                monster.ApplyDot(balista.Dot); // ���� �������� ����
            }

            if (balista.isAoeActive)
            {
                // AOE ���� Ȱ��ȭ
                ActivateAoe(monster.transform.position);
            }

            if (knockbackEnabled)
            {
                monster.StartCoroutine(TemporarilyInvincible(monster));
            }
        }

        if (!balista.isPdActive)
        {
            Destroy(gameObject);
        }
    }


    private void ActivateAoe(Vector2 position)
    {
        if (aoeSpritePrefab != null)
        {
            GameObject aoeSprite = Instantiate(aoeSpritePrefab, position, Quaternion.identity);
            Eff_Explosion aoeEffect = aoeSprite.GetComponent<Eff_Explosion>();

            if (aoeEffect != null)
            {
                aoeEffect.damage = balista.Aoe; // AOE ������ ����
                aoeEffect.aoeAnimationDuration = aoeAnimationDuration;
                aoeEffect.applyDot = balista.isDotActive;
                aoeEffect.dotDamage = balista.Dot;
                aoeEffect.knockbackEnabled = balista.knockbackEnabled; // �˹� Ȱ��ȭ ���� ����
            }
            else
            {
                Debug.LogError("Eff_Explosion component not found on AOE sprite prefab.");
            }
        }
        else
        {
            Debug.LogError("aoeSpritePrefab is not assigned in the Inspector");
        }
    }

    private void Update()
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        if (screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator TemporarilyInvincible(Monster monster)
    {
        monster.invincible = true;
        yield return new WaitForSeconds(0.3f);
        monster.invincible = false;
    }
}
