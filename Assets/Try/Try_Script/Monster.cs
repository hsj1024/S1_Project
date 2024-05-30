using System.Collections;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public static Monster Instance { get; private set; }

    private Bal bal; // 발리스타의 인스턴스를 저장할 변수
    public string monsterName;
    public int hp;
    public int speed;
    public float xp;
    private SpriteRenderer spriteRenderer;
    public GameObject hitPrefab;
    public float fadeOutDuration = 0.4f; // 페이드 아웃 시간
    private MonsterSpawnManager spawnManager;
    public static bool disableGameOver = false;

    public AudioClip hitSound;
    public GameObject hitAnimationPrefab;
    public float animationDuration = 0f;
    public AudioManager audioManager; // AudioManager 참조

    public bool invincible = false; // 무적 상태 여부
    public float invincibleDuration = 0.3f; // 무적 지속 시간
    private float lastHitTime; // 마지막 피격 시간 기록
    public Rigidbody2D rb; // Rigidbody2D 컴포넌트

    public float xpDrop; // 몬스터가 드랍하는 경험치

    // 넉백 관련 변수 추가
    public float knockbackForce = 1f; // 넉백 힘
    public float knockbackDuration = 0.2f; // 넉백 지속 시간
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    private GameObject currentHitInstance;

    private void Start()
    {
        // AudioManager를 찾아서 할당합니다.
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
                rb.velocity = Vector2.zero; // 속도를 초기화하여 넉백 종료

                // hit 프리펩 제거 후 몬스터를 다시 보이게 함
                if (currentHitInstance != null)
                {
                    Destroy(currentHitInstance);
                    spriteRenderer.enabled = true;
                }
            }
        }

        if (!isKnockedBack)
        {
            MoveIfWithinBounds();
        }

        UpdateSortingOrder();
    }

    private void MoveDown()
    {
        if (isKnockedBack) return; // 넉백 상태일 때는 이동하지 않음

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
            Debug.Log($"Monster {monsterName} took {damage} damage. HP before: {hp}");
            hp -= damage;
            Debug.Log($"Monster {monsterName} HP after: {hp}");

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
            // 피격 시간 기록
            lastHitTime = Time.time;
            // 무적 상태로 설정
            invincible = true;
            // 일정 시간 후에 무적 상태 해제
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
            // 넉백을 적용합니다.
            if (knockbackEnabled && !isKnockedBack && rb != null)
            {
                ApplyKnockback(knockbackDirection);
            }

            // 데미지를 적용합니다.
            TakeDamage(damage);

            StartCoroutine(PlayArrowHitAnimation());
        }
    }


    private void ApplyKnockback(Vector2 knockbackDirection)
    {
        if (currentHitInstance != null)
        {
            Destroy(currentHitInstance);
        }
        // hit 프리펩을 가져와서 사용
        currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);

        // 몬스터를 안 보이게 함
        spriteRenderer.enabled = false;

        rb.velocity = Vector2.zero; // 현재 속도를 초기화
        rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse); // 넉백 방향으로 힘을 가함
        Debug.Log($"Knockback applied with direction: {knockbackDirection} and force: {knockbackForce}");

        // 넉백 상태 설정 및 타이머 초기화
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;

        // hitPrefab이 넉백 중에도 함께 이동하도록 합니다.
        if (currentHitInstance != null)
        {
            StartCoroutine(MoveHitPrefabWithKnockback());
        }
    }


    private IEnumerator MoveHitPrefabWithKnockback()
    {
        while (isKnockedBack)
        {
            if (currentHitInstance != null)
            {
                currentHitInstance.transform.position = transform.position; // 몬스터와 함께 이동
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
            audioManager.PlayMonsterHitSound(); // AudioManager에서 화살 발사 소리를 재생하는 메서드 호출
        }

        yield return new WaitForSeconds(animationDuration);
    }

    private IEnumerator ShowHitEffect()
    {
        spriteRenderer.enabled = false;

        if (currentHitInstance == null)
        {
            currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.3f); // hit 프리펩 지속 시간 0.3초

        if (hp > 0)
        {
            spriteRenderer.enabled = true;
            Destroy(currentHitInstance);
            currentHitInstance = null;
        }
    }

    public void FadeOut()
    {
        StopAllCoroutines(); // 현재 진행 중인 모든 코루틴을 중지
        StartCoroutine(FadeOutAndDestroy()); // 페이드 아웃 코루틴 직접 호출
    }

    private IEnumerator FadeOutAndDestroy()
    {
        // 몬스터를 비활성화합니다.
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

        hitSpriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0); // Ensure it's fully transparent

        Destroy(currentHitInstance);
        currentHitInstance = null;

        DropExperience(); // 몬스터가 죽었을 때 경험치를 드랍합니다.
        Destroy(gameObject);
    }



    public void DropExperience()
    {
        if (Bal.Instance == null)
        {
            Debug.LogError("Bal instance is null. Cannot drop experience.");
            return;
        }

        float experienceAmount = xpDrop * Bal.Instance.XPM; // 발리스타의 경험치 배수를 곱합니다.
        Bal.Instance.AddExperience(experienceAmount); // 경험치 누적
    }

    // 충돌 무시 설정
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
        }
    }
}
