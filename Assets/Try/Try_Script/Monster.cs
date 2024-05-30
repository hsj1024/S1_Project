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
    public float knockbackForce = 1f; // �˹� ��
    public float knockbackDuration = 0.2f; // �˹� ���� �ð�
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    private GameObject currentHitInstance;

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
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0)
            {
                isKnockedBack = false;
                rb.velocity = Vector2.zero; // �ӵ��� �ʱ�ȭ�Ͽ� �˹� ����

                // hit ������ ���� �� ���͸� �ٽ� ���̰� ��
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
            // �˹��� �����մϴ�.
            if (knockbackEnabled && !isKnockedBack && rb != null)
            {
                ApplyKnockback(knockbackDirection);
            }

            // �������� �����մϴ�.
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
        // hit �������� �����ͼ� ���
        currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);

        // ���͸� �� ���̰� ��
        spriteRenderer.enabled = false;

        rb.velocity = Vector2.zero; // ���� �ӵ��� �ʱ�ȭ
        rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse); // �˹� �������� ���� ����
        Debug.Log($"Knockback applied with direction: {knockbackDirection} and force: {knockbackForce}");

        // �˹� ���� ���� �� Ÿ�̸� �ʱ�ȭ
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;

        // hitPrefab�� �˹� �߿��� �Բ� �̵��ϵ��� �մϴ�.
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
                currentHitInstance.transform.position = transform.position; // ���Ϳ� �Բ� �̵�
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
            audioManager.PlayMonsterHitSound(); // AudioManager���� ȭ�� �߻� �Ҹ��� ����ϴ� �޼��� ȣ��
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

        yield return new WaitForSeconds(0.3f); // hit ������ ���� �ð� 0.3��

        if (hp > 0)
        {
            spriteRenderer.enabled = true;
            Destroy(currentHitInstance);
            currentHitInstance = null;
        }
    }

    public void FadeOut()
    {
        StopAllCoroutines(); // ���� ���� ���� ��� �ڷ�ƾ�� ����
        StartCoroutine(FadeOutAndDestroy()); // ���̵� �ƿ� �ڷ�ƾ ���� ȣ��
    }

    private IEnumerator FadeOutAndDestroy()
    {
        // ���͸� ��Ȱ��ȭ�մϴ�.
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

    // �浹 ���� ����
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
        }
    }
}
