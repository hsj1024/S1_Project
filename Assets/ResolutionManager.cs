using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResolutionManager : MonoBehaviour
{
    public static ResolutionManager Instance;
    public Vector2 referenceResolution = new Vector2(1920, 1080); // �⺻ �ػ� ����

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ������Ʈ�� �� ��ȯ �� �ı����� �ʵ��� ����
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AdjustCanvasScaler();
    }

    void AdjustCanvasScaler()
    {
        CanvasScaler[] canvasScalers = FindObjectsOfType<CanvasScaler>();

        foreach (CanvasScaler canvasScaler in canvasScalers)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = referenceResolution;
        }
    }
}
