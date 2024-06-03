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
                    monster.TakeDamage(balista.Aoe);

                    penetratedMonsters.Add(monster);

                    if (balista.isDotActive)
                    {
                        monster.ApplyDot(balista.Dot); // 지속 데미지를 설정
                    }

                    if (balista.isAoeActive)
                    {
                        //Debug.Log("Activating AOE Attack");
                        ActivateAoe(monster.transform.position);
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
            //Debug.Log("Instantiating AOE Sprite");
            GameObject aoeSprite = Instantiate(aoeSpritePrefab, position, Quaternion.identity);

            Collider2D aoeCollider = aoeSprite.GetComponent<Collider2D>();
            if (aoeCollider != null)
            {
                aoeCollider.enabled = true;
                StartCoroutine(DisableAoeCollider(aoeCollider));
            }
            else
            {
                Debug.LogError("aoeSpritePrefab does not have a Collider2D component.");
            }
            Destroy(aoeSprite, 0.7f); // 1초 후에 스프라이트 제거
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
