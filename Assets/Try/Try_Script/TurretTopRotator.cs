using UnityEngine;

public class TurretTopRotator : MonoBehaviour
{
    public Turret turret;

    void Update()
    {
        if (turret != null && turret.target != null)
        {
            // 타겟을 향한 벡터를 계산합니다.
            Vector2 directionToTarget = turret.target.position - transform.position;

            // 해당 벡터로부터 각도를 구합니다.
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f; // 터렛의 이미지가 기본적으로 위쪽을 가리키고 있다고 가정합니다. 만약 다르다면 여기서 수정합니다.

            // Quaternion.Euler을 사용하여 Z축 회전만 적용합니다.
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            // Slerp 함수를 사용하여 부드러운 회전을 적용합니다.
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5f);
        }
    }
}
