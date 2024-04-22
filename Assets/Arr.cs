using UnityEngine;

public class Arr : MonoBehaviour
{
   public int damage;  // 발리스타로부터 받아올 데미지

    private void Start()
    {
        // 발리스타의 데미지 정보를 가져옵니다.
        Bal balista = FindObjectOfType<Bal>();
        if (balista != null)
        {
            damage = balista.Dmg;  // 발리스타의 데미지 값을 설정
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
            Destroy(gameObject); // 화살 오브젝트 제거
        }
    }
}
