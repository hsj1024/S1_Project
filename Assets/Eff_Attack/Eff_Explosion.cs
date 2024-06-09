using System.Collections;
using UnityEngine;

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
            animator.speed *= 1.5f; // �ִϸ��̼� �ӵ��� 1.5��� ����
            animator.Play("Eff_Explosion"); // �ִϸ��̼� Ŭ�� ���
        }
        else
        {
            Debug.LogWarning("Animator component not found.");
        }

        Destroy(gameObject, aoeAnimationDuration); // �ִϸ��̼� ���̿� ���� ��������Ʈ ����
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

    private void Update()
    {
        Vector3 currentScale = transform.localScale;
        //Debug.Log($"Current Scale in Update: {currentScale}"); // ���� ������ �α� ���

        // �ݶ��̴� ũ�� ����
        aoeCollider.radius = Mathf.Max(currentScale.x, currentScale.y) * 12; // �ִ� �������� ���������� ���
        DrawCircle(aoeLineRenderer, aoeCollider.radius); // LineRenderer�� ���� �׸�

        //Debug.Log($"AOE Collider Radius in Update: {aoeCollider.radius}");  // ���� �ݶ��̴� ũ�� ���
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Monster"))
        {
            Monster monster = other.gameObject.GetComponent<Monster>();
            if (monster != null)
            {
                Vector2 knockbackDirection = (monster.transform.position - transform.position).normalized;

                // ���� ���ط� ���� �� �˹� ó��
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
