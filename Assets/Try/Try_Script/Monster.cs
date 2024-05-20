using System.Collections;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public static Monster Instance { get; private set; }

    private Bal bal; // �߸���Ÿ�� �ν��Ͻ��� ������ ����
    public string monsterName;
    public int hp;
    public int speed;
    public float xp;
    private SpriteRenderer spriteRenderer;
    public GameObject hitPrefab;
    public float fadeOutDuration = 0.4f; // ���̵� �ƿ� �ð�
    private MonsterSpawnManager spawnManager;
    public static bool disableGameOver = false;

    public AudioClip hitSound;
    public GameObject hitAnimationPrefab;
    public float animationDuration = 0f;
    public AudioManager audioManager; // AudioManager ����

    public bool invincible = false; // ���� ���� ����
    public float invincibleDuration = 0.3f; // ���� ���� �ð�
    private float lastHitTime; // ������ �ǰ� �ð� ���
    public Rigidbody2D rb; // Rigidbody2D ������Ʈ

    public float xpDrop; // ���Ͱ� ����ϴ� ����ġ

    // �˹� ���� ���� �߰�
    public float knockbackForce = 100f; // �˹� ��
    public float knockbackDuration = 0.5f; // �˹� ���� �ð�
    private bool isKnockedBack = false; // �˹� ���� ���θ� ��Ÿ���� ����
    private float knockbackTimer = 0f; // �˹� ���� �ð��� ����ϴ� Ÿ�̸�

    private void Start()
    {
        // AudioManager�� ã�Ƽ� �Ҵ��մϴ�.
        audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            Debug.LogError("AudioManager�� ã�� �� �����ϴ�. AudioManager�� �� ���� �ִ��� Ȯ���ϼ���.");
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
        // �˹� ������ ��� Ÿ�̸Ӹ� ������Ʈ�ϰ� ���� �ð��� ������ �˹� ���� ����
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
        if (isKnockedBack) return; // �˹� ������ ���� �̵����� ����

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
            // �ǰ� �ð� ���
            lastHitTime = Time.time;
            // ���� ���·� ����
            invincible = true;
            // ���� �ð� �Ŀ� ���� ���� ����
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
            // �������� �����մϴ�.
            TakeDamage(damage);

            // �˹��� �����մϴ�.
            if (knockbackEnabled && !isKnockedBack && rb != null)
            {
                ApplyKnockback(knockbackDirection);
            }

            StartCoroutine(PlayArrowHitAnimation());
        }
    }

    private void ApplyKnockback(Vector2 knockbackDirection)
    {
        rb.velocity = Vector2.zero; // ���� �ӵ��� �ʱ�ȭ
        rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse); // �˹� �������� ���� ����
        Debug.Log($"Knockback applied with direction: {knockbackDirection} and force: {knockbackForce}");

        // �˹� ���� ���� �� Ÿ�̸� �ʱ�ȭ
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
            audioManager.PlayMonsterHitSound(); // AudioManager���� ȭ�� �߻� �Ҹ��� ����ϴ� �޼��� ȣ��
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
        StopAllCoroutines(); // ���� ���� ���� ��� �ڷ�ƾ�� ����
        StartCoroutine(FadeOutAndDestroy()); // ���̵� �ƿ� �ڷ�ƾ ���� ȣ��
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
        DropExperience(); // ���Ͱ� �׾��� �� ����ġ�� ����մϴ�.

        Destroy(gameObject);
    }

    public void DropExperience()
    {
        if (Bal.Instance == null)
        {
            Debug.LogError("Bal instance is null. Cannot drop experience.");
            return;
        }

        float experienceAmount = xpDrop * Bal.Instance.XPM; // �߸���Ÿ�� ����ġ ����� ���մϴ�.
        Bal.Instance.AddExperience(experienceAmount); // ����ġ ����
    }
}
