using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PanelController : MonoBehaviour
{
    // 패널 활성화를 위한 public 변수. 인스펙터에서 설정할 수 있습니다.
    public GameObject panelToActivate;
    public GameObject popupToDeactivate;

    // 패널을 활성화하는 메서드
    public void ActivatePanel()
    {
        if (panelToActivate != null)
        {
            panelToActivate.SetActive(true); // 지정된 패널을 활성화
            PauseGame(); // 게임 일시 중지
        }
    }

    // 팝업을 비활성화하는 메서드
    public void DeactivatePopup()
    {
        if (popupToDeactivate != null)
        {
            popupToDeactivate.SetActive(false); // 지정된 팝업을 비활성화
            ResumeGame(); // 게임 재개
        }
    }

    // 게임 일시 중지 메서드
    private void PauseGame()
    {
        Time.timeScale = 0f; // 게임 시간을 정지
    }

    // 게임 재개 메서드
    private void ResumeGame()
    {
        Time.timeScale = 1f; // 게임 시간을 다시 시작
    }

}
