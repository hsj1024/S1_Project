using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Arr : MonoBehaviour
{
    public float damage;
    private Rigidbody2D rb;
    private Bal balista;

    private List<Monster> penetratedMonsters = new List<Monster>();

    public GameObject aoeSpritePrefab; // 범위 피해 스프라이트 프리팹
    public float aoeAnimationDuration = 0.4f; // 애니메이션 길이

    public GameObject criticalHitEffectPrefab; // 치명타 이펙트 프리팹 추가

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

            // 치명타 계산
            if (balista.Chc > 0 && Random.value < balista.Chc * 0.01f)
            {
                finalDamage = damage * balista.Chd * 0.01f; // 원본 데미지에 치명타 배율 적용
                isCritical = true;

                if (criticalHitEffectPrefab != null)
                {
                    GameObject criticalEffect = Instantiate(criticalHitEffectPrefab, monster.transform.position, Quaternion.identity);
                    Destroy(criticalEffect, 0.5f);
                }
            }

            if (penetratedMonsters.Count == 0)
            {
                if (!isCritical && balista.isPdActive)
                {
                    finalDamage += balista.Pd * 0.01f * damage; // 기본 데미지에 관통 데미지를 추가로 적용
                }
                monster.TakeDamageFromArrow(finalDamage, knockbackEnabled, knockbackDirection);
            }
            else
            {
                float penetrationDamage = isCritical
                    ? damage * balista.Chd * 0.01f * balista.Pd * 0.01f
                    : damage * balista.Pd * 0.01f;

                monster.TakeDamageFromArrow(penetrationDamage, knockbackEnabled, knockbackDirection);
            }


            penetratedMonsters.Add(monster);

            if (balista.isDotActive)
            {
                monster.ApplyDot(balista.Dot);
            }

            if (balista.isAoeActive)
            {
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
                aoeEffect.damage = balista.Aoe; // AOE 데미지 설정
                aoeEffect.aoeAnimationDuration = aoeAnimationDuration;
                aoeEffect.applyDot = balista.isDotActive;
                aoeEffect.dotDamage = balista.Dot;

                // 폭발음 재생
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayExplosionSound();
                }
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