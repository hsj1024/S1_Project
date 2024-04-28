using UnityEngine;

public class SpeedToggle : MonoBehaviour
{
    public static Monster Instance { get; private set; }

    public static bool isFastMode = false;

    // 이 함수는 버튼이 클릭될 때 호출됩니다.
    public void ToggleSpeed()
    {
        if (isFastMode)
        {
            Time.timeScale = 1f; // 게임 속도를 원래대로 돌립니다.
            isFastMode = false;
        }
        else
        {
            Time.timeScale = 5f; // 게임 속도를 5배로 높입니다.
            isFastMode = true;
        }
    }
}
