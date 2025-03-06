using System.Collections;
using UnityEngine;

public class SpecialMonster2 : Monster
{
    public float zigzagAmplitude = 20f; // ������� �̵��� ���� ����
    public float zigzagFrequency = 0.1f; // ������� �̵��� �ֱ�, ������� õõ�� �̵�
    public float verticalSpeed = 0.2f; // ���� �̵� �ӵ� ����


    private bool moveLeftToRight;

    private new void Start()
    {
        base.Start();
        moveLeftToRight = transform.position.x < 0;
        base.isSpecialMonster = true;
    }


    public override void MoveDown()
    {
        if (isKnockedBack || (currentHitInstance != null && hp <= 0)) return;

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
}
