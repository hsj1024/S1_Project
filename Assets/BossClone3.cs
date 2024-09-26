using System.Collections;
using UnityEngine;

public class BossClone3 : Monster
{
    public GameObject healEffectPrefab; // 힐 이펙트 프리팹 추가
    private bool hasReachedCenter = false;
    private bool isHealing = false;
    public BossMonster BossMonster;

    public GameObject cloneFireEffectPrefab; // BossClone3 전용 Fire 이펙트 프리팹
    public GameObject cloneFireEffectInstance; // BossClone3 전용 Fire 이펙트 인스턴스
    public bool cloneIsOnFire = false; // BossClone3의 Fire 이펙트 활성화 여부

    void Start()
    {
        base.Start(); // Monster 클래스의 Start() 호출
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f); // 어두운 색상으로 설정
        audioManager = AudioManager.Instance;
        StartCoroutine(HealOverTime());
        invincible = false; // 클론은 무적이 아님
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

        if (cloneFireEffectInstance != null)
        {
            cloneFireEffectInstance.transform.position = transform.position;
        }
    }

    public void ApplyCloneFireEffect()
    {
        if (cloneFireEffectPrefab != null && !cloneIsOnFire)
        {
            cloneFireEffectInstance = Instantiate(cloneFireEffectPrefab, transform.position, Quaternion.identity);
            cloneFireEffectInstance.transform.SetParent(transform); // 클론에 붙이기
            cloneIsOnFire = true;
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

            // 힐 이펙트 프리팹을 호출하여 시각적 효과를 추가
            if (healEffectPrefab != null)
            {
                // 현재 몬스터의 위치에 힐 이펙트를 생성
                GameObject healEffectInstance = Instantiate(healEffectPrefab, transform.position, Quaternion.identity);

                // 일정 시간 후에 이펙트를 제거
                Destroy(healEffectInstance, 1.0f);
            }
            else
            {
                Debug.LogWarning("Heal effect prefab is not assigned!");
            }

            yield return new WaitForSeconds(1.0f); // 이펙트가 재생되는 시간만큼 대기
            isHealing = false;
        }
    }

    public override void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp > 0)
        {
            // 아직 살아있음
        }
        else if (hp <= 0)
        {
            OnCloneDeath(); // 보스 클론 3 사망 처리
        }
    }

    public override void TakeDamageFromArrow(float damage, bool knockbackEnabled, Vector2 knockbackDirection, bool applyDot = false, int dotDamage = 0, bool isAoeHit = false)
    {
        if (hp > 0)
        {
            hp -= damage;
            if (hp <= 0)
            {
                OnCloneDeath(); // 보스 클론 3 사망 처리
            }

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

    private void OnCloneDeath()
    {
        Debug.Log("보스 클론 3 사망");

        if (BossMonster != null)
        {
            BossMonster.OnBossClone3Death();
        }

        if (fireEffectInstance != null)
        {
            Destroy(fireEffectInstance);
            fireEffectInstance = null;
        }

        Destroy(gameObject);
    }

    // DOT 데미지를 보스 클론에 적용하는 메서드
    public override void ApplyDot(int dotDamage)
    {
        // BossClone3는 무적이 없으므로 DOT 데미지를 바로 적용
        if (cloneFireEffectPrefab != null && !cloneIsOnFire)
        {
            cloneFireEffectInstance = Instantiate(cloneFireEffectPrefab, transform.position, Quaternion.identity);
            cloneFireEffectInstance.transform.SetParent(transform); // 클론에 붙이기
            var fireSpriteRenderer = cloneFireEffectInstance.GetComponent<SpriteRenderer>();
            if (fireSpriteRenderer != null)
            {
                fireSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            }
            cloneIsOnFire = true;
        }

        // DOT 데미지 지속적으로 적용
        StartCoroutine(ApplyCloneDotDamage(dotDamage));
    }

    private IEnumerator ApplyCloneDotDamage(int dotDamage)
    {
        // BossClone3는 무적이 없으므로 매 틱마다 DOT 데미지를 바로 적용
        while (hp > 0)
        {
            hp -= dotDamage;

            yield return new WaitForSeconds(1.0f); // DOT 데미지가 1초마다 적용
        }

        if (hp <= 0)
        {
            if (cloneFireEffectInstance != null)
            {
                Destroy(cloneFireEffectInstance);
            }

            // DOT 데미지로 사망 시 OnCloneDeath 호출
            OnCloneDeath();
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
