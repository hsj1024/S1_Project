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
                        ActivateAoe(monster.transform.position, knockbackEnabled, knockbackDirection, balista.isDotActive, balista.Dot);
                    }
                }

                if (!balista.isPdActive)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    private void ActivateAoe(Vector2 position, bool knockbackEnabled, Vector2 knockbackDirection, bool applyDot, int dotDamage)
    {
        if (aoeSpritePrefab != null)
        {
            GameObject aoeSprite = Instantiate(aoeSpritePrefab, position, Quaternion.identity);

            Collider2D aoeCollider = aoeSprite.GetComponent<Collider2D>();
            if (aoeCollider != null)
            {
                aoeCollider.enabled = true;

                // AOE 공격 범위 내의 모든 몬스터를 찾고, 넉백 및 지속 공격 적용
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, aoeCollider.bounds.extents.x);
                foreach (Collider2D hitCollider in hitColliders)
                {
                    if (hitCollider.CompareTag("Monster"))
                    {
                        Monster hitMonster = hitCollider.GetComponent<Monster>();
                        if (hitMonster != null)
                        {
                            hitMonster.TakeDamageFromArrow(damage, knockbackEnabled, knockbackDirection, applyDot, dotDamage);
                        }
                    }
                }

                StartCoroutine(DisableAoeCollider(aoeCollider));
            }
            else
            {
                Debug.LogError("aoeSpritePrefab does not have a Collider2D component.");
            }
            Destroy(aoeSprite, 0.7f); // 0.7초 후에 스프라이트 제거
        }
        else
        {
            Debug.LogError("aoeSpritePrefab is not assigned in the Inspector");
        }
    }

    private IEnumerator DisableAoeCollider(Collider2D aoeCollider)
    {
        yield return new WaitForSeconds(1.0f);
        aoeCollider.enabled = false;
    }

    private void Update()
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        if (screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1)
        {
            Destroy(gameObject);
        }
    }
}
