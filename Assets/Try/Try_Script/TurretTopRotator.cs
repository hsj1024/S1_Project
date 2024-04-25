using UnityEngine;

public class TurretTopRotator : MonoBehaviour
{
    public Turret turret;

    void Update()
    {
        if (turret != null && turret.target != null)
        {
            // Ÿ���� ���� ���͸� ����մϴ�.
            Vector2 directionToTarget = turret.target.position - transform.position;

            // �ش� ���ͷκ��� ������ ���մϴ�.
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f; // �ͷ��� �̹����� �⺻������ ������ ����Ű�� �ִٰ� �����մϴ�. ���� �ٸ��ٸ� ���⼭ �����մϴ�.

            // Quaternion.Euler�� ����Ͽ� Z�� ȸ���� �����մϴ�.
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            // Slerp �Լ��� ����Ͽ� �ε巯�� ȸ���� �����մϴ�.
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5f);
        }
    }
}
