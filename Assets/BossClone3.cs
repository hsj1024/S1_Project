using System.Collections;
using UnityEngine;

public class BossClone3 : MonoBehaviour
{
    public float hp = 300f; // 분신의 체력
    public float speed = 8f;
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

    private bool hasReachedCenter = false;
    private Animator animator;
    private bool isFadingOut = false;
    private GameObject currentHitInstance;

    // 추가된 변수들
    public BossMonster bossMonster;
    private bool isHealing = false;
    private Color originalColor;
    private Color healColor = new Color(103f / 255f, 255f / 255f, 192f / 255f);
    public float fireDuration = 5f;
    public float fireDamageInterval = 1f;
    public float fireDamageAmount = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f); // 어두운 색상으로 설정 (R, G, B 값 조정)
        originalColor = spriteRenderer.color;
        animator = GetComponent<Animator>();
        if (audioManager == null)
        {
            audioManager = AudioManager.Instance;
        }
        StartCoroutine(HealOverTime());
    }

    public void SetBoss(BossMonster boss)
    {
        bossMonster = boss;
    }

    void Update()
    {
        if (!hasReachedCenter)
        {
            MoveDown();
        }

        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0)
            {
                isKnockedBack = false;
                rb.velocity = Vector2.zero;

                if (currentHitInstance != null)
                {
                    Destroy(currentHitInstance);
                    spriteRenderer.enabled = true;
                }
            }
        }

        if (fireEffectInstance != null)
        {
            fireEffectInstance.transform.position = transform.position;

            // Fire 이펙트의 sortingOrder 정기적으로 업데이트
            var fireSpriteRenderer = fireEffectInstance.GetComponent<SpriteRenderer>();
            if (fireSpriteRenderer != null)
            {
                fireSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            }
        }

        if (currentHitInstance != null)
        {
            currentHitInstance.transform.position = transform.position;
        }
    }

    void MoveDown()
    {
        if (isKnockedBack) return;

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

                // 보스도 함께 움직이게 설정
                if (bossMonster != null)
                {
                    bossMonster.MoveToCenter();
                }
            }
        }

        if (transform.position.y <= -5.0f)
        {
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

    public void TakeDamage(int damage)
    {
        if (!invincible)
        {
            hp -= damage;

            if (hp <= 0)
            {
                OnCloneDeath();
            }
            else
            {
                StartCoroutine(ShowHitEffect());
            }
            lastHitTime = Time.time;
            ActivateInvincibility();
        }
    }

    private IEnumerator ShowHitEffect()
    {
        spriteRenderer.enabled = false;

        currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.3f);

        if (hp > 0)
        {
            spriteRenderer.enabled = true;
            Destroy(currentHitInstance);
        }
    }

    private void OnCloneDeath()
    {
        if (bossMonster != null)
        {
            bossMonster.OnBossClone3Death();
        }

        Destroy(gameObject);
    }

    public void ApplyKnockback(Vector2 direction)
    {
        if (isKnockedBack) return;

        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
        rb.velocity = direction.normalized * knockbackForce;
        spriteRenderer.enabled = false;

        // Knockback 동안 sprite를 비활성화하고 hitPrefab을 활성화
        if (hitPrefab != null)
        {
            currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);
        }
    }

    public void SetOnFire()
    {
        if (isOnFire) return;

        isOnFire = true;
        fireEffectInstance = Instantiate(fireEffectPrefab, transform.position, Quaternion.identity);

        StartCoroutine(FireDamageOverTime());
    }

    private IEnumerator FireDamageOverTime()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fireDuration)
        {
            hp -= fireDamageAmount;
            yield return new WaitForSeconds(fireDamageInterval);

            elapsedTime += fireDamageInterval;
            if (hp <= 0)
            {
                OnCloneDeath();
                break;
            }
        }

        Destroy(fireEffectInstance);
        isOnFire = false;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Arrow"))
        {
            Arr arrow = collision.gameObject.GetComponent<Arr>();
            if (arrow != null)
            {
                TakeDamage((int)arrow.damage);

                // Knockback 적용
                ApplyKnockback(collision.transform.up);

                Destroy(collision.gameObject);
            }
        }
    }
}
