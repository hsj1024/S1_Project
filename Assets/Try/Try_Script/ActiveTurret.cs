using UnityEngine;

public class ActiveTurret : MonoBehaviour
{
    public GameObject turret; // �ͷ� ������Ʈ�� ���� ����

    // �ͷ� Ȱ��ȭ �޼���
    public void ToggleTurretActive()
    {
        // �ͷ��� Ȱ��ȭ ���¸� ���
        turret.SetActive(!turret.activeSelf);
    }
}