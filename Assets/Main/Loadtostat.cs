using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Button을 사용하기 위해 필요합니다;

public class Loadtostat : MonoBehaviour
{
    // 인스펙터에서 직접 설정할 수 있는 public 변수
    public string sceneNameToLoad;
    public Button loadButton; // 버튼 참조 추가

    // 이 메서드는 버튼 클릭에 의해 호출됩니다
    public void LoadScene()
    {
        if (loadButton != null)
        {
            loadButton.interactable = false; // 중복 클릭 방지
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
            loadButton.onClick.AddListener(LoadScene); // 버튼에 리스너 추가
        }
    }
}