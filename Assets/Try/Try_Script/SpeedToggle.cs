using UnityEngine;

public class SpeedToggle : MonoBehaviour
{
    public static Monster Instance { get; private set; }

    public static bool isFastMode = false;

    // �� �Լ��� ��ư�� Ŭ���� �� ȣ��˴ϴ�.
    public void ToggleSpeed()
    {
        if (isFastMode)
        {
            Time.timeScale = 1f; // ���� �ӵ��� ������� �����ϴ�.
            isFastMode = false;
        }
        else
        {
            Time.timeScale = 5f; // ���� �ӵ��� 5��� ���Դϴ�.
            isFastMode = true;
        }
    }
}
