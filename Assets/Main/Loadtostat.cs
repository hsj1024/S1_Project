using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // SceneManager�� ����ϱ� ���� �ʿ��մϴ�.

public class Loadtostat : MonoBehaviour
{
    // �ν����Ϳ��� ���� ������ �� �ִ� public ����
    public string sceneNameToLoad;

    // �� �޼���� ��ư Ŭ���� ���� ȣ��˴ϴ�
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneNameToLoad);
    }
}