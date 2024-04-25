using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PanelController : MonoBehaviour
{
    // �г� Ȱ��ȭ�� ���� public ����. �ν����Ϳ��� ������ �� �ֽ��ϴ�.
    public GameObject panelToActivate;
    public GameObject popupToDeactivate;

    // �г��� Ȱ��ȭ�ϴ� �޼���
    public void ActivatePanel()
    {
        if (panelToActivate != null)
        {
            panelToActivate.SetActive(true); // ������ �г��� Ȱ��ȭ
            PauseGame(); // ���� �Ͻ� ����
        }
    }

    // �˾��� ��Ȱ��ȭ�ϴ� �޼���
    public void DeactivatePopup()
    {
        if (popupToDeactivate != null)
        {
            popupToDeactivate.SetActive(false); // ������ �˾��� ��Ȱ��ȭ
            ResumeGame(); // ���� �簳
        }
    }

    // ���� �Ͻ� ���� �޼���
    private void PauseGame()
    {
        Time.timeScale = 0f; // ���� �ð��� ����
    }

    // ���� �簳 �޼���
    private void ResumeGame()
    {
        Time.timeScale = 1f; // ���� �ð��� �ٽ� ����
    }

}
