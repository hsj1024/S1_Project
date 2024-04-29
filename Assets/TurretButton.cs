using UnityEngine;
using UnityEngine.UI;

public class TurretButton : MonoBehaviour
{
    public Toggle toggle;

    void Start()
    {
        // 토글 상태가 변경될 때마다 이벤트에 연결
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
        Bal.Instance.isTurretActive = false;
        toggle.isOn = false;


    }

    // 토글 상태가 변경될 때 호출되는 메서드
    void OnToggleValueChanged(bool isTurretActive)
    {
        // 토글이 활성화되면
        if (isTurretActive)
        {
            // Bal 클래스의 isTurretActive 변수를 true로 설정
            Bal.Instance.isTurretActive = true;
        }
        else
        {
            // 토글이 비활성화되면
            Bal.Instance.isTurretActive = false;
        }
    }

    // 스크립트가 파괴될 때 이벤트 연결 해제
    void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }
}
