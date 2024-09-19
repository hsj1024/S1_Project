using System.Collections;
using UnityEngine;

public class Barricade : MonoBehaviour
{
    public GameObject leftBrokenPrefab;    // 왼쪽 바리케이드의 부서진 상태 프리팹
    public GameObject rightBrokenPrefab;   // 오른쪽 바리케이드의 부서진 상태 프리팹
    public bool isLeftBarricade;           // 이 바리케이드가 왼쪽에 위치하는지 여부를 결정하는 플래그

    void OnCollisionEnter2D(Collision2D collision)
    {
        Monster monster = collision.collider.GetComponent<Monster>();
        if (monster != null)
        {
            // 몬스터를 즉시 페이드 아웃시키기
            monster.FadeOut(false, false);
            // 흔들림 효과 시작
            StartCoroutine(ShakeAndReplace(0.2f)); // 0.2초 동안 흔들림
            AudioManager.Instance.PlaySecondMonsterHitSound();

        }
    }

    private IEnumerator ShakeAndReplace(float duration)
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-0.1f, 0.1f);
            transform.position = new Vector3(originalPosition.x + x, originalPosition.y, originalPosition.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;

        Destroy(gameObject);

        // 적절한 부서진 바리케이드 프리팹을 생성
        GameObject brokenPrefab = isLeftBarricade ? leftBrokenPrefab : rightBrokenPrefab;
        GameObject brokenBarricade = Instantiate(brokenPrefab, transform.position, Quaternion.identity);

        // 부서진 바리케이드에 필요한 컴포넌트 추가
        Rigidbody2D rb = brokenBarricade.AddComponent<Rigidbody2D>();
        rb.isKinematic = true; // 필요에 따라 설정

        var brokenCollider = brokenBarricade.GetComponent<Collider2D>();
        if (brokenCollider != null)
        {
            brokenCollider.isTrigger = false;
            BrokenBarricade brokenBarricadeScript = brokenBarricade.AddComponent<BrokenBarricade>();
            brokenBarricadeScript.StartInvincibility(0.3f); // 0.3초 무적 시간 설정
        }
    }
}

// 부서진 바리케이드용 스크립트
public class BrokenBarricade : MonoBehaviour
{
    private bool isInvincible = false;

    public void StartInvincibility(float duration)
    {
        StartCoroutine(InvincibilityCoroutine(duration));
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isInvincible) return; // 무적 상태일 때 충돌 무시

        Monster monster = collision.collider.GetComponent<Monster>();
        if (monster != null)
        {
            // 몬스터를 즉시 페이드 아웃시키기
            monster.FadeOut(false, false);

            // 부서진 바리케이드 흔들림 효과 시작
            StartCoroutine(ShakeAndDisable(0.2f)); // 0.2초 동안 흔들림
            AudioManager.Instance.PlaySecondMonsterHitSound();

        }
    }

    private IEnumerator ShakeAndDisable(float duration)
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-0.1f, 0.1f);
            transform.position = new Vector3(originalPosition.x + x, originalPosition.y, originalPosition.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;

        // 부서진 바리케이드 비활성화
        gameObject.SetActive(false);
    }


}

