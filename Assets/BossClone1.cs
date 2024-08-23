using System.Collections;
using UnityEngine;

public class BossClone1 : MonoBehaviour
{
    public float hp = 300f; // 분신의 체력
    public float speed = 20f;
    public float xp = 0f; // 경험치
    public bool invincible = true; // 무적 상태
    public float invincibleDuration = 0.3f;
    public float lastHitTime;
    public Rigidbody2D rb;
    public float knockbackForce = 0.5f;
    public float knockbackDuration = 0.2f;
    public bool isKnockedBack = false;
    public float knockbackTimer = 0f;
    public GameObject fireEffectPrefab;
    public GameObject fireEffectInstance;
    public bool isOnFire = false;
    public SpriteRenderer spriteRenderer;
    public GameObject hitPrefab;
    public AudioClip hitSound;
    public AudioManager audioManager;
    public GameObject hitAnimationPrefab;
    public float animationDuration = 0f;

    public float zigzagAmplitude = 20f; // 지그재그 이동의 진폭
    public float zigzagFrequency = 0.1f; // 지그재그 이동의 주기
    public float verticalSpeed = 0.2f; // 수직 이동 속도

    private bool moveLeftToRight;
    private BossMonster bossMonster;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 원래 스프라이트의 색을 어둡게 조정
        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f); // 어두운 색상으로 설정 (R, G, B 값 조정)

        audioManager = AudioManager.Instance;
        invincible = true;

        moveLeftToRight = transform.position.x < 0; // 초기 이동 방향 설정
    }

    public void SetBoss(BossMonster boss)
    {
        bossMonster = boss;
    }

    void Update()
    {
        MoveDown();
    }

    public void MoveDown()
    {
        if (isKnockedBack || (hp <= 0)) return;

        // 스폰된 위치에 따라 지그재그 방향 설정
        float zigzagDirection = moveLeftToRight ? 1 : -1;
        float zigzag = Mathf.PingPong(Time.time * zigzagFrequency, 1) * 2 - 1; // -1에서 1 사이로 지그재그 이동

        // 대각선 이동 벡터 계산
        Vector3 direction = new Vector3(zigzag * zigzagAmplitude * zigzagDirection, -verticalSpeed, 0);

        transform.position += direction * Time.deltaTime;

        // 몬스터가 카메라 경계를 벗어나지 않도록 설정
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        viewportPos.x = Mathf.Clamp(viewportPos.x, 0.1f, 0.9f); // 수평 경계 내에 유지
        viewportPos.y = Mathf.Clamp(viewportPos.y, 0, 1); // 수직 경계 내에 유지
        transform.position = Camera.main.ViewportToWorldPoint(viewportPos);

        if (transform.position.y <= -5.0f)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        if (!invincible)
        {
            hp -= damage;

            if (hp > 0)
            {
                StartCoroutine(ShowHitEffect());
            }
            else if (hp <= 0)
            {
                OnCloneDeath(); // 분신이 죽었을 때 실행
            }
            lastHitTime = Time.time;
            ActivateInvincibility();
        }
    }

    private void ActivateInvincibility()
    {
        invincible = true;
        StartCoroutine(DisableInvincibility());
    }

    private IEnumerator DisableInvincibility()
    {
        yield return new WaitForSeconds(invincibleDuration);
        invincible = false;
    }

    private IEnumerator ShowHitEffect()
    {
        spriteRenderer.enabled = false;

        GameObject currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.3f);

        if (hp > 0)
        {
            spriteRenderer.enabled = true;
            Destroy(currentHitInstance);
        }
    }

    private void OnCloneDeath()
    {
        BossMonster boss = FindObjectOfType<BossMonster>(); // 보스 몬스터를 찾아서 무적 해제
        if (boss != null)
        {
            StartCoroutine(TemporarilyDisableBossInvincibility(boss));
        }

        Destroy(gameObject);
    }

    private IEnumerator TemporarilyDisableBossInvincibility(BossMonster boss)
    {
        boss.invincible = false;
        yield return new WaitForSeconds(10f);
        boss.invincible = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Arrow"))
        {
            Arr arrow = collision.gameObject.GetComponent<Arr>();
            if (arrow != null)
            {
                TakeDamage((int)arrow.damage);
                Destroy(collision.gameObject);
            }
        }
    }
}
