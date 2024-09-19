using System.Collections;
using UnityEngine;

public class BossClone1 : Monster
{
    public float zigzagAmplitude = 20f; // ������� �̵��� ����
    public float zigzagFrequency = 0.1f; // ������� �̵��� �ֱ�
    public float verticalSpeed = 0.2f; // ���� �̵� �ӵ�
    private bool moveLeftToRight;
    public BossMonster BossMonster;

    void Start()
    {
        base.Start(); // Monster Ŭ������ Start() ȣ��
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f); // ��ο� �������� ����
        moveLeftToRight = transform.position.x < 0; // �ʱ� �̵� ���� ����

        if (BossMonster == null)
        {
            Debug.LogError("BossMonster�� �Ҵ���� �ʾҽ��ϴ�! SetBoss�� ���� �Ҵ��ϼ���.");
        }
    }

    public void SetBoss(BossMonster boss)
    {
        BossMonster = boss;
    }

    void Update()
    {
        base.Update(); // Monster Ŭ������ Update() ȣ��
        MoveDown(); // ��ӹ��� MoveDown() ��� ������� �̵��� ����
    }

    public override void MoveDown()
    {
        if (isKnockedBack || (hp <= 0)) return;

        // ������� �̵� ó��
        float zigzagDirection = moveLeftToRight ? 1 : -1;
        float zigzag = Mathf.PingPong(Time.time * zigzagFrequency, 1) * 2 - 1;
        Vector3 direction = new Vector3(zigzag * zigzagAmplitude * zigzagDirection, -verticalSpeed, 0);
        transform.position += direction * Time.deltaTime;

        // ȭ�� ��� üũ
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        viewportPos.x = Mathf.Clamp(viewportPos.x, 0.1f, 0.9f);
        viewportPos.y = Mathf.Clamp(viewportPos.y, 0, 1);
        transform.position = Camera.main.ViewportToWorldPoint(viewportPos);

        // ȭ�� ������ ������ ����
        if (transform.position.y <= -5.0f)
        {
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
                // ���� �������
            }
            else if (hp <= 0)
            {
                OnCloneDeath(); // ���� Ŭ�� 1�� ����
            }
        }
    }

    private void OnCloneDeath()
    {
        Debug.Log("���� Ŭ�� 1�� ����");

        if (BossMonster != null)
        {

            BossMonster.OnBossClone1Death(); // �������� ���� ���� ��û
        }
        else
        {
            Debug.LogError("BossMonster�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        Destroy(gameObject); // Ŭ�� ����
    }

    public override void TakeDamageFromArrow(float damage, bool knockbackEnabled, Vector2 knockbackDirection, bool applyDot = false, int dotDamage = 0, bool isAoeHit = false)
    {
        if (hp > 0)
        {
            // ������ ����
            hp -= damage;

            if (hp <= 0)
            {
                OnCloneDeath(); // ���� Ŭ�� 1�� ����
            }

            if (knockbackEnabled && !isKnockedBack && rb != null && !isAoeHit)
            {
                ApplyKnockback(knockbackDirection);
            }

            if (applyDot && dotDamage > 0)
            {
                ApplyDot(dotDamage);
            }
        }

        StartCoroutine(PlayArrowHitAnimation());
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
