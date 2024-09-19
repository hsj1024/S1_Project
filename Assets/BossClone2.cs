using System.Collections;
using UnityEngine;

public class BossClone2 : Monster
{
    private bool hasReachedCenter = false;
    public BossMonster BossMonster;
    private bool isDead = false; // Ŭ���� �̹� �׾����� ���θ� ����

    void Start()
    {
        base.Start(); // Monster Ŭ������ Start() ȣ��
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f); // ��ο� �������� ����
        audioManager = AudioManager.Instance;
    }

    public void SetBoss(BossMonster boss)
    {
        BossMonster = boss;
    }

    void Update()
    {
        if (!isKnockedBack && !hasReachedCenter)
        {
            MoveDown(); // �߾ӿ� �������� �ʾ��� ���� �̵�
        }

        if (hasReachedCenter)
        {
            rb.velocity = Vector2.zero; // �߾ӿ� �����ϸ� ����
        }
    }

    public override void MoveDown()
    {
        if (isKnockedBack || (hp <= 0)) return;

        float speedScale = 0.02f;
        transform.position += Vector3.down * speed * speedScale * Time.deltaTime;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            float cameraCenterY = mainCamera.transform.position.y;

            if (transform.position.y <= cameraCenterY && !hasReachedCenter)
            {
                hasReachedCenter = true;
                rb.velocity = Vector2.zero; // �߾ӿ� �������� �� ����
            }
        }

        if (transform.position.y <= -5.0f)
        {
            if (!disableGameOver)
            {
                LevelManager.Instance?.GameOver();
            }

            Destroy(gameObject);
        }
    }

    public override void TakeDamage(int damage)
    {
        if (!invincible)
        {
            hp -= damage;
            if (hp > 0)
            {

            }
            else if (hp <= 0)
            {
                OnCloneDeath(); // ������ �׾��� �� ó��
            }
        }
    }

    private void OnCloneDeath()
    {
        if (!isDead) // �̹� ���� �ʾҴٸ�
        {
            isDead = true; // ���� �÷��� ����
            Debug.Log("���� Ŭ�� 2 ���");

            if (BossMonster != null)
            {
                BossMonster.OnBossClone2Death();
            }

            Destroy(gameObject); // Ŭ�� �ı�
        }
    }

    public override void TakeDamageFromArrow(float damage, bool knockbackEnabled, Vector2 knockbackDirection, bool applyDot = false, int dotDamage = 0, bool isAoeHit = false)
    {
        if (!isDead) // �̹� ���� Ŭ�п� ���ؼ��� ������ ó�� ����
        {
            // ������ ����
            hp -= damage;

            if (hp <= 0)
            {
                OnCloneDeath(); // ü���� 0 �����̸� ���� ó��
            }
            else
            {
                // ������ hitPrefab�̳� ������ ȿ���� �߻���Ű�� ����
                if (knockbackEnabled && !isKnockedBack && rb != null && !isAoeHit)
                {
                    ApplyKnockback(knockbackDirection); // �˹� ó��
                }

                if (applyDot && dotDamage > 0)
                {
                    ApplyDot(dotDamage); // ���� ������(DOT) ����
                }
            }

            StartCoroutine(PlayArrowHitAnimation()); // ȭ�� �¾��� �� �ִϸ��̼� ����
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

}
