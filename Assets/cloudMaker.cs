using UnityEngine;

public class cloudMaker : MonoBehaviour
{
    public float speed = 1.0f; // ���� �̵� �ӵ�
    private float screenWidth; // ȭ�� �ʺ�
    private float cloudWidth; // ������ �ʺ�
    private Vector3 startPosition; // �ʱ� ��ġ

    void Start()
    {
        // ������ �ʺ� ��� (������ SpriteRenderer ������Ʈ�� ����)
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        cloudWidth = spriteRenderer.bounds.size.x;

        // ȭ�� �ʺ� ��� (Orthographic ī�޶� ����)
        screenWidth = Camera.main.orthographicSize * Camera.main.aspect;

        // ������ �ʱ� ��ġ ���� (���� ��ġ ���)
        startPosition = transform.position;
    }

    void Update()
    {
        // ������ ���������� �̵�
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // ������ ȭ�� ������ ���� �Ѿ ������ �������
        if (transform.position.x - cloudWidth / 2 > screenWidth)
        {
            // ������ ȭ�� ���� �ٱ��ʿ��� �ٽ� ����
            float newX = -screenWidth - cloudWidth / 2;
            transform.position = new Vector3(newX, startPosition.y, startPosition.z);
        }
    }
}
