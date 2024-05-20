using UnityEngine;

public class Arr : MonoBehaviour
{
    public int damage; // 발리스타로부터 받아올 데미지
    private Rigidbody2D rb; // Rigidbody2D 참조

    private void Start()
    {
        // 발리스타의 데미지 정보를 가져옵니다.
        Bal balista = Bal.Instance;
        if (balista != null)
        {
            damage = balista.Dmg; // 발리스타의 데미지 값을 설정
        }

        // Rigidbody2D를 가져옵니다.
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
                Vector2 knockbackDirection = rb.velocity.normalized; // 넉백 방향 설정
                monster.TakeDamageFromArrow(damage, knockbackEnabled, knockbackDirection); // 넉백 활성/비활성 정보를 전달
                Debug.Log($"Knockback direction: {knockbackDirection}"); // 디버그 로그 추가
            }
            Destroy(gameObject); // 화살 오브젝트 제거
        }
    }
}
