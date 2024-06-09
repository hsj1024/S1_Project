using UnityEngine;
using System.Collections;

public class Eff_Explosion : MonoBehaviour
{
    private Bal balista;
    private Animator animator;

    public int damage;
    public bool applyDot;
    public int dotDamage;
    public float aoeAnimationDuration; // 애니메이션 길이

    private CircleCollider2D aoeCollider;
    private LineRenderer aoeLineRenderer;

    private void Start()
    {
        balista = Bal.Instance;
        animator = GetComponent<Animator>();
        aoeCollider = gameObject.AddComponent<CircleCollider2D>();
        aoeCollider.isTrigger = true;
        aoeLineRenderer = gameObject.AddComponent<LineRenderer>();
        SetupLineRenderer(aoeLineRenderer);

        if (animator != null)
        {
            animator.speed *= 1.5f; // 애니메이션 속도를 1.5배로 증가
            animator.enabled = false; // Animator 비활성화
            StartCoroutine(ReenableAnimator()); // 일정 시간 후 Animator 다시 활성화
        }
        else
        {
            Debug.LogWarning("Animator component not found.");
        }

        Destroy(gameObject, aoeAnimationDuration); // 애니메이션 길이에 맞춰 스프라이트 제거
    }

    private IEnumerator ReenableAnimator()
    {
        yield return new WaitForSeconds(0.1f); // 0.1초 후 Animator 다시 활성화
        animator.enabled = true;
    }

    private void SetupLineRenderer(LineRenderer lineRenderer)
    {
        lineRenderer.positionCount = 50; // 원을 그릴 점의 수
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.loop = true;
    }

    private void DrawCircle(LineRenderer lineRenderer, float radius)
    {
        float angle = 2 * Mathf.PI / lineRenderer.positionCount;
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            float x = Mathf.Cos(i * angle) * radius;
            float y = Mathf.Sin(i * angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }

    private void FixedUpdate()
    {
        Vector3 currentScale = transform.localScale;
        Debug.Log($"Current Scale: {currentScale}"); // 현재 스케일 로그 출력

        aoeCollider.radius = Mathf.Max(currentScale.x, currentScale.y) / 2; // 최대 스케일을 반지름으로 사용
        DrawCircle(aoeLineRenderer, aoeCollider.radius); // LineRenderer로 원을 그림

        Debug.Log($"AOE Collider Radius: {aoeCollider.radius}");  // 현재 콜라이더 크기 출력

        // 현재 크기에 따른 콜라이더 범위 내의 몬스터에게 데미지 적용
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, aoeCollider.radius);
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Monster"))
            {
                Monster hitMonster = hitCollider.GetComponent<Monster>();
                if (hitMonster != null)
                {
                    // 지속 데미지 적용
                    if (applyDot)
                    {
                        hitMonster.ApplyDot(dotDamage);
                    }

                    hitMonster.TakeDamage(damage);  // 범위 공격 데미지 적용
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Monster"))
        {
            Monster monster = other.gameObject.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(damage); // 범위 피해량 적용

                if (applyDot)
                {
                    monster.ApplyDot(dotDamage);
                }
            }
        }
    }
}
