using System.Collections;
using UnityEngine;

public class Barricade : MonoBehaviour
{
    public GameObject leftBrokenPrefab;    // ���� �ٸ����̵��� �μ��� ���� ������
    public GameObject rightBrokenPrefab;   // ������ �ٸ����̵��� �μ��� ���� ������
    public bool isLeftBarricade;           // �� �ٸ����̵尡 ���ʿ� ��ġ�ϴ��� ���θ� �����ϴ� �÷���

    void OnCollisionEnter2D(Collision2D collision)
    {
        Monster monster = collision.collider.GetComponent<Monster>();
        if (monster != null)
        {
            // ���͸� ��� ���̵� �ƿ���Ű��
            monster.FadeOut();
            // ��鸲 ȿ�� ����
            StartCoroutine(ShakeAndReplace(0.2f)); // 0.2�� ���� ��鸲
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

        // ������ �μ��� �ٸ����̵� �������� ����
        GameObject brokenPrefab = isLeftBarricade ? leftBrokenPrefab : rightBrokenPrefab;
        Instantiate(brokenPrefab, transform.position, Quaternion.identity);
    }
}
