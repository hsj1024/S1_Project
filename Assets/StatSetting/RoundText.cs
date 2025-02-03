using UnityEngine;
using TMPro;

public class RoundText : MonoBehaviour
{
    public TextMeshProUGUI roundText; // TextMeshPro�� ����

    void Start()
    {
        // PlayerPrefs���� ���� ȸ�� ������ ������
        int currentRound = PlayerPrefs.GetInt("CurrentRound", 1); // �⺻�� 1
        roundText.text = $"{currentRound} ȸ��"; // �ؽ�Ʈ ����
    }
}
