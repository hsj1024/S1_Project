using System.Collections;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public static Monster Instance { get; private set; }

    private Bal bal;
    public string monsterName;
    public int hp;
    public int speed;
    public float xp;
    private SpriteRenderer spriteRenderer;
    public GameObject hitPrefab;
    public float fadeOutDuration = 0.4f;
    private MonsterSpawnManager spawnManager;
    public static bool disableGameOver = false;

    public AudioClip hitSound;
    public AudioManager audioManager;

    public bool invincible = false;
    public float invincibleDuration = 0.3f;
    private float lastHitTime;
    public Rigidbody2D rb;

    public float xpDrop;

    public float knockbackForce = 0.5f;
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    private GameObject currentHitInstance;

    public GameObject fireEffectPrefab; // Fire 이펙트 프리팹 추가
    private GameObject fireEffectInstance; // Fire 이펙트 인스턴스
    private GameObject hitFireEffectInstance; // Hit 프리펩에 붙는 Fire 이펙트 인스턴스
    private bool isOnFire = false; // Fire 이펙트가 활성화되었는지 여부

    private bool isFadingOut = false;

    private void Start()
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

    private void Update()
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

    private void MoveDown()
    {
        if (isKnockedBack || (currentHitInstance != null && hp <= 0)) return;

        float speedScale = 0.04f;
        transform.position += Vector3.down * speed * speedScale * Time.deltaTime;
        if (transform.position.y <= -5.0f)
        {
            if (!disableGameOver)
            {
                LevelManager.Instance.GameOver();
            }

            Destroy(gameObject);
        }
    }

    public static void ToggleInvincibility()
    {
        disableGameOver = !disableGameOver;
    }

    private void UpdateSortingOrder()
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

    private void SetFireEffectParent()
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

    private void MoveIfWithinBounds()
    {
        if (transform.position.x >= -2 && transform.position.x <= 2.2)
        {
            MoveDown();
        }
    }

    public void TakeDamage(int damage)
    {
        if (!invincible)
        {
            hp -= damage;
          
            if (hp > 0)
            {
                StartCoroutine(ShowHitEffect(false)); // fire 이펙트 없이 hit 효과만 표시
            }
            else if (hp <= 0 && !isFadingOut) // Ensure this condition
            {
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.IncrementMonsterKillCount();
                }               
                StartCoroutine(FadeOutAndDestroy(false, false)); // fire 이펙트를 표시하지 않음
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

    public void TakeDamageFromArrow(int damage, bool knockbackEnabled, Vector2 knockbackDirection, bool applyDot = false, int dotDamage = 0, bool isAoeHit = false)
    {
        if (hp > 0)
        {
            // 화살에 맞았을 때 무조건 hit 프리팹을 생성하고 정지
            StartCoroutine(ShowHitEffect(true, applyDot));

            if (!invincible)
            {
                hp -= damage;
               
                if (hp > 0)
                {
                    if (applyDot && dotDamage > 0)
                    {
                        ApplyDot(dotDamage);
                    }
                }
                else if (hp <= 0 && !isFadingOut) // Ensure this condition
                {
                    if (LevelManager.Instance != null)
                    {
                        LevelManager.Instance.IncrementMonsterKillCount();
                    }
                 
                    StartCoroutine(FadeOutAndDestroy(true, applyDot)); // 화살로 인한 페이드 아웃에서는 fire 이펙트를 표시
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

    private void ActivateInvincibility()
    {
        invincible = true;
        StartCoroutine(DisableInvincibility());
    }


    public void ApplyDot(int dotDamage)
    {
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
            hp -= dotDamage;
            yield return new WaitForSeconds(1.0f);
        }

        if (hp <= 0)
        {
            // Fire 이펙트 제거
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

            StartCoroutine(FadeOutAndDestroy(true, true)); // showFireEffect 및 applyDot 매개변수 전달
        }
    }

    public void ApplyKnockback(Vector2 knockbackDirection)
    {
        if (currentHitInstance != null)
        {
            Destroy(currentHitInstance);
        }
        currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);

        spriteRenderer.enabled = false;

        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);

        isKnockedBack = true;
        knockbackTimer = knockbackDuration;

        if (currentHitInstance != null)
        {
            StartCoroutine(MoveHitPrefabWithKnockback());
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

    private IEnumerator MoveHitPrefabWithKnockback()
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
        else
        {
            StartCoroutine(FadeOutAndDestroy(true, true)); // showFireEffect 및 applyDot 매개변수 전달
        }

        IgnoreCollisionsWithOtherMonsters(false);
    }

    private IEnumerator PlayArrowHitAnimation()
    {
        if (audioManager != null)
        {
            audioManager.PlayMonsterHitSound();
        }

        // 사운드 재생만 하므로 대기 시간 없이 즉시 종료
        yield return null;
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
        StopAllCoroutines();
        StartCoroutine(FadeOutAndDestroy(showFireEffect, applyDot));
    }

    private IEnumerator FadeOutAndDestroy(bool showFireEffect, bool applyDot)
    {
        if (isFadingOut)
        {
            yield break; // 이미 페이드 아웃이 진행 중이면 중단
        }
        isFadingOut = true; // 페이드 아웃 시작

 

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
        Destroy(gameObject);
     
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
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMonsterHitSound();
            }
            else
            {
                Debug.LogError("AudioManager Instance is null.");
            }
            // 필요 시 바리케이드와의 충돌 처리 추가
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
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMonsterHitSound();
            }
            else
            {
                Debug.LogError("AudioManager Instance is null.");
            }
            // 필요 시 바리케이드와의 충돌 처리 추가
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
        }
        else if (collision.gameObject.CompareTag("Barricade"))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMonsterHitSound();
            }
            else
            {
                Debug.LogError("AudioManager Instance is null.");
            }
            // 필요 시 바리케이드와의 충돌 처리 추가
        }
    }
}
