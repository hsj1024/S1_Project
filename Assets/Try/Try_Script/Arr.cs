using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Arr : MonoBehaviour
{
    public int damage;
    private Rigidbody2D rb;
    private Bal balista;

    private List<Monster> penetratedMonsters = new List<Monster>();

    public GameObject aoeSpritePrefab; // ���� ���� ��������Ʈ ������
    public float aoeAnimationDuration = 1.0f; // �ִϸ��̼� ����

    private Collider2D aoeCollider; // aoeCollider ���� ����

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
                        monster.ApplyDot(balista.Dot); // ���� �������� ����
                    }

                    if (balista.isAoeActive)
                    {
                        // AOE ���� Ȱ��ȭ
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

            aoeCollider = aoeSprite.GetComponent<Collider2D>(); // aoeCollider �ʱ�ȭ
            Animator aoeAnimator = aoeSprite.GetComponent<Animator>();

            if (aoeCollider != null && aoeAnimator != null)
            {
                aoeCollider.enabled = true;

                // �ݶ��̴� ũ�⸦ �����ϴ� �ڷ�ƾ ����
                StartCoroutine(SynchronizeColliderWithAnimation(aoeCollider, aoeAnimator, position, knockbackEnabled, applyDot, dotDamage));

                StartCoroutine(DisableAoeCollider(aoeCollider));
            }
            else
            {
                Debug.LogError("aoeSpritePrefab does not have a Collider2D or Animator component.");
            }
            Destroy(aoeSprite, aoeAnimationDuration); // �ִϸ��̼� ���̿� ���� ��������Ʈ ����
        }
        else
        {
            Debug.LogError("aoeSpritePrefab is not assigned in the Inspector");
        }
    }

    private IEnumerator SynchronizeColliderWithAnimation(Collider2D aoeCollider, Animator aoeAnimator, Vector2 position, bool knockbackEnabled, bool applyDot, int dotDamage)
    {
        float timeElapsed = 0f;
        while (timeElapsed < aoeAnimationDuration)
        {
            // �ִϸ��̼� Ŭ���� ���� ���� ������ ������
            AnimatorStateInfo stateInfo = aoeAnimator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = stateInfo.normalizedTime % 1; // ���� �ִϸ��̼� Ŭ�� ���� ��Ȳ (0.0 ~ 1.0)

            // �ִϸ��̼� Ŭ������ ���� ũ�� ������ ������
            Vector3 currentScale = aoeAnimator.transform.localScale;
            aoeCollider.transform.localScale = currentScale;

            // ���� ũ�⿡ ���� �ݶ��̴� ���� ���� ���Ϳ��� ������ �� �˹� ����
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, currentScale.x / 2); // �������� ���� ���������� ���
            foreach (Collider2D hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Monster"))
                {
                    Monster hitMonster = hitCollider.GetComponent<Monster>();
                    if (hitMonster != null)
                    {
                        // �˹� ����
                        if (knockbackEnabled)
                        {
                            Vector2 knockbackDir = (hitMonster.transform.position - (Vector3)position).normalized;
                            hitMonster.ApplyKnockback(knockbackDir, applyDot);
                        }

                        // ���� ������ ����
                        if (applyDot)
                        {
                            hitMonster.ApplyDot(dotDamage);
                        }

                        hitMonster.TakeDamage(damage);  // ���� ���� ������ ����
                    }
                }
            }

            Debug.Log($"AOE Collider Scale: {currentScale}");  // ���� �ݶ��̴� ũ�� ���

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // �ִϸ��̼� ���� �� �ִ� ũ��� ����
        Vector3 finalScale = aoeAnimator.transform.localScale;
        aoeCollider.transform.localScale = finalScale;
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

    private void OnDrawGizmos()
    {
        // AOE �ݶ��̴��� ���� ũ�� ǥ��
        if (aoeCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(aoeCollider.transform.position, aoeCollider.bounds.extents.x);
        }
    }
}
