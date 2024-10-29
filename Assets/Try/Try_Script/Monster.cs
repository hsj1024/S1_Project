using System.Collections;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public static Monster Instance { get; private set; }

    private Bal bal;
    public string monsterName;
    public float hp;
    public int speed;
    public float xp;
    public SpriteRenderer spriteRenderer;
    public GameObject hitPrefab;
    public float fadeOutDuration = 0.4f;
    public MonsterSpawnManager spawnManager;
    public static bool disableGameOver = false;
    public bool isSpecialMonster = false;

    public AudioClip hitSound;
    public GameObject hitAnimationPrefab;
    public float animationDuration = 0.3f;
    public AudioManager audioManager;

    public bool invincible = false;
    public float invincibleDuration = 0.3f;
    public float lastHitTime;
    public Rigidbody2D rb;

    public float xpDrop;

    public float knockbackForce = 0.5f;
    public float knockbackDuration = 0.2f;
    public bool isKnockedBack = false;
    public float knockbackTimer = 0f;
    public GameObject currentHitInstance;

    public GameObject fireEffectPrefab; // Fire 이펙트 프리팹 추가
    public GameObject fireEffectInstance; // Fire 이펙트 인스턴스
    public GameObject hitFireEffectInstance; // Hit 프리펩에 붙는 Fire 이펙트 인스턴스
    public bool isOnFire = false; // Fire 이펙트가 활성화되었는지 여부

    public bool isFadingOut = false;
    // 충돌 횟수를 추적하는 변수 추가
    private int collisionCount = 0;
    public bool isBoss = false; // 보스 여부를 확인하는 변수 추가
    public bool isClone = false; // 보스 여부를 확인하는 변수 추가


    public void Start()
    {
        audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            Debug.LogError("AudioManager를 찾을 수 없습니다. AudioManager가 씬 내에 있는지 확인하세요.");
            return;
        }

        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnManager = FindObjectOfType<MonsterSpawnManager>();
    }

    public void Update()
    {
        if (!isFadingOut)
        {
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
                        UpdateSortingOrder();
                        SetFireEffectParent();
                    }
                }
            }

            if (!isKnockedBack)
            {
                MoveIfWithinBounds();
            }
        }

        UpdateSortingOrder();

        if (currentHitInstance != null)
        {
            currentHitInstance.transform.position = transform.position;
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
    }

    public virtual void MoveDown()
    {
        if (isKnockedBack || (currentHitInstance != null && hp <= 0)) return;

        float speedScale = 0.02f;
        transform.position += Vector3.down * speed * speedScale * Time.deltaTime;
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


    public static void ToggleInvincibility()
    {
        disableGameOver = !disableGameOver;
    }

    public void UpdateSortingOrder()
    {
        int sortingOrderBase = 10;
        spriteRenderer.sortingOrder = sortingOrderBase - (int)(transform.position.y * 10);

        if (fireEffectInstance != null)
        {
            var fireSpriteRenderer = fireEffectInstance.GetComponent<SpriteRenderer>();
            if (fireSpriteRenderer != null)
            {
                fireSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            }
        }

        if (spawnManager != null)
        {
            foreach (var otherMonster in spawnManager.activeMonsters)
            {
                if (otherMonster != this && otherMonster.transform.position.y > this.transform.position.y)
                {
                    otherMonster.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;

                    if (otherMonster.fireEffectInstance != null)
                    {
                        var otherFireSpriteRenderer = otherMonster.fireEffectInstance.GetComponent<SpriteRenderer>();
                        if (otherFireSpriteRenderer != null)
                        {
                            otherFireSpriteRenderer.sortingOrder = otherMonster.spriteRenderer.sortingOrder + 1;
                        }
                    }
                }
            }
        }
    }

    public void SetFireEffectParent()
    {
        if (fireEffectInstance != null)
        {
            fireEffectInstance.transform.SetParent(transform);
            fireEffectInstance.transform.localPosition = Vector3.zero;

            // Fire 이펙트의 sortingOrder 업데이트
            var fireSpriteRenderer = fireEffectInstance.GetComponent<SpriteRenderer>();
            if (fireSpriteRenderer != null)
            {
                fireSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            }
            else
            {
                Debug.LogError("Fire Effect does not have a SpriteRenderer component.");
            }
        }
    }

    public void MoveIfWithinBounds()
    {
        if (transform.position.x >= -2 && transform.position.x <= 2.2)
        {
            MoveDown();
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (!invincible)
        {
            hp -= damage;

            if (hp > 0)
            {
                StartCoroutine(ShowHitEffect(false)); // 기본 hit 효과
            }
            else if (hp <= 0 && !isFadingOut)
            {
                if (!isBoss && !isClone)  // 보스나 보스 클론이 아니면 FadeOutAndDestroy 실행
                {
                    StartCoroutine(FadeOutAndDestroy(false, false));
                }
                else  // 보스나 보스 클론일 경우 즉시 사망 처리
                {
                    OnDeath();  // 보스나 보스 클론의 사망 처리
                }
            }
            lastHitTime = Time.time;
            ActivateInvincibility();
        }
    }

    private IEnumerator DisableInvincibility()
    {
        yield return new WaitForSeconds(invincibleDuration);
        invincible = false;
    }

    public virtual void TakeDamageFromArrow(float damage, bool knockbackEnabled, Vector2 knockbackDirection, bool applyDot = false, int dotDamage = 0, bool isAoeHit = false)
    {
        if (hp > 0)
        {
            // 화살에 맞았을 때 무조건 hit 프리팹을 생성하고 정지
            StartCoroutine(ShowHitEffect(true, applyDot));

            if (!invincible)
            {
                hp -= damage;
                //Debug.Log($"Monster hp: {hp}");

                if (hp > 0)
                {
                    if (applyDot && dotDamage > 0)
                    {
                        ApplyDot(dotDamage);
                    }
                }
                else if (hp <= 0)
                {
                    if (knockbackEnabled && !isKnockedBack && rb != null && !isAoeHit)
                    {
                        ApplyKnockback(knockbackDirection, true);
                    }
                    else if (!isKnockedBack)
                    {
                        if (LevelManager.Instance != null)
                        {
                            LevelManager.Instance.IncrementMonsterKillCount();
                        }

                        // 보스나 보스 클론이 아닌 경우에만 FadeOutAndDestroy 호출
                        if (!isBoss && !isClone)
                        {
                            StartCoroutine(FadeOutAndDestroy(true, applyDot)); // 화살로 인한 페이드 아웃에서는 fire 이펙트를 표시
                        }
                        else
                        {
                            // 보스와 보스 클론이 사망할 때 실행할 로직 (Destroy 또는 다른 처리)
                            OnDeath(); // 보스나 클론의 사망 처리 로직
                        }
                    }
                }

                if (knockbackEnabled && !isKnockedBack && rb != null && !isAoeHit)
                {
                    ApplyKnockback(knockbackDirection);
                }

                lastHitTime = Time.time;
                ActivateInvincibility();
            }

            // 히트 사운드 재생
            StartCoroutine(PlayArrowHitAnimation());
        }
    }
    protected virtual void OnDeath()
    {
        // 기본 몬스터에는 FadeOutAndDestroy가 호출되지만, 보스와 보스 클론에서는 개별 처리
        if (isBoss || isClone)
        {
            // 보스 또는 클론의 죽음 처리 (FadeOutAndDestroy를 호출하지 않음)
            Destroy(gameObject); // 보스나 클론은 바로 게임 오브젝트를 제거
        }
        else
        {
            StartCoroutine(FadeOutAndDestroy(true, true)); // 일반 몬스터의 경우만 FadeOutAndDestroy 호출
        }
    }


    public void ActivateInvincibility()
    {
        invincible = true;
        StartCoroutine(DisableInvincibility());
    }

    public virtual void ApplyDot(int dotDamage)
    {
        // 보스는 DOT 데미지 무적을 Monster에서 처리하지 않음
        if (isBoss)
        {
            // 보스일 경우 DOT 데미지 처리는 보스 스크립트에서 처리
            return;
        }

        if (fireEffectPrefab != null && !isOnFire)
        {
            // Fire 이펙트를 몬스터에 추가
            fireEffectInstance = Instantiate(fireEffectPrefab, transform.position, Quaternion.identity);
            fireEffectInstance.transform.SetParent(transform);
            var fireSpriteRenderer = fireEffectInstance.GetComponent<SpriteRenderer>();
            if (fireSpriteRenderer != null)
            {
                fireSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1; // 몬스터 스프라이트보다 높은 값으로 설정
            }
            isOnFire = true;
        }

        StartCoroutine(DotDamage(dotDamage));
    }

    private IEnumerator DotDamage(int dotDamage)
    {
        while (hp > 0)
        {
            // 보스가 아닌 경우에만 DOT 데미지를 적용
            if (!isBoss)
            {
                hp -= dotDamage;
            }

            yield return new WaitForSeconds(1.0f); // DOT 데미지가 1초마다 적용됨
        }

        if (hp <= 0)
        {
            // Fire 이펙트 제거
            if (fireEffectInstance != null)
            {
                Destroy(fireEffectInstance);
                fireEffectInstance = null;
            }

            StartCoroutine(FadeOutAndDestroy(true, true)); // showFireEffect 및 applyDot 매개변수 전달
        }
    }
    public void ApplyKnockback(Vector2 knockbackDirection, bool destroyAfterKnockback = false)
    {
        if (currentHitInstance != null)
        {
            Destroy(currentHitInstance);
        }
        currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);

        spriteRenderer.enabled = false;

        rb.velocity = Vector2.zero;
        float knockbackDistance = 0.5f;
        rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);

        isKnockedBack = true;
        knockbackTimer = knockbackDuration;

        if (currentHitInstance != null)
        {
            StartCoroutine(MoveHitPrefabWithKnockback(destroyAfterKnockback));
        }

        IgnoreCollisionsWithOtherMonsters(true);
        invincible = true;
        StartCoroutine(DisableInvincibility());
    }

    private void IgnoreCollisionsWithOtherMonsters(bool ignore)
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            Collider2D[] allColliders = FindObjectsOfType<Collider2D>();
            foreach (Collider2D otherCollider in allColliders)
            {
                if (otherCollider.CompareTag("Monster") && otherCollider != collider)
                {
                    Physics2D.IgnoreCollision(collider, otherCollider, ignore);
                }
            }
        }
    }

    private IEnumerator MoveHitPrefabWithKnockback(bool destroyAfterKnockback)
    {
        while (isKnockedBack)
        {
            if (currentHitInstance != null)
            {
                currentHitInstance.transform.position = transform.position;
            }
            yield return null;
        }

        if (hp > 0)
        {
            spriteRenderer.enabled = true;
            Destroy(currentHitInstance);
            currentHitInstance = null;
        }
        else if (destroyAfterKnockback) // 넉백 후 파괴 설정이 있을 경우
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.IncrementMonsterKillCount();
            }
            StartCoroutine(FadeOutAndDestroy(true, true)); // showFireEffect 및 applyDot 매개변수 전달
        }

        IgnoreCollisionsWithOtherMonsters(false);
    }

   public IEnumerator PlayArrowHitAnimation()
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

    private IEnumerator ShowHitEffect(bool showFireEffect = true, bool applyDot = false)
    {
        spriteRenderer.enabled = false;

        if (currentHitInstance == null)
        {
            currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);

            if (showFireEffect && fireEffectPrefab != null && applyDot)
            {
                hitFireEffectInstance = Instantiate(fireEffectPrefab, currentHitInstance.transform.position, Quaternion.identity);
                hitFireEffectInstance.transform.SetParent(currentHitInstance.transform);
                var hitFireSpriteRenderer = hitFireEffectInstance.GetComponent<SpriteRenderer>();
                if (hitFireSpriteRenderer != null)
                {
                    hitFireSpriteRenderer.sortingOrder = 11; // 초기 높은 값으로 설정

                    var hitSpriteRenderer = currentHitInstance.GetComponent<SpriteRenderer>();
                    if (hitSpriteRenderer != null)
                    {
                        hitFireSpriteRenderer.sortingOrder = hitSpriteRenderer.sortingOrder + 1;
                    }
                }
                else
                {
                    Debug.LogError("Hit Fire Effect does not have a SpriteRenderer component.");
                }
            }
        }

        yield return new WaitForSeconds(0.3f);

        if (hp > 0)
        {
            spriteRenderer.enabled = true;
            UpdateSortingOrder(); // 몬스터가 활성화될 때 sortingOrder 업데이트
            Destroy(currentHitInstance);
            currentHitInstance = null;

            // Fire 이펙트가 몬스터에 다시 붙도록 위치 조정
            if (showFireEffect && fireEffectInstance != null && applyDot)
            {
                fireEffectInstance.transform.SetParent(transform);
                fireEffectInstance.transform.localPosition = Vector3.zero;

                // Fire 이펙트의 sortingOrder 업데이트
                var fireSpriteRenderer = fireEffectInstance.GetComponent<SpriteRenderer>();
                if (fireSpriteRenderer != null)
                {
                    fireSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
                }
                else
                {
                    Debug.LogError("Fire Effect does not have a SpriteRenderer component.");
                }
            }
        }
        else
        {
            StartCoroutine(FadeOutAndDestroy(showFireEffect, applyDot)); // showFireEffect 및 applyDot 매개변수 전달
        }
    }


    public void FadeOut(bool showFireEffect = true, bool applyDot = false)
    {
        Debug.Log("FadeOut called");
        if (!isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy(showFireEffect, applyDot));
        }
    }

    private IEnumerator FadeOutAndDestroy(bool showFireEffect, bool applyDot)
    {
        if (isFadingOut)
        {
            Debug.Log("FadeOutAndDestroy already in progress");
            yield break; // 이미 페이드 아웃이 진행 중이면 중단
        }

        isFadingOut = true; // 페이드 아웃 시작
        Debug.Log("FadeOutAndDestroy started");

        spriteRenderer.enabled = false;

        if (currentHitInstance == null)
        {
            currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);
        }

        var hitInstanceSpriteRenderer = currentHitInstance?.GetComponent<SpriteRenderer>();

        if (hitInstanceSpriteRenderer == null)
        {
            if (currentHitInstance != null)
            {
                Destroy(currentHitInstance);
            }
            Destroy(gameObject);
            yield break;
        }

        float elapsed = 0f;
        Color originalColor = hitInstanceSpriteRenderer.color;

        while (elapsed < fadeOutDuration)
        {
            if (hitInstanceSpriteRenderer != null)
            {
                float t = elapsed / fadeOutDuration;
                hitInstanceSpriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(originalColor.a, 0, t));
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (hitInstanceSpriteRenderer != null)
        {
            hitInstanceSpriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        }

        if (fireEffectInstance != null)
        {
            Destroy(fireEffectInstance);
            fireEffectInstance = null;
        }
        if (hitFireEffectInstance != null)
        {
            Destroy(hitFireEffectInstance);
            hitFireEffectInstance = null;
        }

        if (currentHitInstance != null)
        {
            Destroy(currentHitInstance);
            currentHitInstance = null;
        }
        else
        {
            Debug.Log("currentHitInstance was already null");
        }

        DropExperience();
        //Debug.Log("Destroying monster");
        Destroy(gameObject);
    }

    public virtual void ApplyFireEffect()
    {
        // 보스나 보스 클론의 경우 Monster의 Fire 이펙트를 사용하지 않음
        if (isBoss || isClone)
        {
            return;
        }

        if (fireEffectPrefab != null && !isOnFire)
        {
            fireEffectInstance = Instantiate(fireEffectPrefab, transform.position, Quaternion.identity);
            fireEffectInstance.transform.SetParent(transform);
            isOnFire = true;
        }
    }


    public void DropExperience()
    {
        if (Bal.Instance == null)
        {
            Debug.LogError("Bal instance is null. Cannot drop experience.");
            return;
        }

        float experienceAmount = xpDrop * Bal.Instance.XPM;
        Bal.Instance.AddExperience(experienceAmount);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
        }
        else if (collision.gameObject.CompareTag("Barricade"))
        {
        }
    }



    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
        }
        else if (collision.gameObject.CompareTag("Barricade"))
        {
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
        }

    }
}