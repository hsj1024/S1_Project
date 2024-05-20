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
    public float knockbackForce = 100f; // 넉백 힘
    public float knockbackDuration = 0.5f; // 넉백 지속 시간
    private bool isKnockedBack = false; // 넉백 상태 여부를 나타내는 변수
    private float knockbackTimer = 0f; // 넉백 지속 시간을 계산하는 타이머

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
        // 넉백 상태인 경우 타이머를 업데이트하고 지속 시간이 끝나면 넉백 상태 해제
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0)
            {
                isKnockedBack = false;
            }
        }

        MoveIfWithinBounds();
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
                spriteRenderer.enabled = false;
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
            // 데미지를 적용합니다.
            TakeDamage(damage);

            // 넉백을 적용합니다.
            if (knockbackEnabled && !isKnockedBack && rb != null)
            {
                ApplyKnockback(knockbackDirection);
            }

            StartCoroutine(PlayArrowHitAnimation());
        }
    }

    private void ApplyKnockback(Vector2 knockbackDirection)
    {
        rb.velocity = Vector2.zero; // 현재 속도를 초기화
        rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse); // 넉백 방향으로 힘을 가함
        Debug.Log($"Knockback applied with direction: {knockbackDirection} and force: {knockbackForce}");

        // 넉백 상태 설정 및 타이머 초기화
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
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
        GameObject hitInstance = null;
        if (hitPrefab != null)
        {
            hitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);
        }
        yield return new WaitForSeconds(0.5f);

        if (hitInstance != null)
        {
            Destroy(hitInstance);
        }

        if (hp > 0)
        {
            spriteRenderer.enabled = true;
        }
    }

    public void FadeOut()
    {
        StopAllCoroutines(); // 현재 진행 중인 모든 코루틴을 중지
        StartCoroutine(FadeOutAndDestroy()); // 페이드 아웃 코루틴 직접 호출
    }

    private IEnumerator FadeOutAndDestroy()
    {
        GameObject hitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);
        SpriteRenderer hitSpriteRenderer = hitInstance.GetComponent<SpriteRenderer>();
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            float t = elapsed / fadeOutDuration;
            hitSpriteRenderer.color = Color.Lerp(hitSpriteRenderer.color, new Color(hitSpriteRenderer.color.r, hitSpriteRenderer.color.g, hitSpriteRenderer.color.b, 0), t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(hitInstance);
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
}
