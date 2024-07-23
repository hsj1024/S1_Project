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

    public AudioClip hitSound;
    public GameObject hitAnimationPrefab;
    public float animationDuration = 0f;
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

    public GameObject fireEffectPrefab; // Fire ����Ʈ ������ �߰�
    public GameObject fireEffectInstance; // Fire ����Ʈ �ν��Ͻ�
    public GameObject hitFireEffectInstance; // Hit �����鿡 �ٴ� Fire ����Ʈ �ν��Ͻ�
    public bool isOnFire = false; // Fire ����Ʈ�� Ȱ��ȭ�Ǿ����� ����

    public bool isFadingOut = false;

    public void Start()
    {
        audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            Debug.LogError("AudioManager�� ã�� �� �����ϴ�. AudioManager�� �� ���� �ִ��� Ȯ���ϼ���.");
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

            // Fire ����Ʈ�� sortingOrder ���������� ������Ʈ
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

            // Fire ����Ʈ�� sortingOrder ������Ʈ
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

    public void TakeDamage(int damage)
    {
        if (!invincible)
        {
            hp -= damage;

            if (hp > 0)
            {
                StartCoroutine(ShowHitEffect(false)); // fire ����Ʈ ���� hit ȿ���� ǥ��
            }
            else if (hp <= 0 && !isFadingOut) // Ensure this condition
            {
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.IncrementMonsterKillCount();
                }
                StartCoroutine(FadeOutAndDestroy(false, false)); // fire ����Ʈ�� ǥ������ ����
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

    public void TakeDamageFromArrow(float damage, bool knockbackEnabled, Vector2 knockbackDirection, bool applyDot = false, int dotDamage = 0, bool isAoeHit = false)
    {
        if (hp > 0)
        {
            // ȭ�쿡 �¾��� �� ������ hit �������� �����ϰ� ����
            StartCoroutine(ShowHitEffect(true, applyDot));

            if (!invincible)
            {
                hp -= damage;
                Debug.Log($"Monster hp: {hp}");
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
                        StartCoroutine(FadeOutAndDestroy(true, applyDot)); // ȭ��� ���� ���̵� �ƿ������� fire ����Ʈ�� ǥ��
                    }
                }

                if (knockbackEnabled && !isKnockedBack && rb != null && !isAoeHit)
                {
                    ApplyKnockback(knockbackDirection);
                }

                lastHitTime = Time.time;
                ActivateInvincibility();
            }

            // ��Ʈ ���� ���
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
            // Fire ����Ʈ�� ���Ϳ� �߰�
            fireEffectInstance = Instantiate(fireEffectPrefab, transform.position, Quaternion.identity);
            fireEffectInstance.transform.SetParent(transform);
            var fireSpriteRenderer = fireEffectInstance.GetComponent<SpriteRenderer>();
            if (fireSpriteRenderer != null)
            {
                fireSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1; // ���� ��������Ʈ���� ���� ������ ����
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
            // Fire ����Ʈ ����
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

            StartCoroutine(FadeOutAndDestroy(true, true)); // showFireEffect �� applyDot �Ű����� ����
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
        else if (destroyAfterKnockback) // �˹� �� �ı� ������ ���� ���
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.IncrementMonsterKillCount();
            }
            StartCoroutine(FadeOutAndDestroy(true, true)); // showFireEffect �� applyDot �Ű����� ����
        }

        IgnoreCollisionsWithOtherMonsters(false);
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
                    hitFireSpriteRenderer.sortingOrder = 11; // �ʱ� ���� ������ ����

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
            UpdateSortingOrder(); // ���Ͱ� Ȱ��ȭ�� �� sortingOrder ������Ʈ
            Destroy(currentHitInstance);
            currentHitInstance = null;

            // Fire ����Ʈ�� ���Ϳ� �ٽ� �ٵ��� ��ġ ����
            if (showFireEffect && fireEffectInstance != null && applyDot)
            {
                fireEffectInstance.transform.SetParent(transform);
                fireEffectInstance.transform.localPosition = Vector3.zero;

                // Fire ����Ʈ�� sortingOrder ������Ʈ
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
            StartCoroutine(FadeOutAndDestroy(showFireEffect, applyDot)); // showFireEffect �� applyDot �Ű����� ����
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
            yield break; // �̹� ���̵� �ƿ��� ���� ���̸� �ߴ�
        }

        isFadingOut = true; // ���̵� �ƿ� ����
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

        }
    }
}