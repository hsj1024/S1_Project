using UnityEngine;
using UnityEngine.SceneManagement;

public class DataResetManager : MonoBehaviour
{
    private void Update()
    {
        // Ư�� Ű �Է� ���� (��: SŰ)
        if (Input.GetKeyDown(KeyCode.S))
        {
            ResetPlayerPrefs();
        }
    }

    // PlayerPrefs ������ �ʱ�ȭ
    private void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll(); // ��� PlayerPrefs ������ ����
        PlayerPrefs.Save(); // ���� ���� ����
        Debug.Log("PlayerPrefs �����Ͱ� �ʱ�ȭ�Ǿ����ϴ�.");
    }
}
