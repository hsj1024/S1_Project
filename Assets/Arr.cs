using UnityEngine;

public class Arr : MonoBehaviour
{
   public int damage;  // �߸���Ÿ�κ��� �޾ƿ� ������

    private void Start()
    {
        // �߸���Ÿ�� ������ ������ �����ɴϴ�.
        Bal balista = FindObjectOfType<Bal>();
        if (balista != null)
        {
            damage = balista.Dmg;  // �߸���Ÿ�� ������ ���� ����
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            Monster monster = collision.gameObject.GetComponent<Monster>();
            if (monster != null)
            {
                //Debug.Log($"Arrow hitting {monster.monsterName} with {damage} damage.");
                //monster.TakeDamage(damage);
                //Debug.Log($"{monster.monsterName} has {monster.hp} HP left after being hit.");
            }
            Destroy(gameObject); // ȭ�� ������Ʈ ����
        }
    }
}
