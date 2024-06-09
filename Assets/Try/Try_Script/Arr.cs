using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Arr : MonoBehaviour
{
    public int damage;
    private Rigidbody2D rb;
    private Bal balista;

    private List<Monster> penetratedMonsters = new List<Monster>();

    public GameObject aoeSpritePrefab; // 범위 피해 스프라이트 프리팹
    public float aoeAnimationDuration = 0.6f; // 애니메이션 길이

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
                if (!penetratedMonsters.Contains(monster))
                {
                    bool knockbackEnabled = balista.knockbackEnabled;
                    Vector2 knockbackDirection = rb.velocity.normalized;

                    monster.TakeDamageFromArrow(damage, knockbackEnabled, knockbackDirection);
                    penetratedMonsters.Add(monster);

                    if (balista.isDotActive)
                    {
                        monster.ApplyDot(balista.Dot); // 지속 데미지를 설정
                    }

                    if (balista.isAoeActive)
                    {
                        // AOE 공격 활성화
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
                aoeEffect.knockbackEnabled = balista.knockbackEnabled; // 넉백 활성화 여부 전달
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
