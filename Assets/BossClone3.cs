using System.Collections;
using UnityEngine;

public class BossClone3 : Monster
{
    private bool hasReachedCenter = false;
    private bool isHealing = false;
    private Color originalColor;
    private Color healColor = new Color(103f / 255f, 255f / 255f, 192f / 255f);
    public BossMonster BossMonster;

    void Start()
    {
        base.Start(); // Monster 클래스의 Start() 호출
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f); // 어두운 색상으로 설정
        originalColor = spriteRenderer.color;

        audioManager = AudioManager.Instance;
        StartCoroutine(HealOverTime());
    }

    public void SetBoss(BossMonster boss)
    {
        BossMonster = boss;
    }

    void Update()
    {
        base.Update(); // Monster 클래스의 Update() 호출

        if (!isKnockedBack && !hasReachedCenter)
        {
            MoveDown();
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

                // 보스와 상호작용: 보스가 다시 움직일 수 있게 함
                if (BossMonster != null)
                {
                    BossMonster.EnableBossMovementAgain();
                }
            }
        }

        if (transform.position.y <= -5.0f)
        {
            if (!disableGameOver)
            {
                LevelManager.Instance?.GameOver();
            }

            Destroy(gameObject);
        }
    }


    private IEnumerator HealOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(2.0f);

            if (hp < 300)
            {
                Heal(5);
            }
        }
    }

    private void Heal(float amount)
    {
        hp += amount;
        if (hp > 300)
        {
            hp = 300;
        }

        StartCoroutine(ShowHealEffect());
    }

    private IEnumerator ShowHealEffect()
    {
        if (!isHealing)
        {
            isHealing = true;
            float duration = 0.5f;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                spriteRenderer.color = Color.Lerp(originalColor, healColor, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            spriteRenderer.color = healColor;
            yield return new WaitForSeconds(0.5f);

            elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                spriteRenderer.color = Color.Lerp(healColor, originalColor, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            spriteRenderer.color = originalColor;
            isHealing = false;
        }
    }

    public override void TakeDamage(int damage)
    {
        if (!invincible)
        {
            hp -= damage;
            if (hp > 0)
            {

            }
            else if (hp <= 0)
            {
                OnCloneDeath(); // 보스가 죽었을 때 처리
            }
        }
    }

    private void OnCloneDeath()
    {
        BossMonster?.OnBossClone3Death();
        Destroy(gameObject);
    }

    public override void TakeDamageFromArrow(float damage, bool knockbackEnabled, Vector2 knockbackDirection, bool applyDot = false, int dotDamage = 0, bool isAoeHit = false)
    {
        if (hp > 0)
        {
            // 데미지 적용
            hp -= damage;

            if (hp <= 0)
            {
                OnCloneDeath(); // 보스가 죽었을 때 처리
            }

            // 보스는 hitPrefab이나 깜빡임 효과를 발생시키지 않음

            if (knockbackEnabled && !isKnockedBack && rb != null && !isAoeHit)
            {
                ApplyKnockback(knockbackDirection);
            }

            if (applyDot && dotDamage > 0)
            {
                ApplyDot(dotDamage);
            }
        }

        StartCoroutine(PlayArrowHitAnimation());
    }

    private IEnumerator PlayArrowHitAnimation()
    {
        if (hitAnimationPrefab != null)
        {
            Vector3 animationPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);
            GameObject animationInstance = Instantiate(hitAnimationPrefab, animationPosition, Quaternion.identity);
            Destroy(animationInstance, animationDuration);
        }

        if (audioManager != null)
        {
            audioManager.PlayMonsterHitSound();
        }

        yield return new WaitForSeconds(animationDuration);
    }


}
