using System.Collections;
using UnityEngine;

public class SpecialMonster : Monster
{
    private bool hasReachedCenter = false; // �߾� ���� ����

    private new void Start()
    {
        base.Start();
    }

    private new void Update()
    {
        if (!isKnockedBack && !hasReachedCenter)
        {
            MoveDown();
        }

        if (hasReachedCenter)
        {
            rb.velocity = Vector2.zero; // �߾ӿ� �����ϸ� ����
        }

        UpdateSortingOrder();

        if (currentHitInstance != null)
        {
            currentHitInstance.transform.position = transform.position;
        }

        if (fireEffectInstance != null)
        {
            fireEffectInstance.transform.position = transform.position;

            var fireSpriteRenderer = fireEffectInstance.GetComponent<SpriteRenderer>();
            if (fireSpriteRenderer != null)
            {
                fireSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            }
        }
    }

    public override void MoveDown()
    {
        if (isKnockedBack || hp <= 0) return;

        float speedScale = 0.02f;
        transform.position += Vector3.down * speed * speedScale * Time.deltaTime;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            float cameraCenterY = mainCamera.transform.position.y;

            // �߾ӿ� �����ߴ��� Ȯ��
            if (transform.position.y <= cameraCenterY && !hasReachedCenter)
            {
                hasReachedCenter = true;
                rb.velocity = Vector2.zero; // �̵� ����
                return;
            }
        }

        // ȭ�� �Ʒ��� ������� ��� ó��
        if (transform.position.y <= -5.0f)
        {
            if (!disableGameOver)
            {
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.GameOver();
                }
                else
                {
                    Debug.LogError("LevelManager.Instance is null");
                }
            }

            Destroy(gameObject);
        }
    }
}
