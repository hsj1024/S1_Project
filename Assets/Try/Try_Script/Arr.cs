using UnityEngine;
using System.Collections.Generic;

public class Arr : MonoBehaviour
{
    public int damage; // 발리스타로부터 받아올 데미지
    private Rigidbody2D rb; // Rigidbody2D 참조
    private Bal balista; // 발리스타 인스턴스 참조

    private List<Monster> penetratedMonsters = new List<Monster>(); // 관통한 몬스터의 목록

    private void Start()
    {
        // 발리스타의 데미지 정보를 가져옵니다.
        balista = Bal.Instance;
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
                if (!penetratedMonsters.Contains(monster))
                {
                    bool knockbackEnabled = balista.knockbackEnabled;
                    Vector2 knockbackDirection = rb.velocity.normalized; // 넉백 방향 설정
                    monster.TakeDamageFromArrow(damage, knockbackEnabled, knockbackDirection); // 넉백 활성/비활성 정보를 전달
                    Debug.Log($"Knockback direction: {knockbackDirection}"); // 디버그 로그 추가

                    // 관통한 몬스터 목록에 추가
                    penetratedMonsters.Add(monster);
                }
            }

            // pd(관통 피해량)가 비활성화된 경우 화살을 제거
            if (!balista.isPdActive)
            {
                Destroy(gameObject); // 화살 오브젝트 제거
            }
        }
    }

    private void Update()
    {
        // 화살이 화면 밖으로 나가면 제거
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        if (screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1)
        {
            Destroy(gameObject);
        }
    }
}
