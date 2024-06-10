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
            monster.FadeOut();
            // 흔들림 효과 시작
            StartCoroutine(ShakeAndReplace(0.2f)); // 0.2초 동안 흔들림
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
        Instantiate(brokenPrefab, transform.position, Quaternion.identity);
    }
}
