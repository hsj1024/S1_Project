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
    public float aoeAnimationDuration = 1.0f; // 애니메이션 길이

    private Collider2D aoeCollider; // aoeCollider 변수 선언

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

            aoeCollider = aoeSprite.GetComponent<Collider2D>(); // aoeCollider 초기화
            Animator aoeAnimator = aoeSprite.GetComponent<Animator>();

            if (aoeCollider != null && aoeAnimator != null)
            {
                aoeCollider.enabled = true;

                // 콜라이더 크기를 조절하는 코루틴 시작
                StartCoroutine(SynchronizeColliderWithAnimation(aoeCollider, aoeAnimator, position, knockbackEnabled, applyDot, dotDamage));

                StartCoroutine(DisableAoeCollider(aoeCollider));
            }
            else
            {
                Debug.LogError("aoeSpritePrefab does not have a Collider2D or Animator component.");
            }
            Destroy(aoeSprite, aoeAnimationDuration); // 애니메이션 길이에 맞춰 스프라이트 제거
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
            // 애니메이션 클립의 현재 상태 정보를 가져옴
            AnimatorStateInfo stateInfo = aoeAnimator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = stateInfo.normalizedTime % 1; // 현재 애니메이션 클립 진행 상황 (0.0 ~ 1.0)

            // 애니메이션 클립에서 현재 크기 정보를 가져옴
            Vector3 currentScale = aoeAnimator.transform.localScale;
            aoeCollider.transform.localScale = currentScale;

            // 현재 크기에 따른 콜라이더 범위 내의 몬스터에게 데미지 및 넉백 적용
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, currentScale.x / 2); // 스케일의 반을 반지름으로 사용
            foreach (Collider2D hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Monster"))
                {
                    Monster hitMonster = hitCollider.GetComponent<Monster>();
                    if (hitMonster != null)
                    {
                        // 넉백 적용
                        if (knockbackEnabled)
                        {
                            Vector2 knockbackDir = (hitMonster.transform.position - (Vector3)position).normalized;
                            hitMonster.ApplyKnockback(knockbackDir, applyDot);
                        }

                        // 지속 데미지 적용
                        if (applyDot)
                        {
                            hitMonster.ApplyDot(dotDamage);
                        }

                        hitMonster.TakeDamage(damage);  // 범위 공격 데미지 적용
                    }
                }
            }

            Debug.Log($"AOE Collider Scale: {currentScale}");  // 현재 콜라이더 크기 출력

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // 애니메이션 종료 시 최대 크기로 설정
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
        // AOE 콜라이더의 현재 크기 표시
        if (aoeCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(aoeCollider.transform.position, aoeCollider.bounds.extents.x);
        }
    }
}
