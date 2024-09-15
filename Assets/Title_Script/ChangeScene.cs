using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // SceneManager�� ����ϱ� ���� �ʿ��մϴ�.

public class ChangeScene : MonoBehaviour
{
    // �ν����Ϳ��� ���� ������ �� �ִ� public ����
    //public string sceneNameToLoad;

    // �� �޼���� ��ư Ŭ���� ���� ȣ��˴ϴ�
    public void LoadSceneTry()
    {
        SceneManager.LoadScene("Try");
    }

    public void LoadSceneMain()
    {
        SceneManager.LoadScene("Main");
    }

    public void LoadSceneTitle()
    {
        SceneManager.LoadScene("Title");
    }
}