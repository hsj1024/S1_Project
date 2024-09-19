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
            monster.FadeOut(false, false);
            // ��鸲 ȿ�� ����
            StartCoroutine(ShakeAndReplace(0.2f)); // 0.2�� ���� ��鸲
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

        // ������ �μ��� �ٸ����̵� �������� ����
        GameObject brokenPrefab = isLeftBarricade ? leftBrokenPrefab : rightBrokenPrefab;
        GameObject brokenBarricade = Instantiate(brokenPrefab, transform.position, Quaternion.identity);

        // �μ��� �ٸ����̵忡 �ʿ��� ������Ʈ �߰�
        Rigidbody2D rb = brokenBarricade.AddComponent<Rigidbody2D>();
        rb.isKinematic = true; // �ʿ信 ���� ����

        var brokenCollider = brokenBarricade.GetComponent<Collider2D>();
        if (brokenCollider != null)
        {
            brokenCollider.isTrigger = false;
            BrokenBarricade brokenBarricadeScript = brokenBarricade.AddComponent<BrokenBarricade>();
            brokenBarricadeScript.StartInvincibility(0.3f); // 0.3�� ���� �ð� ����
        }
    }
}

// �μ��� �ٸ����̵�� ��ũ��Ʈ
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
        if (isInvincible) return; // ���� ������ �� �浹 ����

        Monster monster = collision.collider.GetComponent<Monster>();
        if (monster != null)
        {
            // ���͸� ��� ���̵� �ƿ���Ű��
            monster.FadeOut(false, false);

            // �μ��� �ٸ����̵� ��鸲 ȿ�� ����
            StartCoroutine(ShakeAndDisable(0.2f)); // 0.2�� ���� ��鸲
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

        // �μ��� �ٸ����̵� ��Ȱ��ȭ
        gameObject.SetActive(false);
    }


}

