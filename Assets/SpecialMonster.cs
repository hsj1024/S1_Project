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

    // �˹� ó�� �������̵�
    public override void ApplyKnockback(Vector2 knockbackDirection, bool destroyAfterKnockback = false)
    {
        // �̹� �߾ӿ� ���������� �˹� ���� X
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

    // �˹� �� ������� �̵� �簳
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

        // �˹� ���� �� �ٽ� ���� ������ �簳
        if (!hasReachedCenter && hp > 0)
        {
            MoveDown();  // ���� ������ �簳
        }
    }
}
