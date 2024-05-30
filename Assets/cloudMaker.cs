using UnityEngine;

public class cloudMaker : MonoBehaviour
{
    public float speed = 1.0f; // 구름 이동 속도
    private float screenWidth; // 화면 너비
    private float cloudWidth; // 구름의 너비
    private Vector3 startPosition; // 초기 위치

    void Start()
    {
        // 구름의 너비 계산 (구름의 SpriteRenderer 컴포넌트를 통해)
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        cloudWidth = spriteRenderer.bounds.size.x;

        // 화면 너비 계산 (Orthographic 카메라 기준)
        screenWidth = Camera.main.orthographicSize * Camera.main.aspect;

        // 구름의 초기 위치 설정 (현재 위치 사용)
        startPosition = transform.position;
    }

    void Update()
    {
        // 구름을 오른쪽으로 이동
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // 구름이 화면 오른쪽 끝을 넘어서 완전히 사라지면
        if (transform.position.x - cloudWidth / 2 > screenWidth)
        {
            // 구름을 화면 왼쪽 바깥쪽에서 다시 생성
            float newX = -screenWidth - cloudWidth / 2;
            transform.position = new Vector3(newX, startPosition.y, startPosition.z);
        }
    }
}
