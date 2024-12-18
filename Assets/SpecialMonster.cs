using System.Collections;
using UnityEngine;

public class SpecialMonster : Monster
{
    private bool hasReachedCenter = false; // 중앙 도달 여부

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
            rb.velocity = Vector2.zero; // 중앙에 도달하면 정지
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

            // 중앙에 도달했는지 확인
            if (transform.position.y <= cameraCenterY && !hasReachedCenter)
            {
                hasReachedCenter = true;
                rb.velocity = Vector2.zero; // 이동 정지
                return;
            }
        }

        // 화면 아래로 사라졌을 경우 처리
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
