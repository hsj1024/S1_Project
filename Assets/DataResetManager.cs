using UnityEngine;
using UnityEngine.SceneManagement;

public class DataResetManager : MonoBehaviour
{
    private void Update()
    {
        // 특정 키 입력 감지 (예: S키)
        if (Input.GetKeyDown(KeyCode.S))
        {
            ResetPlayerPrefs();
        }
    }

    // PlayerPrefs 데이터 초기화
    private void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll(); // 모든 PlayerPrefs 데이터 삭제
        PlayerPrefs.Save(); // 변경 사항 저장
        Debug.Log("PlayerPrefs 데이터가 초기화되었습니다.");
    }
}
