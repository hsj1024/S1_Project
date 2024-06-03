using UnityEngine;

public class Eff_Explosion : MonoBehaviour
{
    private Bal balista;
    private Animator animator;

    private void Start()
    {
        balista = Bal.Instance;
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.speed *= 1.5f; // 애니메이션 속도를 1.5배로 증가
        }
        else
        {
            Debug.LogWarning("Animator component not found.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Monster"))
        {
            Monster monster = other.gameObject.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(balista.Aoe); // 범위 피해량 적용
            }
        }
    }
}
