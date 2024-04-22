using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static StatManager;

public class LevelManager : MonoBehaviour
{
    public int Level = 1;
    public float NextLevelXP = 2.5f; // ���� �������� �ʿ��� ����ġ
    public Text levelUpText; // ������ �ؽ�Ʈ UI
    public GameObject levelUpPopup; // ������ �˾� UI
    public Button closeButton; // �˾��� �ݱ� ��ư
    public GameObject overlayPanel; // �������� �г� ����
    public Bal balInstance; // Bal Ŭ������ �ν��Ͻ� ����

    private bool isGamePaused = false; // ���� �Ͻ����� ����
    public List<StatUpgrade> statUpgrades = new List<StatUpgrade>();
    private StatManager statManager; // StatManager�� ���� ������ ������ �ʵ�
    public static LevelManager Instance { get; private set; }
    // LevelManager ���� ��ư ���� ���� �߰�
    public Button[] upgradeButtons; // �� �迭�� Inspector���� �ʱ�ȭ
    [System.Serializable]
    public class StatUpgrade
    {
        public string name;
        public float effect;
        public int probability;

        public StatUpgrade(string name, float effect, int probability)
        {
            this.name = name;
            this.effect = effect;
            this.probability = probability;
        }
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // �� �ε� �Ϸ� �� �ʿ��� ������Ʈ ��˻� �� ������ �缳��
        statManager = FindObjectOfType<StatManager>();
        if (statManager == null)
        {
            Debug.LogError("StatManager ������Ʈ�� ã�� �� �����ϴ�.");
        }
        else
        {
            SetupButtons();
        }
    }
    void SetupButtons()
    {
        Debug.Log("SetupButtons called."); // �Լ��� ȣ��Ǿ����� Ȯ���ϱ� ���� �α�

        List<StatUpgrade> selectedUpgrades = SelectRandomStatUpgrades();

        // ��� ��ư�� �ϴ� Ȱ��ȭ
        foreach (var button in upgradeButtons)
        {
            button.gameObject.SetActive(true);
            button.interactable = false; // �ʱ⿡�� ��Ȱ��ȭ
        }

        // ��ȿ�� ���׷��̵尡 �ִ� ��ư�� Ȱ��ȭ
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < selectedUpgrades.Count)
            {
                var upgrade = selectedUpgrades[i];
                upgradeButtons[i].GetComponentInChildren<TMP_Text>().text = $"{upgrade.name} (+{upgrade.effect})";
                upgradeButtons[i].onClick.RemoveAllListeners();
                //upgradeButtons[i].onClick.AddListener(() => ApplyStatUpgrade(upgrade));
                upgradeButtons[i].interactable = true; // ��ư�� Ȱ��ȭ
            }
            else
            {
                upgradeButtons[i].GetComponentInChildren<TMP_Text>().text = "No Upgrade Available";
                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].interactable = false;  // ��ư�� ��Ȱ��ȭ
            }
        }
    }







    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // �� ��ȯ �� �ı����� ����
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    private List<StatUpgrade> selectedUpgrades = new List<StatUpgrade>(); // selectedUpgrades�� Ŭ���� �������� �̵�

    void Start()
    {
        /*SceneManager.LoadScene("Try", LoadSceneMode.Single);
        SceneManager.LoadScene("StatSetting", LoadSceneMode.Additive);*/

        // StatManager�� ���� �̱��� �ν��Ͻ��� ���� ã���ϴ�.
        statManager = StatManager.Instance;
        InitializeStatUpgrades();

        // Bal Ŭ������ �ν��Ͻ��� ã�� �����մϴ�.
        balInstance = FindObjectOfType<Bal>();
        if (balInstance == null)
        {
            Debug.LogError("Bal Ŭ������ �ν��Ͻ��� ã�� �� �����ϴ�.");
            return;
        }

        // StatManager�� �ν��Ͻ��� ã�� ������ �����մϴ�.
        statManager = FindObjectOfType<StatManager>();
        if (statManager == null)
        {
            Debug.LogError("StatManager ������Ʈ�� ���� �������� �ʽ��ϴ�.");
            return;
        }
        // LevelUpPopup�� �ִ� Canvas ����
        Canvas popupCanvas = levelUpPopup.GetComponentInParent<Canvas>();
        if (popupCanvas)
        {
            popupCanvas.sortingOrder = 5000;  // �ٸ� ��� Canvas���� ���� ���� ����
        }

        // �� ��ư�� ���� �̺�Ʈ �ڵ鷯 �߰�
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            int buttonIndex = i; // Ŭ������ �����ϱ� ���� �ε��� ������ ������ ����
            upgradeButtons[i].onClick.AddListener(() => ApplyStatUpgrade(selectedUpgrades[buttonIndex]));
        }

        // �ʱ� ������ �䱸 ����ġ ����
        UpdateLevelUpRequirement();

        // �ʱ⿡�� ������ �˾��� ��Ȱ��ȭ
        levelUpPopup.SetActive(false);
        overlayPanel.SetActive(false);

        // �ݱ� ��ư�� �̺�Ʈ ������ �߰�
        closeButton.onClick.AddListener(CloseLevelUpPopup);
    }
    public void ApplyStatUpgrade(StatUpgrade upgrade)
    {
        // StatManager�� �ν��Ͻ��� ���� ���׷��̵� ����
        if (StatManager.Instance != null)
        {
            StatManager.Instance.ApplyUpgrade(upgrade);
        }

        // �г��� �ݽ��ϴ�.
        CloseLevelUpPopup();
    }
    void InitializeStatUpgrades()
    {
        statUpgrades.Add(new StatUpgrade("���ط� ���� 1", 2, 11));
        statUpgrades.Add(new StatUpgrade("���ط� ���� 2", 3, 9));
        statUpgrades.Add(new StatUpgrade("���ط� ���� 3", 5, 5));
        statUpgrades.Add(new StatUpgrade("������ �ð� ����", -0.05f, 7));
        statUpgrades.Add(new StatUpgrade("����ü �ӵ� ����", 2, 7));

        statUpgrades.Add(new StatUpgrade("ġ��Ÿ Ȯ�� ����", 5, 9));

        statUpgrades.Add(new StatUpgrade("ġ��Ÿ ���ط� ���� 1", 15, 8));

        statUpgrades.Add(new StatUpgrade("ġ��Ÿ ���ط� ���� 2", 25, 5));

        statUpgrades.Add(new StatUpgrade("���� ���ط� ����", 3, 7));
        statUpgrades.Add(new StatUpgrade("���� ���ط� ����", 5, 7));
        statUpgrades.Add(new StatUpgrade("���� ���ط� ����", 10, 7));
        
        statUpgrades.Add(new StatUpgrade("�ڵ� �ͷ� ������ �ð� ����", -0.3f, 4));
        statUpgrades.Add(new StatUpgrade("�ڵ� �ͷ� ���ط� ����", 5, 4));
        statUpgrades.Add(new StatUpgrade("����ġ ��� ���� 1", 0.2f, 7));
        statUpgrades.Add(new StatUpgrade("����ġ ��� ���� 2", 0.4f, 3));


    }
    void Update()
    {
        // ������ �Ͻ����� ������ ���� �������� Ȯ������ �ʽ��ϴ�.
        if (!isGamePaused)
        {
            // ����ġ�� �ֱ������� Ȯ���Ͽ� �������� ó���մϴ�.
            if (balInstance.totalExperience >= NextLevelXP)
            {
                Level++; // ���� ����
                UpdateLevelUpRequirement(); // ���� ������ �䱸 ����ġ ������Ʈ
                PauseGame(); // ���� �Ͻ�����
                ShowLevelUpPopup(); // ������ �˾� ǥ��
                Debug.Log($"Level Up! New level: {Level}, New XP Requirement: {NextLevelXP}");
            }
        }
    }

    void UpdateLevelUpRequirement()
    {
        // ������ ������ �䱸 ����ġ ����
        if (Level == 1)
        {
            NextLevelXP = 2.5f; // ���� 1���� ���� 2�� ���� ���
        }
        else if (Level == 2)
        {
            NextLevelXP = 7.5f; // ���� 2���� ���� 3�� ���� ���
        }
        else if (Level >= 3 && Level < 20)
        {
            NextLevelXP = (5 + (10 * Level)) / 2.0f; // ���� 3���� ���� 20����
        }
        else if (Level >= 20 && Level < 40)
        {
            NextLevelXP = (13 + (13 * Level)) / 2.0f; // ���� 20���� ���� 40����
        }
        else if (Level >= 40)
        {
            NextLevelXP = (16 + (16 * Level)) / 2.0f; // ���� 40 �̻�
        }
    }
    void ShowLevelUpPopup()
    {
        selectedUpgrades = SelectRandomStatUpgrades(); // selectedUpgrades�� �ʱ�ȭ

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < selectedUpgrades.Count)
            {
                StatUpgrade upgrade = selectedUpgrades[i];
                upgradeButtons[i].GetComponentInChildren<TMP_Text>().text = $"{upgrade.name} (+{upgrade.effect})";
                upgradeButtons[i].onClick.RemoveAllListeners(); // ������ �Ҵ�� ��� �����ʸ� ����

                // Ŭ���� ������ �ذ��ϱ� ���� �ӽ� ������ ���׷��̵带 ������ �� �̸� Ŭ�� �̺�Ʈ�� ����
                StatUpgrade selectedUpgrade = upgrade;
                upgradeButtons[i].onClick.AddListener(() => ApplyStatUpgrade(selectedUpgrade));
                upgradeButtons[i].interactable = true; // ��ư Ȱ��ȭ
            }
            else
            {
                upgradeButtons[i].GetComponentInChildren<TMP_Text>().text = "No Upgrade Available";
                upgradeButtons[i].onClick.RemoveAllListeners(); // ������ �Ҵ�� ��� �����ʸ� ����
                upgradeButtons[i].interactable = false;  // ��ư ��Ȱ��ȭ
            }
        }

        overlayPanel.SetActive(true);
        levelUpPopup.SetActive(true);
    }




    /*   public void ApplyStatUpgrade(StatUpgrade upgrade)
       {
           // StatManager�� �ν��Ͻ��� ���� ���׷��̵� ����
           if (StatManager.Instance != null)
           {
               StatManager.Instance.ApplyUpgrade(upgrade);
           }
           CloseLevelUpPopup();
       }*/


    List<StatUpgrade> SelectRandomStatUpgrades()
    {
        List<StatUpgrade> selected = new List<StatUpgrade>();
        HashSet<int> usedIndices = new HashSet<int>();  // �ߺ� ���� ������ ���� HashSet

        while (selected.Count < 3 && selected.Count < statUpgrades.Count) // �� ���� ���׷��̵带 ������ �� ���� �� ���� ���� ���� �߰�
        {
            int index = UnityEngine.Random.Range(0, statUpgrades.Count);
            if (!usedIndices.Contains(index))
            {
                selected.Add(statUpgrades[index]);
                usedIndices.Add(index);
            }
        }
        return selected;
    }



    public void CloseLevelUpPopup()
    {
        overlayPanel.SetActive(false); // �������� �г� ��Ȱ��ȭ
        levelUpPopup.SetActive(false);

        ResumeGame(); // ���� �簳
    }

    void PauseGame()
    {
        Time.timeScale = 0f; // �ð��� ����ϴ�.
        isGamePaused = true;
    }

    void ResumeGame()
    {
        Time.timeScale = 1f; // �ð��� �ٽ� �����մϴ�.
        isGamePaused = false;
    }
}
