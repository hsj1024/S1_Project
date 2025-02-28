using UnityEngine;

public class Eff_Explosion : MonoBehaviour
{
    public float damage;                      // ���� ������
    public float aoeAnimationDuration = 0.4f; // ���� ����Ʈ ���� �ð�
    public bool applyDot = false;             // ���� �� DOT ���� ����
    public int dotDamage = 0;                  // DOT ������
    public float explosionRadius = 1.5f;       // ���� ���� �ݰ�

    private void Start()
    {
        // ������ �ð� �� ���� ����Ʈ �ڵ� ����
        Destroy(gameObject, aoeAnimationDuration);

        // ���� ���� �� ���͵鿡�� ������ �� DOT ���� (�˹� ����)
        ApplyExplosionDamage(transform.position, damage);
    }

    private void ApplyExplosionDamage(Vector2 position, float damage)
    {
        Collider2D[] hitMonsters = Physics2D.OverlapCircleAll(position, explosionRadius);

        foreach (Collider2D collider in hitMonsters)
        {
            Monster monster = collider.GetComponent<Monster>();
            if (monster != null)
            {
                // �˹��� ����, �������� ����
                Vector2 noKnockbackDirection = Vector2.zero;
                monster.TakeDamageFromArrow(damage, false, noKnockbackDirection, applyDot, dotDamage, true);
            }
        }
    }
}
