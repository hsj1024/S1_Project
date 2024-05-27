using System.Collections;
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
    public Text currentLevel;
    public GameObject levelUpPopup; // ������ �˾� UI
    public Button closeButton; // �˾��� �ݱ� ��ư
    public GameObject overlayPanel; // �������� �г� ����
    public Bal balInstance; // Bal Ŭ������ �ν��Ͻ� ����

    private bool isGamePaused = false; // ���� �Ͻ����� ����
    public List<StatUpgrade> statUpgrades = new List<StatUpgrade>();

    public GameObject specialLevelUpPanel; // Ư�� ������ �г�
    public List<StatUpgrade> specialStatUpgrades = new List<StatUpgrade>(); // Ư�� ������ ���� ���׷��̵� ����Ʈ �߰�
    public Button[] specialUpgradeButtons; // Ư�� �������� ���� ��ư �迭

    public List<GameObject> specialButtonPrefabs; // Ư�� ���� ���׷��̵� ��ư �������� ������ ����Ʈ

    private StatManager statManager; // StatManager�� ���� ������ ������ �ʵ�
    public static LevelManager Instance { get; private set; }
    // LevelManager ���� ��ư ���� ���� �߰�
    public Button[] upgradeButtons; // �� �迭�� Inspector���� �ʱ�ȭ

    public int totalMonstersKilled = 0; // �� ���� óġ ���� ������ ����
    public GameObject gameOverPanel;

    
    //��ư 

    public Transform[] buttonPositions; // ��ư ��ġ�� ������ �迭
    public List<GameObject> buttonPrefabs; // ���Ⱥ��� �ٸ� ��ư �������� ������ ����Ʈ

    public GameObject[] normalCardObjects; // �Ϲ� ������ ī�� ���� ������Ʈ �迭



    [System.Serializable]
    public class StatUpgrade
    {
        public string name;
        public float effect;
        public int probability;
        public GameObject buttonPrefab; // �� ���� ���׷��̵忡 �ش��ϴ� ��ư ������

        public StatUpgrade(string name, float effect, int probability, GameObject buttonPrefab)
        {
            this.name = name;
            this.effect = effect;
            this.probability = probability;
            this.buttonPrefab = buttonPrefab;
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
        
        
            // currentLevel �ؽ�Ʈ�� ã�ƾ� �մϴ�.
        GameObject currentLevelObject = GameObject.Find("currentLevel");
        if (currentLevelObject != null)
        {
            currentLevel = currentLevelObject.GetComponent<Text>();
            if (currentLevel == null)
            {
                Debug.LogError("currentLevel �ؽ�Ʈ ������Ʈ�� ã�� �� �����ϴ�.");
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
        UpdateLevelDisplay(); // ���� ���� �� ���� ǥ�ø� ������Ʈ


        // StatManager�� ���� �̱��� �ν��Ͻ��� ���� ã���ϴ�.
        statManager = StatManager.Instance;
        InitializeStatUpgrades();
        InitializeSpecialStatUpgrades();

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


        if (SceneManager.GetActiveScene().name == "StatSetting")
        {
            StatManager.Instance.SetupButtons();
        }
        // �ʱ� ������ �䱸 ����ġ ����
        UpdateLevelUpRequirement();

        // �ʱ⿡�� ������ �˾��� ��Ȱ��ȭ
        levelUpPopup.SetActive(false);
        overlayPanel.SetActive(false);

        // �ݱ� ��ư�� �̺�Ʈ ������ �߰�
        //closeButton.onClick.AddListener(CloseLevelUpPopup);
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
        statUpgrades.Add(new StatUpgrade("���ط� ���� 1", 2, 11, buttonPrefabs[0]));
        statUpgrades.Add(new StatUpgrade("���ط� ���� 2", 3, 9, buttonPrefabs[1]));
        statUpgrades.Add(new StatUpgrade("���ط� ���� 3", 5, 5, buttonPrefabs[2]));

        statUpgrades.Add(new StatUpgrade("������ �ð� ����", -0.05f, 7, buttonPrefabs[3]));
        statUpgrades.Add(new StatUpgrade("����ü �ӵ� ����", 2, 7, buttonPrefabs[4])); // ���� ����

        statUpgrades.Add(new StatUpgrade("ġ��Ÿ Ȯ�� ����", 5, 9, buttonPrefabs[5]));

        statUpgrades.Add(new StatUpgrade("ġ��Ÿ ���ط� ���� 1", 15, 8, buttonPrefabs[6]));

        statUpgrades.Add(new StatUpgrade("ġ��Ÿ ���ط� ���� 2", 25, 5, buttonPrefabs[7]));

        statUpgrades.Add(new StatUpgrade("���� ���ط� ����", 3, 7, buttonPrefabs[8]));
        statUpgrades.Add(new StatUpgrade("���� ���ط� ����", 5, 7, buttonPrefabs[9]));
        statUpgrades.Add(new StatUpgrade("���� ���ط� ����", 10, 7, buttonPrefabs[10]));

        statUpgrades.Add(new StatUpgrade("�ڵ� �ͷ� ������ �ð� ����", -0.3f, 4, buttonPrefabs[11]));
        statUpgrades.Add(new StatUpgrade("�ڵ� �ͷ� ���ط� ����", 5, 4, buttonPrefabs[12]));
        statUpgrades.Add(new StatUpgrade("����ġ ��� ���� 1", 0.2f, 7, buttonPrefabs[13]));
        statUpgrades.Add(new StatUpgrade("����ġ ��� ���� 2", 0.4f, 3, buttonPrefabs[14]));


    }

    void InitializeSpecialStatUpgrades()
    {
        // Ư�� ���׷��̵� �߰�
        specialStatUpgrades.Add(new StatUpgrade("���� ���� �ο�", 15, 15, specialButtonPrefabs[0]));
        specialStatUpgrades.Add(new StatUpgrade("���� ���� �ο�", 15, 15, specialButtonPrefabs[1]));
        specialStatUpgrades.Add(new StatUpgrade("����ü �� ����(�ߺ�)", 15, 15, specialButtonPrefabs[2]));
        specialStatUpgrades.Add(new StatUpgrade("����ü ���� �ο�", 15, 15, specialButtonPrefabs[3]));
        specialStatUpgrades.Add(new StatUpgrade("���� ��� ǥ��", 15, 15, specialButtonPrefabs[4]));
        specialStatUpgrades.Add(new StatUpgrade("�ڵ� �ͷ� ����", 15, 15, specialButtonPrefabs[5]));
        specialStatUpgrades.Add(new StatUpgrade("����ü �˹� �ο�", 15, 15, specialButtonPrefabs[6]));

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
        UpdateLevelDisplay(); // ������ �䱸 ������ ������Ʈ�� �� ���� ǥ�õ� ������Ʈ

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
            NextLevelXP = 2.5f; // ���� 20���� ���� 40���� �׽�Ʈ �ϴ��� ���� �ٽ� ���ľ���
        }
        else if (Level >= 40)
        {
            NextLevelXP = (16 + (16 * Level)) / 2.0f; // ���� 40 �̻�
        }
    }

    

    

    public void ShowLevelUpPopup()
    {
        List<StatUpgrade> selectedUpgrades = SelectRandomStatUpgrades();
        UpdateLevelDisplay(); // ������ �˾��� ������ �� ���� ǥ�� ������Ʈ


        // ��� ���� ��ư�� ��Ȱ��ȭ �� ����
        for (int i = 0; i < buttonPositions.Length; i++)
        {
            if (buttonPositions[i].childCount > 0)
            {
                Destroy(buttonPositions[i].GetChild(0).gameObject);  // ������ �ִ� ��ư ����
            }
        }

        // Ư�� �������� ���
        if (Level % 20 == 1) // Ư�� ������ ����
        {
            specialLevelUpPanel.SetActive(true); // Ư�� ������ �г� Ȱ��ȭ
            overlayPanel.SetActive(true);

            levelUpPopup.SetActive(false); // �Ϲ� ������ �г� ��Ȱ��ȭ

            for (int i = 0; i < specialUpgradeButtons.Length; i++)
            {
                if (i < selectedUpgrades.Count)
                {
                    GameObject buttonPrefab = Instantiate(selectedUpgrades[i].buttonPrefab, specialUpgradeButtons[i].transform.position, Quaternion.identity, specialUpgradeButtons[i].transform);
                    buttonPrefab.transform.localPosition = Vector3.zero; // ��ġ ����
                    buttonPrefab.transform.localRotation = Quaternion.identity; // ȸ�� ����
                    buttonPrefab.transform.localScale = Vector3.one; // ũ�� ����

                    RectTransform rectTransform = buttonPrefab.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.sizeDelta = new Vector2(499, 600); // ���ϴ� ũ��� ����
                    }

                    // �ؽ�Ʈ ������Ʈ
                    TMP_Text buttonText = buttonPrefab.GetComponentInChildren<TMP_Text>();
                    if (buttonText != null)
                    {
                        buttonText.text = $"{selectedUpgrades[i].name} (+{selectedUpgrades[i].effect})";
                    }
                    else
                    {
                        Debug.LogError("TMP_Text component not found in the button prefab.");
                    }

                    // ��ư�� ������ �߰�
                    Button btn = buttonPrefab.GetComponent<Button>();
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => ApplyStatUpgrade(selectedUpgrades[i]));
                    buttonPrefab.SetActive(true);

                   /* // ī�� �ִϸ��̼� ���
                    CardAnimation cardAnim = buttonPrefab.GetComponent<CardAnimation>();
                    if (cardAnim != null)
                    {
                        cardAnim.PlayAnimation();
                    }*/
                }
                else
                {
                    specialUpgradeButtons[i].gameObject.SetActive(false);
                }
            }
        }
        else // �Ϲ� ������ ó��
        {
            overlayPanel.SetActive(true);

            levelUpPopup.SetActive(true); // �Ϲ� ������ �г� Ȱ��ȭ
            specialLevelUpPanel.SetActive(false); // Ư�� ������ �г� ��Ȱ��ȭ

            // ���õ� ���׷��̵忡 �ش��ϴ� ���ο� ��ư�� �����ϰ� ���� ����
            for (int i = 0; i < selectedUpgrades.Count; i++)
            {
                if (i < buttonPositions.Length)
                {
                    GameObject buttonObject = Instantiate(selectedUpgrades[i].buttonPrefab, buttonPositions[i].position, Quaternion.identity, buttonPositions[i]);
                    SetupButton(buttonObject, selectedUpgrades[i]); // ��ư �ʱ�ȭ
                    //buttonObject.transform.localPosition = Vector3.zero; // ��ġ ����
                    //buttonObject.transform.localRotation = Quaternion.identity; // ȸ�� ����
                    //buttonObject.transform.localScale = Vector3.one; // ũ�� ����

                    RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.sizeDelta = new Vector2(414, 600); // ���ϴ� ũ��� ����
                    }
                    // �ִϸ��̼� Ʈ���� �߰�
                    CardAnimation cardAnim = buttonObject.GetComponent<CardAnimation>();
                    if (cardAnim != null)
                    {
                        cardAnim.card = buttonObject;
                        string animationName = GetAnimationName(selectedUpgrades[i].name);
                        StartCoroutine(PlayCardAnimation(cardAnim, animationName));
                    }


                    Button button = buttonObject.GetComponent<Button>();
                    button.gameObject.SetActive(true);
                    button.interactable = true; // ��ư Ȱ��ȭ
                    button.GetComponentInChildren<TMP_Text>().text = $"{selectedUpgrades[i].name} (+{selectedUpgrades[i].effect})";

                    // �̺�Ʈ ������ ����
                    button.onClick.RemoveAllListeners();
                    int captureIndex = i; // Ŭ������ ���� �ε��� ����

                    button.onClick.AddListener(() => ApplyStatUpgrade(selectedUpgrades[captureIndex]));

                    
                }
            }

            overlayPanel.SetActive(true);
            levelUpPopup.SetActive(true);
        }
    }
    void SetupButton(GameObject buttonObject, StatUpgrade upgrade)
    {
        buttonObject.transform.localPosition = Vector3.zero;
        buttonObject.transform.localRotation = Quaternion.identity;
        buttonObject.transform.localScale = Vector3.one;

        Button button = buttonObject.GetComponent<Button>();
        button.gameObject.SetActive(true);
        button.interactable = true;
        button.GetComponentInChildren<TMP_Text>().text = $"{upgrade.name} (+{upgrade.effect})";
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => ApplyStatUpgrade(upgrade));
        // �ִϸ��̼� Ʈ���� �߰�
        CardAnimation cardAnim = buttonObject.GetComponent<CardAnimation>();
        if (cardAnim != null)
        {
            cardAnim.card = buttonObject;
            string animationName = GetAnimationName(upgrade.name);
            StartCoroutine(PlayCardAnimation(cardAnim, animationName));
        }
        else
        {
            Debug.LogError("CardAnimation component not found on the button object.");
        }
    }

    // ���� ���� ȭ�鿡 ǥ��
    void UpdateLevelDisplay()
    {
        if (currentLevel != null)
        {
            currentLevel.text = "Lvl: " + Level;
        }
        else
        {
            Debug.LogError("currentLevel text component is not assigned!");
        }
    }




    List<StatUpgrade> SelectRandomStatUpgrades()
    {
        List<StatUpgrade> selected = new List<StatUpgrade>();
        HashSet<int> usedIndices = new HashSet<int>();
        int numUpgrades = Level % 20 == 1 ? 2 : 3; // Ư�� �������̸� 2��, �ƴϸ� 3��
        List<StatUpgrade> upgradeList = Level % 20 == 1 ? specialStatUpgrades : statUpgrades;

        while (selected.Count < numUpgrades && selected.Count < upgradeList.Count)
        {
            int index = UnityEngine.Random.Range(0, upgradeList.Count);
            if (!usedIndices.Contains(index))
            {
                selected.Add(upgradeList[index]);
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

    //���� �߰�

    public void IncrementMonsterKillCount()
    {
        totalMonstersKilled++;
    }


    public void GameOver()
    {
        // �÷��̾��� ���� óġ ��, ������ ����, ���ʽ� ������ PlayerPrefs�� ����
        PlayerPrefs.SetInt("TotalMonstersKilled", totalMonstersKilled);
        PlayerPrefs.SetInt("LevelReached", Level);
        float bonusStats = Mathf.Floor(Level * 0.1f);
        PlayerPrefs.SetFloat("BonusStats", bonusStats);

        // STATMANAGER�� ���� ������ ����
        PlayerPrefs.SetInt("DmgUpgradeCount", StatManager.Instance.dmgUpgradeCount);
        PlayerPrefs.SetInt("RtUpgradeCount", StatManager.Instance.rtUpgradeCount);
        PlayerPrefs.SetInt("XpmUpgradeCount", StatManager.Instance.xpmUpgradeCount);
        PlayerPrefs.SetInt("TurretDmgUpgradeCount", StatManager.Instance.turretDmgUpgradeCount);
        PlayerPrefs.SetInt("Points", StatManager.Instance.points);

        // ���� ���� �г��� Ȱ��ȭ
        gameOverPanel.SetActive(true);

        // ������ �ð��� ���� �� ���� ������ ���ư��� �ڷ�ƾ ����
        StartCoroutine(ReturnToMainAfterDelay(5f));
    }
    IEnumerator ReturnToMainAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        StatManager.Instance.LoadStatsFromPlayerPrefs();

        Time.timeScale = 1f;
        SceneManager.LoadScene("Main/Main");
        gameOverPanel.SetActive(false);
    }

    private string GetAnimationName(string statName)
    {
        switch (statName)
        {
            case "���ط� ���� 1":
            case "���ط� ���� 2":
            case "���ط� ���� 3":
                return "UI_Dmg";
            case "������ �ð� ����":
                return "UI_Rt";
            case "����ü �ӵ� ����":
                return "UI_As";
            case "ġ��Ÿ Ȯ�� ����":
                return "UI_Chc";
            case "ġ��Ÿ ���ط� ���� 1":
            case "ġ��Ÿ ���ط� ���� 2":
                return "UI_Chd";
            case "���� ���ط� ����":
                return "UI_Dot";
            case "���� ���ط� ����":
                return "UI_Doa";
            case "���� ���ط� ����":
                return "UI_Pd";
            case "�ڵ� �ͷ� ������ �ð� ����":
                return "UI_Tur_Rt";
            case "�ڵ� �ͷ� ���ط� ����":
                return "UI_Tur_Dmg";
            case "����ġ ��� ���� 1":
            case "����ġ ��� ���� 2":
                return "UI_Xpm";
            default:
                return "UI_Chc";
        }
    }

    private IEnumerator PlayCardAnimation(CardAnimation cardAnim, string animationName, float animationSpeed = 1.0f)
    {
        // �ִϸ��̼� ���� ���� ��ü�� ��ȿ���� Ȯ��
        if (cardAnim != null && cardAnim.card != null)
        {
            cardAnim.PlayAnimation(animationName, animationSpeed);
            yield return new WaitForSecondsRealtime(cardAnim.animationDuration);

            // �ִϸ��̼��� ���� �Ŀ� ī�带 Ȱ��ȭ�մϴ�.
            if (cardAnim.card != null) // �ٽ� ��ü�� ��ȿ���� Ȯ��
            {
                cardAnim.card.SetActive(true);
            }
        }
    }



}
