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
            animator.speed *= 1.5f; // �ִϸ��̼� �ӵ��� 1.5��� ����
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
                monster.TakeDamage(balista.Aoe); // ���� ���ط� ����
            }
        }
    }
}
