using UnityEngine;
using UnityEngine.UI;

public class TurretButton : MonoBehaviour
{
    public Toggle toggle;

    void Start()
    {
        // ��� ���°� ����� ������ �̺�Ʈ�� ����
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
        Bal.Instance.isTurretActive = false;
        toggle.isOn = false;


    }

    // ��� ���°� ����� �� ȣ��Ǵ� �޼���
    void OnToggleValueChanged(bool isTurretActive)
    {
        // ����� Ȱ��ȭ�Ǹ�
        if (isTurretActive)
        {
            // Bal Ŭ������ isTurretActive ������ true�� ����
            Bal.Instance.isTurretActive = true;
        }
        else
        {
            // ����� ��Ȱ��ȭ�Ǹ�
            Bal.Instance.isTurretActive = false;
        }
    }

    // ��ũ��Ʈ�� �ı��� �� �̺�Ʈ ���� ����
    void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }
}
