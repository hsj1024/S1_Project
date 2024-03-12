using UnityEngine;
using UnityEngine.UI; // UI ���ӽ����̽��� ����ϱ� ���� �ʿ�

public class SoundSettings : MonoBehaviour
{
    public Slider volumeSlider; // ���� ���� �����̴�
    public AudioSource bgmSource; // ��� ������ ����� AudioSource

    void Start()
    {
        // �ʱ� ���� ���� �����̴��� ������ ����
        volumeSlider.value = bgmSource.volume;
        // �����̴� ���� ���� �� ȣ��� �޼��带 �߰�
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        // AudioSource�� ������ �����̴��� ������ ����
        bgmSource.volume = volume;
    }
}
