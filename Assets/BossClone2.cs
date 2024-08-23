using System.Collections;
using UnityEngine;

public class BossClone2 : MonoBehaviour
{
    public float hp = 300f; // �н��� ü��
    public float speed = 8f;
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

    private bool hasReachedCenter = false;
    private Animator animator;
    private bool isFadingOut = false;
    private GameObject currentHitInstance;

    public BossMonster bossMonster; // ���� ���� ����

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f); // ��ο� �������� ���� (R, G, B �� ����)
        /* animator = GetComponent<Animator>();
         if (animator == null)
         {
             Debug.LogError("Animator component is missing from the BossClone2.");
         }*/
    }

    public void SetBoss(BossMonster boss)
    {
        bossMonster = boss;
    }

    void Update()
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
                    }
                }
            }

            if (!isKnockedBack && !hasReachedCenter)
            {
                MoveDown();
            }
        }

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

    void MoveDown()
    {
        if (isKnockedBack || (currentHitInstance != null && hp <= 0)) return;

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

                if (animator != null)
                {
                    animator.enabled = false;  // �ִϸ��̼� ���߱�
                }

                return;
            }
        }

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
                OnCloneDeath(); // Ŭ���� �׾��� �� ����
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
            bossMonster.OnBossClone2Death(); // �������� Ŭ���� ������ �˸�
        }

        Destroy(gameObject); // Ŭ�� ����
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
