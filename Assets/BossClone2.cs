using System.Collections;
using UnityEngine;

public class BossClone2 : Monster
{
    private bool hasReachedCenter = false;
    public BossMonster BossMonster;
    private bool isDead = false; // 클론이 이미 죽었는지 여부를 추적

    public GameObject cloneFireEffectPrefab; // BossClone2 전용 Fire 이펙트 프리팹
    public GameObject cloneFireEffectInstance; // BossClone2 전용 Fire 이펙트 인스턴스
    public bool cloneIsOnFire = false; // BossClone2의 Fire 이펙트 활성화 여부

    void Start()
    {
        base.Start();
        originalHp = hp; // 초기 체력을 originalHp로 저장

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f); // 어두운 색상으로 설정
        audioManager = AudioManager.Instance;

        invincible = false; // 클론은 무적이 아님
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

        if (cloneFireEffectInstance != null)
        {
            cloneFireEffectInstance.transform.position = transform.position;
        }
        UpdateSortingOrder();
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

    public void ApplyCloneFireEffect()
    {
        if (cloneFireEffectPrefab != null && !cloneIsOnFire)
        {
            cloneFireEffectInstance = Instantiate(cloneFireEffectPrefab, transform.position, Quaternion.identity);
            cloneFireEffectInstance.transform.SetParent(transform); // 클론에 붙이기
            cloneIsOnFire = true;
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
            OnCloneDeath(); // 보스 클론 2 사망 처리
        }
    }

    public override void TakeDamageFromArrow(float damage, bool knockbackEnabled, Vector2 knockbackDirection, bool applyDot = false, int dotDamage = 0, bool isAoeHit = false)
    {
        if (hp > 0)
        {
            hp -= damage;

            if (hp <= 0)
            {
                OnCloneDeath(); // 사망 처리
                return;
            }

            if (knockbackEnabled && !isKnockedBack && rb != null && !isAoeHit)
            {
                // 넉백 적용 후 다시 아래로 이동하도록 코루틴 호출
                StartCoroutine(HandleKnockbackAndResume(knockbackDirection));
            }

            if (applyDot && dotDamage > 0)
            {
                ApplyDot(dotDamage);
            }
        }

        StartCoroutine(PlayArrowHitAnimation());
    }

    private IEnumerator HandleKnockbackAndResume(Vector2 knockbackDirection)
    {
        // 넉백 처리
        isKnockedBack = true;
        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration); // 넉백 지속 시간 대기

        // 넉백 상태 해제 및 속도 초기화
        isKnockedBack = false;
        rb.velocity = Vector2.zero;

        // 중앙에 도달하지 않았고 사망하지 않은 경우 아래로 이동 재개
        if (!hasReachedCenter && hp > 0)
        {
            MoveDown();
        }
    }


    private void OnCloneDeath()
    {
        // Debug.Log("보스 클론 2 사망");

        // 상태 초기화
        isKnockedBack = false; // 넉백 상태 초기화
        rb.velocity = Vector2.zero; // 남은 물리 속도 제거

        // 보스와 연결된 동작 처리
        if (BossMonster != null)
        {
            BossMonster.OnBossClone2Death();
        }

        // 효과 제거
        if (fireEffectInstance != null)
        {
            Destroy(fireEffectInstance);
            fireEffectInstance = null;
        }

        if (cloneFireEffectInstance != null)
        {
            Destroy(cloneFireEffectInstance);
            cloneFireEffectInstance = null;
        }

        Destroy(gameObject); // 시체 제거
    }

    // DOT 데미지를 보스 클론에 적용하는 메서드
    public override void ApplyDot(int dotDamage)
    {
        // BossClone2는 무적이 없으므로 DOT 데미지를 바로 적용
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
        // BossClone2는 무적이 없으므로 매 틱마다 DOT 데미지를 바로 적용
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

    public override void AdjustHp(float multiplier)
    {
        base.AdjustHp(multiplier); // 부모 클래스의 AdjustHp 호출   
    }
}