using UnityEngine;
using System.Collections.Generic;

public class Arr : MonoBehaviour
{
    public int damage; // �߸���Ÿ�κ��� �޾ƿ� ������
    private Rigidbody2D rb; // Rigidbody2D ����
    private Bal balista; // �߸���Ÿ �ν��Ͻ� ����

    private List<Monster> penetratedMonsters = new List<Monster>(); // ������ ������ ���

    private void Start()
    {
        // �߸���Ÿ�� ������ ������ �����ɴϴ�.
        balista = Bal.Instance;
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
                if (!penetratedMonsters.Contains(monster))
                {
                    bool knockbackEnabled = balista.knockbackEnabled;
                    Vector2 knockbackDirection = rb.velocity.normalized; // �˹� ���� ����
                    monster.TakeDamageFromArrow(damage, knockbackEnabled, knockbackDirection); // �˹� Ȱ��/��Ȱ�� ������ ����
                    Debug.Log($"Knockback direction: {knockbackDirection}"); // ����� �α� �߰�

                    // ������ ���� ��Ͽ� �߰�
                    penetratedMonsters.Add(monster);
                }
            }

            // pd(���� ���ط�)�� ��Ȱ��ȭ�� ��� ȭ���� ����
            if (!balista.isPdActive)
            {
                Destroy(gameObject); // ȭ�� ������Ʈ ����
            }
        }
    }

    private void Update()
    {
        // ȭ���� ȭ�� ������ ������ ����
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        if (screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1)
        {
            Destroy(gameObject);
        }
    }
}
