using UnityEngine;

public class ActiveTurret : MonoBehaviour
{
    public GameObject turret; // 터렛 오브젝트에 대한 참조

    // 터렛 활성화 메서드
    public void ToggleTurretActive()
    {
        // 터렛의 활성화 상태를 토글
        turret.SetActive(!turret.activeSelf);
    }
}