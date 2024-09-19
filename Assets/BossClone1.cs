using System.Collections;
using UnityEngine;

public class BossClone1 : Monster
{
    public float zigzagAmplitude = 20f; // 지그재그 이동의 진폭
    public float zigzagFrequency = 0.1f; // 지그재그 이동의 주기
    public float verticalSpeed = 0.2f; // 수직 이동 속도
    private bool moveLeftToRight;
    public BossMonster BossMonster;

    void Start()
    {
        base.Start(); // Monster 클래스의 Start() 호출
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

        // 화면 밖으로 나가면 제거
        if (transform.position.y <= -5.0f)
        {
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
                // 아직 살아있음
            }
            else if (hp <= 0)
            {
                OnCloneDeath(); // 보스 클론 1이 죽음
            }
        }
    }

    private void OnCloneDeath()
    {
        Debug.Log("보스 클론 1이 죽음");

        if (BossMonster != null)
        {

            BossMonster.OnBossClone1Death(); // 보스에게 다음 진행 요청
        }
        else
        {
            Debug.LogError("BossMonster가 할당되지 않았습니다.");
        }

        Destroy(gameObject); // 클론 제거
    }

    public override void TakeDamageFromArrow(float damage, bool knockbackEnabled, Vector2 knockbackDirection, bool applyDot = false, int dotDamage = 0, bool isAoeHit = false)
    {
        if (hp > 0)
        {
            // 데미지 적용
            hp -= damage;

            if (hp <= 0)
            {
                OnCloneDeath(); // 보스 클론 1이 죽음
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
