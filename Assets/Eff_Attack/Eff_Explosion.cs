using System.Collections;
using UnityEngine;

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

    public bool knockbackEnabled;

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
            animator.Play("Eff_Explosion"); // 애니메이션 클립 재생
        }
        else
        {
            Debug.LogWarning("Animator component not found.");
        }

        Destroy(gameObject, aoeAnimationDuration); // 애니메이션 길이에 맞춰 스프라이트 제거
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

    private void Update()
    {
        Vector3 currentScale = transform.localScale;
        //Debug.Log($"Current Scale in Update: {currentScale}"); // 현재 스케일 로그 출력

        // 콜라이더 크기 조정
        aoeCollider.radius = Mathf.Max(currentScale.x, currentScale.y) * 12; // 최대 스케일을 반지름으로 사용
        DrawCircle(aoeLineRenderer, aoeCollider.radius); // LineRenderer로 원을 그림

        //Debug.Log($"AOE Collider Radius in Update: {aoeCollider.radius}");  // 현재 콜라이더 크기 출력
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Monster"))
        {
            Monster monster = other.gameObject.GetComponent<Monster>();
            if (monster != null)
            {
                Vector2 knockbackDirection = (monster.transform.position - transform.position).normalized;

                // 범위 피해량 적용 및 넉백 처리
                monster.TakeDamageFromArrow(damage, knockbackEnabled, knockbackDirection, applyDot, dotDamage, isAoeHit: true);

                if (knockbackEnabled)
                {
                    monster.StartCoroutine(TemporarilyInvincible(monster));
                }
            }
        }
    }

    private IEnumerator TemporarilyInvincible(Monster monster)
    {
        monster.invincible = true;
        yield return new WaitForSeconds(0.3f);
        monster.invincible = false;
    }
}
