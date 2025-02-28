using UnityEngine;

public class Eff_Explosion : MonoBehaviour
{
    public float damage;                      // 폭발 데미지
    public float aoeAnimationDuration = 0.4f; // 폭발 이펙트 지속 시간
    public bool applyDot = false;             // 폭발 시 DOT 적용 여부
    public int dotDamage = 0;                  // DOT 데미지
    public float explosionRadius = 1.5f;       // 폭발 범위 반경

    private void Start()
    {
        // 지정된 시간 후 폭발 이펙트 자동 제거
        Destroy(gameObject, aoeAnimationDuration);

        // 폭발 범위 내 몬스터들에게 데미지 및 DOT 적용 (넉백 없음)
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
                // 넉백은 제거, 데미지만 적용
                Vector2 noKnockbackDirection = Vector2.zero;
                monster.TakeDamageFromArrow(damage, false, noKnockbackDirection, applyDot, dotDamage, true);
            }
        }
    }
}
