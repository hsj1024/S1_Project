using System.Collections;
using UnityEngine;

public class BossClone1 : MonoBehaviour
{
    public float hp = 300f; // �н��� ü��
    public float speed = 20f;
    public float xp = 0f; // ����ġ
    public bool invincible = true; // ���� ����
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

    public float zigzagAmplitude = 20f; // ������� �̵��� ����
    public float zigzagFrequency = 0.1f; // ������� �̵��� �ֱ�
    public float verticalSpeed = 0.2f; // ���� �̵� �ӵ�

    private bool moveLeftToRight;
    private BossMonster bossMonster;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ���� ��������Ʈ�� ���� ��Ӱ� ����
        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f); // ��ο� �������� ���� (R, G, B �� ����)

        audioManager = AudioManager.Instance;
        invincible = true;

        moveLeftToRight = transform.position.x < 0; // �ʱ� �̵� ���� ����
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

        // ������ ��ġ�� ���� ������� ���� ����
        float zigzagDirection = moveLeftToRight ? 1 : -1;
        float zigzag = Mathf.PingPong(Time.time * zigzagFrequency, 1) * 2 - 1; // -1���� 1 ���̷� ������� �̵�

        // �밢�� �̵� ���� ���
        Vector3 direction = new Vector3(zigzag * zigzagAmplitude * zigzagDirection, -verticalSpeed, 0);

        transform.position += direction * Time.deltaTime;

        // ���Ͱ� ī�޶� ��踦 ����� �ʵ��� ����
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        viewportPos.x = Mathf.Clamp(viewportPos.x, 0.1f, 0.9f); // ���� ��� ���� ����
        viewportPos.y = Mathf.Clamp(viewportPos.y, 0, 1); // ���� ��� ���� ����
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
                OnCloneDeath(); // �н��� �׾��� �� ����
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
        BossMonster boss = FindObjectOfType<BossMonster>(); // ���� ���͸� ã�Ƽ� ���� ����
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
