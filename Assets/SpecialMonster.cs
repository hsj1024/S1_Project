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

        if (hp <= 0 && !isFadingOut)
        {
            FadeOut(true, true, false);
            return;
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

            if (transform.position.y <= cameraCenterY && !hasReachedCenter)
            {
                hasReachedCenter = true;
                rb.velocity = Vector2.zero;
                return;
            }
        }

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

    // 넉백 처리 오버라이드
    public override void ApplyKnockback(Vector2 knockbackDirection, bool destroyAfterKnockback = false)
    {
        // 이미 중앙에 도달했으면 넉백 적용 X
        if (hasReachedCenter) return;

        if (currentHitInstance != null)
        {
            Destroy(currentHitInstance);
        }
        currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);

        spriteRenderer.enabled = false;

        rb.velocity = Vector2.zero;

        Vector3 targetPosition = transform.position + (Vector3)(knockbackDirection.normalized * 0.5f);

        isKnockedBack = true;
        knockbackTimer = knockbackDuration;

        StartCoroutine(MoveToPosition(targetPosition, knockbackDuration, destroyAfterKnockback));
    }

    // 넉백 후 원래대로 이동 재개
    private IEnumerator MoveToPosition(Vector3 targetPosition, float duration, bool destroyAfterKnockback)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        isKnockedBack = false;
        spriteRenderer.enabled = true;

        Destroy(currentHitInstance);
        currentHitInstance = null;

        IgnoreCollisionsWithOtherMonsters(false);

        if (hp <= 0)
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.IncrementMonsterKillCount();
            }

            if (!isBoss && !isClone)
            {
                StartCoroutine(FadeOutAndDestroy(true, true, false));
            }
            else
            {
                OnDeath();
            }
        }

        // 넉백 끝난 후 다시 원래 움직임 재개
        if (!hasReachedCenter && hp > 0)
        {
            MoveDown();  // 원래 움직임 재개
        }
    }
}
