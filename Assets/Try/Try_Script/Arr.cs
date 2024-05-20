using UnityEngine;

public class Arr : MonoBehaviour
{
    public int damage; // �߸���Ÿ�κ��� �޾ƿ� ������
    private Rigidbody2D rb; // Rigidbody2D ����

    private void Start()
    {
        // �߸���Ÿ�� ������ ������ �����ɴϴ�.
        Bal balista = Bal.Instance;
        if (balista != null)
        {
            damage = balista.Dmg; // �߸���Ÿ�� ������ ���� ����
        }

        // Rigidbody2D�� �����ɴϴ�.
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            Monster monster = collision.gameObject.GetComponent<Monster>();
            if (monster != null)
            {
                bool knockbackEnabled = Bal.Instance.knockbackEnabled;
                Vector2 knockbackDirection = rb.velocity.normalized; // �˹� ���� ����
                monster.TakeDamageFromArrow(damage, knockbackEnabled, knockbackDirection); // �˹� Ȱ��/��Ȱ�� ������ ����
                Debug.Log($"Knockback direction: {knockbackDirection}"); // ����� �α� �߰�
            }
            Destroy(gameObject); // ȭ�� ������Ʈ ����
        }
    }
}
