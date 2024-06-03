using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Button�� ����ϱ� ���� �ʿ��մϴ�;

public class Loadtostat : MonoBehaviour
{
    // �ν����Ϳ��� ���� ������ �� �ִ� public ����
    public string sceneNameToLoad;
    public Button loadButton; // ��ư ���� �߰�

    // �� �޼���� ��ư Ŭ���� ���� ȣ��˴ϴ�
    public void LoadScene()
    {
        if (loadButton != null)
        {
            loadButton.interactable = false; // �ߺ� Ŭ�� ����
        }
        StartCoroutine(LoadSceneAsync());
    }
    IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneNameToLoad);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
    void Start()
    {
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(LoadScene); // ��ư�� ������ �߰�
        }
    }
}