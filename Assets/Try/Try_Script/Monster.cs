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
    public GameObject hitAnimationPrefab;
    public float animationDuration = 0f;
    public AudioManager audioManager;

    public bool invincible = false;
    public float invincibleDuration = 0.3f;
    private float lastHitTime;
    public Rigidbody2D rb;

    public float xpDrop;

    public float knockbackForce = 1f;
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    private GameObject currentHitInstance;

    public GameObject fireEffectPrefab;
    private GameObject currentFireEffect;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            Debug.LogError("AudioManager를 찾을 수 없습니다. AudioManager가 씬 내에 있는지 확인하세요.");
            return;
        }

        rb = GetComponent<Rigidbody2D>();
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
                    RestoreFireEffect();
                }
            }
        }

        if (!isKnockedBack)
        {
            MoveIfWithinBounds();
        }

        UpdateSortingOrder();

        UpdateFireEffectPosition();

        if (currentHitInstance != null)
        {
            currentHitInstance.transform.position = transform.position;
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
                Debug.Log("GameOver");
            }

            Destroy(gameObject);
            Debug.Log("Destroy");
        }
    }

    public static void ToggleInvincibility()
    {
        disableGameOver = !disableGameOver;
    }

    private void UpdateSortingOrder()
    {
        int sortingOrderBase = 3000;
        spriteRenderer.sortingOrder = sortingOrderBase - (int)(transform.position.y * 10);
        if (spawnManager != null)
        {
            foreach (var otherMonster in spawnManager.activeMonsters)
            {
                if (otherMonster != this && otherMonster.transform.position.y > this.transform.position.y)
                {
                    otherMonster.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
                }
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
                StartCoroutine(ShowHitEffect());
            }
            else
            {
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.IncrementMonsterKillCount();
                }
                StartCoroutine(FadeOutAndDestroy());
            }
            lastHitTime = Time.time;
            invincible = true;
            StartCoroutine(DisableInvincibility());
        }
    }

    private IEnumerator DisableInvincibility()
    {
        yield return new WaitForSeconds(invincibleDuration);
        invincible = false;
    }

    public void TakeDamageFromArrow(int damage, bool knockbackEnabled, Vector2 knockbackDirection)
    {
        if (hp > 0)
        {
            if (knockbackEnabled && !isKnockedBack && rb != null)
            {
                ApplyKnockback(knockbackDirection);
            }

            TakeDamage(damage);

            StartCoroutine(PlayArrowHitAnimation());
        }
    }

    public void ApplyDot(int dotDamage)
    {
        if (currentFireEffect == null)
        {
            CreateFireEffect();
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
            StartCoroutine(FadeOutAndDestroy());
        }
    }

    private void CreateFireEffect()
    {
        currentFireEffect = Instantiate(fireEffectPrefab, transform.position, Quaternion.identity);
        currentFireEffect.transform.SetParent(transform);
        currentFireEffect.transform.localPosition = Vector3.zero;
        currentFireEffect.transform.position = new Vector3(currentFireEffect.transform.position.x, currentFireEffect.transform.position.y, -1);

        SpriteRenderer fireSpriteRenderer = currentFireEffect.GetComponent<SpriteRenderer>();
        if (fireSpriteRenderer != null)
        {
            fireSpriteRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
            fireSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
        }
        else
        {
            Debug.LogError("Fire effect prefab does not have a SpriteRenderer component.");
        }
    }

    private void UpdateFireEffectPosition()
    {
        if (currentFireEffect != null)
        {
            if (currentHitInstance != null)
            {
                currentFireEffect.transform.position = currentHitInstance.transform.position;
            }
            else
            {
                currentFireEffect.transform.position = transform.position;
            }
        }
    }

    private void ApplyKnockback(Vector2 knockbackDirection)
    {
        if (currentHitInstance != null)
        {
            Destroy(currentHitInstance);
        }
        currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);

        spriteRenderer.enabled = false;
        HideFireEffect();

        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);

        isKnockedBack = true;
        knockbackTimer = knockbackDuration;

        if (currentHitInstance != null)
        {
            StartCoroutine(MoveHitPrefabWithKnockback());
        }
    }

    private void HideFireEffect()
    {
        if (currentFireEffect != null)
        {
            currentFireEffect.SetActive(false);
        }
    }

    private void RestoreFireEffect()
    {
        if (currentFireEffect != null)
        {
            currentFireEffect.SetActive(true);
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
            RestoreFireEffect();
            Destroy(currentHitInstance);
            currentHitInstance = null;
        }
        else
        {
            StartCoroutine(FadeOutAndDestroy());
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

    private IEnumerator ShowHitEffect()
    {
        spriteRenderer.enabled = false;
        HideFireEffect();

        if (currentHitInstance == null)
        {
            currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.3f);

        if (hp > 0)
        {
            spriteRenderer.enabled = true;
            RestoreFireEffect();
            Destroy(currentHitInstance);
            currentHitInstance = null;
        }
    }

    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        spriteRenderer.enabled = false;

        if (currentHitInstance == null)
        {
            currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);
        }

        SpriteRenderer hitSpriteRenderer = currentHitInstance.GetComponent<SpriteRenderer>();

        if (hitSpriteRenderer == null)
        {
            Debug.LogError("hitPrefab does not have a SpriteRenderer component.");
            Destroy(currentHitInstance);
            Destroy(gameObject);
            yield break;
        }

        float elapsed = 0f;
        Color originalColor = hitSpriteRenderer.color;

        while (elapsed < fadeOutDuration)
        {
            float t = elapsed / fadeOutDuration;
            hitSpriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(originalColor.a, 0, t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        hitSpriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);

        if (currentFireEffect != null)
        {
            Debug.Log("불 이펙트가 파괴됨: " + currentFireEffect);
            Destroy(currentFireEffect);
            currentFireEffect = null;
        }

        Destroy(currentHitInstance);
        currentHitInstance = null;

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
    }
}
