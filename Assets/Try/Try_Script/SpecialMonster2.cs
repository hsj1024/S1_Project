using System.Collections;
using UnityEngine;

public class SpecialMonster2 : Monster
{
    public float zigzagAmplitude = 20f; // 지그재그 이동의 진폭 증가
    public float zigzagFrequency = 0.1f; // 지그재그 이동의 주기, 낮출수록 천천히 이동
    public float verticalSpeed = 0.2f; // 수직 이동 속도 조정


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

        // 스폰된 위치에 따라 지그재그 방향 설정
        float zigzagDirection = moveLeftToRight ? 1 : -1;
        float zigzag = Mathf.PingPong(Time.time * zigzagFrequency, 1) * 2 - 1; // -1에서 1 사이로 지그재그 이동

        // 대각선 이동 벡터 계산
        Vector3 direction = new Vector3(zigzag * zigzagAmplitude * zigzagDirection, -verticalSpeed, 0);

        transform.position += direction * Time.deltaTime;

        // 몬스터가 카메라 경계를 벗어나지 않도록 설정
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        viewportPos.x = Mathf.Clamp(viewportPos.x, 0.1f, 0.9f); // 수평 경계 내에 유지
        viewportPos.y = Mathf.Clamp(viewportPos.y, 0, 1); // 수직 경계 내에 유지
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
