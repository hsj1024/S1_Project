using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // SceneManager를 사용하기 위해 필요합니다.

public class Loadtostat : MonoBehaviour
{
    // 인스펙터에서 직접 설정할 수 있는 public 변수
    public string sceneNameToLoad;

    // 이 메서드는 버튼 클릭에 의해 호출됩니다
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneNameToLoad);
    }
}