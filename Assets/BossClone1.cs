using System.Collections;
using UnityEngine;

public class BossClone1 : Monster
{
    public float zigzagAmplitude = 20f; // 지그재그 이동의 진폭
    public float zigzagFrequency = 0.1f; // 지그재그 이동의 주기
    public float verticalSpeed = 0.2f; // 수직 이동 속도
    private bool moveLeftToRight;
    public BossMonster BossMonster;

    public GameObject cloneFireEffectPrefab; // BossClone1 전용 Fire 이펙트 프리팹
    public GameObject cloneFireEffectInstance; // BossClone1 전용 Fire 이펙트 인스턴스
    public bool cloneIsOnFire = false; // BossClone1의 Fire 이펙트 활성화 여부


    void Start()
    {
        base.Start();
        originalHp = hp; // 초기 체력을 originalHp로 저장
 
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f); // 어두운 색상으로 설정
        moveLeftToRight = transform.position.x < 0; // 초기 이동 방향 설정

        if (BossMonster == null)
        {
            Debug.LogError("BossMonster가 할당되지 않았습니다! SetBoss를 통해 할당하세요.");
        }
    }

    public void SetBoss(BossMonster boss)
    {
        BossMonster = boss;
    }

    void Update()
    {
        base.Update(); // Monster 클래스의 Update() 호출
        MoveDown(); // 상속받은 MoveDown() 대신 지그재그 이동을 구현

        if (cloneFireEffectInstance != null)
        {
            cloneFireEffectInstance.transform.position = transform.position;
        }

        UpdateSortingOrder();
    }


    public override void MoveDown()
    {
        if (isKnockedBack || (hp <= 0)) return;

        // 지그재그 이동 처리
        float zigzagDirection = moveLeftToRight ? 1 : -1;
        float zigzag = Mathf.PingPong(Time.time * zigzagFrequency, 1) * 2 - 1;
        Vector3 direction = new Vector3(zigzag * zigzagAmplitude * zigzagDirection, -verticalSpeed, 0);
        transform.position += direction * Time.deltaTime;

        // 화면 경계 체크
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        viewportPos.x = Mathf.Clamp(viewportPos.x, 0.1f, 0.9f);
        viewportPos.y = Mathf.Clamp(viewportPos.y, 0, 1);
        transform.position = Camera.main.ViewportToWorldPoint(viewportPos);

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
            OnCloneDeath(); // 보스 클론 1 사망 처리
        }
    }

    public override void TakeDamageFromArrow(float damage, bool knockbackEnabled, Vector2 knockbackDirection, bool applyDot = false, int dotDamage = 0, bool isAoeHit = false)
    {
        if (hp > 0)
        {
            hp -= damage;

            if (hp <= 0)
            {
                // 넉백 상태 초기화
                isKnockedBack = false;
                rb.velocity = Vector2.zero;

                OnCloneDeath(); // 보스 클론 1 사망 처리
                return;
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
        //Debug.Log("보스 클론 1이 죽음");

        // 넉백 상태 초기화
        isKnockedBack = false;
        rb.velocity = Vector2.zero; // 넉백으로 인한 이동 중지

        if (BossMonster != null)
        {
            BossMonster.OnBossClone1Death();
        }

        // Fire 이펙트 제거
        if (fireEffectInstance != null)
        {
            Destroy(fireEffectInstance);
            fireEffectInstance = null;
        }

        // Clone Fire 이펙트 제거
        if (cloneFireEffectInstance != null)
        {
            Destroy(cloneFireEffectInstance);
            cloneFireEffectInstance = null;
        }

        Destroy(gameObject); 
    }

    // DOT 데미지를 보스 클론에 적용하는 메서드
    public override void ApplyDot(int dotDamage)
    {
        // BossClone1은 무적이 없으므로 DOT 데미지를 바로 적용
        if (cloneFireEffectPrefab != null && !cloneIsOnFire)
        {
            cloneFireEffectInstance = Instantiate(cloneFireEffectPrefab, transform.position, Quaternion.identity);
            cloneFireEffectInstance.transform.SetParent(transform);
            var fireSpriteRenderer = cloneFireEffectInstance.GetComponent<SpriteRenderer>();
            if (fireSpriteRenderer != null)
            {
                fireSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            }
            cloneIsOnFire = true;
        }

        StartCoroutine(ApplyCloneDotDamage(dotDamage));
    }

    private IEnumerator ApplyCloneDotDamage(int dotDamage)
    {
        // 무적 상태와 상관없이 DOT 데미지 지속적으로 적용
        while (hp > 0)
        {
            hp -= dotDamage;

            yield return new WaitForSeconds(1.0f); // DOT 데미지가 1초마다 적용됨
        }

        if (hp <= 0)
        {
            if (cloneFireEffectInstance != null)
            {
                Destroy(cloneFireEffectInstance);
            }

            OnCloneDeath(); // DOT 데미지로 사망 시 호출
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
