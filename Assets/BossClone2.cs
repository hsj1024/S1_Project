using System.Collections;
using UnityEngine;

public class BossClone2 : Monster
{
    private bool hasReachedCenter = false;
    public BossMonster BossMonster;
    private bool isDead = false; // 클론이 이미 죽었는지 여부를 추적

    void Start()
    {
        base.Start(); // Monster 클래스의 Start() 호출
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f); // 어두운 색상으로 설정
        audioManager = AudioManager.Instance;
    }

    public void SetBoss(BossMonster boss)
    {
        BossMonster = boss;
    }

    void Update()
    {
        if (!isKnockedBack && !hasReachedCenter)
        {
            MoveDown(); // 중앙에 도달하지 않았을 때만 이동
        }

        if (hasReachedCenter)
        {
            rb.velocity = Vector2.zero; // 중앙에 도달하면 정지
        }
    }

    public override void MoveDown()
    {
        if (isKnockedBack || (hp <= 0)) return;

        float speedScale = 0.02f;
        transform.position += Vector3.down * speed * speedScale * Time.deltaTime;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            float cameraCenterY = mainCamera.transform.position.y;

            if (transform.position.y <= cameraCenterY && !hasReachedCenter)
            {
                hasReachedCenter = true;
                rb.velocity = Vector2.zero; // 중앙에 도달했을 때 정지
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
        if (!isDead) // 이미 죽지 않았다면
        {
            isDead = true; // 죽음 플래그 설정
            Debug.Log("보스 클론 2 사망");

            if (BossMonster != null)
            {
                BossMonster.OnBossClone2Death();
            }

            Destroy(gameObject); // 클론 파괴
        }
    }

    public override void TakeDamageFromArrow(float damage, bool knockbackEnabled, Vector2 knockbackDirection, bool applyDot = false, int dotDamage = 0, bool isAoeHit = false)
    {
        if (!isDead) // 이미 죽은 클론에 대해서는 데미지 처리 안함
        {
            // 데미지 적용
            hp -= damage;

            if (hp <= 0)
            {
                OnCloneDeath(); // 체력이 0 이하이면 죽음 처리
            }
            else
            {
                // 보스는 hitPrefab이나 깜빡임 효과를 발생시키지 않음
                if (knockbackEnabled && !isKnockedBack && rb != null && !isAoeHit)
                {
                    ApplyKnockback(knockbackDirection); // 넉백 처리
                }

                if (applyDot && dotDamage > 0)
                {
                    ApplyDot(dotDamage); // 지속 데미지(DOT) 적용
                }
            }

            StartCoroutine(PlayArrowHitAnimation()); // 화살 맞았을 때 애니메이션 실행
        }
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
