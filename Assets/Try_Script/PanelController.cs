using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PanelController : MonoBehaviour
{
    // 패널 활성화를 위한 public 변수. 인스펙터에서 설정할 수 있습니다.
    public GameObject panelToActivate;
    public GameObject popupToDeactivate;

    // 패널을 활성화하는 메서드
    public void ActivatePanel()
    {
        if (panelToActivate != null)
            panelToActivate.SetActive(true); // 지정된 패널을 활성화
    }

    // 팝업을 비활성화하는 메서드
    public void DeactivatePopup()
    {
        if (popupToDeactivate != null)
            popupToDeactivate.SetActive(false); // 지정된 팝업을 비활성화
    }
}

