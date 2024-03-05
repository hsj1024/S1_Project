using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PanelController : MonoBehaviour
{
    // �г� Ȱ��ȭ�� ���� public ����. �ν����Ϳ��� ������ �� �ֽ��ϴ�.
    public GameObject panelToActivate;
    public GameObject popupToDeactivate;

    // �г��� Ȱ��ȭ�ϴ� �޼���
    public void ActivatePanel()
    {
        if (panelToActivate != null)
            panelToActivate.SetActive(true); // ������ �г��� Ȱ��ȭ
    }

    // �˾��� ��Ȱ��ȭ�ϴ� �޼���
    public void DeactivatePopup()
    {
        if (popupToDeactivate != null)
            popupToDeactivate.SetActive(false); // ������ �˾��� ��Ȱ��ȭ
    }
}

