using UnityEngine;
using System.Collections;

public class Eff_Explosion : MonoBehaviour
{
    private Bal balista;
    private Animator animator;

    public int damage;
    public bool applyDot;
    public int dotDamage;
    public float aoeAnimationDuration; // �ִϸ��̼� ����

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
            animator.speed *= 1.5f; // �ִϸ��̼� �ӵ��� 1.5��� ����
            animator.enabled = false; // Animator ��Ȱ��ȭ
            StartCoroutine(ReenableAnimator()); // ���� �ð� �� Animator �ٽ� Ȱ��ȭ
        }
        else
        {
            Debug.LogWarning("Animator component not found.");
        }

        Destroy(gameObject, aoeAnimationDuration); // �ִϸ��̼� ���̿� ���� ��������Ʈ ����
    }

    private IEnumerator ReenableAnimator()
    {
        yield return new WaitForSeconds(0.1f); // 0.1�� �� Animator �ٽ� Ȱ��ȭ
        animator.enabled = true;
    }

    private void SetupLineRenderer(LineRenderer lineRenderer)
    {
        lineRenderer.positionCount = 50; // ���� �׸� ���� ��
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
        Debug.Log($"Current Scale: {currentScale}"); // ���� ������ �α� ���

        aoeCollider.radius = Mathf.Max(currentScale.x, currentScale.y) / 2; // �ִ� �������� ���������� ���
        DrawCircle(aoeLineRenderer, aoeCollider.radius); // LineRenderer�� ���� �׸�

        Debug.Log($"AOE Collider Radius: {aoeCollider.radius}");  // ���� �ݶ��̴� ũ�� ���

        // ���� ũ�⿡ ���� �ݶ��̴� ���� ���� ���Ϳ��� ������ ����
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, aoeCollider.radius);
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Monster"))
            {
                Monster hitMonster = hitCollider.GetComponent<Monster>();
                if (hitMonster != null)
                {
                    // ���� ������ ����
                    if (applyDot)
                    {
                        hitMonster.ApplyDot(dotDamage);
                    }

                    hitMonster.TakeDamage(damage);  // ���� ���� ������ ����
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
                monster.TakeDamage(damage); // ���� ���ط� ����

                if (applyDot)
                {
                    monster.ApplyDot(dotDamage);
                }
            }
        }
    }
}
