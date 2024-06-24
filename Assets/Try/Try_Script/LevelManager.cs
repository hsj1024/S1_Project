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
    public BallistaController ballistaController; // BallistaController ���� �߰�

    private StatManager statManager; // StatManager�� ���� ������ ������ �ʵ�
    public static LevelManager Instance { get; private set; }
    // LevelManager ���� ��ư ���� ���� �߰�
    public Button[] upgradeButtons; // �� �迭�� Inspector���� �ʱ�ȭ

    public int totalMonstersKilled = 0; // �� ���� óġ ���� ������ ����
    public GameObject gameOverPanel;

    public Canvas canvas; // Canvas ����
    public Camera mainCamera; // Camera ����

    public GameObject panelToActivate; // �г� Ȱ��ȭ�� ���� ����
    public GameObject popupToDeactivate; // �˾� ��Ȱ��ȭ�� ���� ����

    private bool isLevelUpPopupActive = false; // ������ �˾� Ȱ��ȭ ����

    //��ư 

    public Transform[] buttonPositions; // ��ư ��ġ�� ������ �迭
    public List<GameObject> buttonPrefabs; // ���Ⱥ��� �ٸ� ��ư �������� ������ ����Ʈ

    public GameObject[] normalCardObjects; // �Ϲ� ������ ī�� ���� ������Ʈ �迭
    public GameObject turretObject; // �ͷ� ������Ʈ�� ������ �� �ִ� ���� �߰�


    public GameObject settingsPanel; // ���� �г�
    public Button settingButton; // ���� ��ư

    public Button quitButton; // ������ ��ư

    public Slider xpSlider; // ����ġ �� UI
    public Image barImage;

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
            else
            {
                UpdateLevelDisplay(); // currentLevel �ؽ�Ʈ ������Ʈ�� ã������ ���� ǥ�ø� ������Ʈ�մϴ�.
            }
        }
        if (scene.name == "Main/Main")
        {
            levelUpPopup.SetActive(false);
            overlayPanel.SetActive(false);
            specialLevelUpPanel.SetActive(false);
            isGamePaused = false;
        }

        // GameOver �г� �ʱ�ȭ �� ��ġ ����
        GameOverUI gameOverUI = gameOverPanel.GetComponent<GameOverUI>();
        if (gameOverUI != null)
        {
            gameOverUI.ResetPanelSizeAndPosition();
        }
        mainCamera = Camera.main;
        AssignCameraToCanvas();

        // ���� �г� �ʱ�ȭ
        InitializeSettingsPanel();

        // ��ư �ʱ�ȭ �� ���� ���� ����
        InitializeButtons(scene.name);

        InitializeXpSlider();
    }

    private void InitializeButtons(string sceneName)
    {
        // Ư�� �������� ��ư�� Ȱ��ȭ
        if (sceneName == "Try")
        {
            if (settingButton != null)
            {
                settingButton.gameObject.SetActive(true);
            }
            else
            {
            }
        }
        else
        {
            if (settingButton != null)
            {
                settingButton.gameObject.SetActive(false);
            }
        }
    }
    private void InitializeSettingsPanel()
    {
        // ���� �г� �� ��ư �缳��
        if (settingsPanel == null)
        {
            settingsPanel = GameObject.Find("Setting_panel");
        }
        if (settingButton == null)
        {
            GameObject settingButtonObject = GameObject.Find("Setting_button");
            if (settingButtonObject != null)
            {
                settingButton = settingButtonObject.GetComponent<Button>();
                settingButton.onClick.RemoveAllListeners();
                settingButton.onClick.AddListener(ToggleSettingsPanel);
            }
        }

        // ������ ��ư �̺�Ʈ ����
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners(); // ���� ������ ����
            quitButton.onClick.AddListener(GameOver); // ���ο� ������ �߰�
        }
        else
        {
            Debug.LogError("QuitButton is not assigned in the inspector.");
        }
    }
    private void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf); // �г� Ȱ��ȭ/��Ȱ��ȭ ���
            if (settingsPanel.activeSelf)
            {
                PauseGame(); // �г��� Ȱ��ȭ�Ǹ� ������ �Ͻ� ����
            }
            else
            {
                ResumeGame(); // �г��� ��Ȱ��ȭ�Ǹ� ������ �簳
            }
        }
    }
    private void AssignCameraToCanvas()
    {
        if (canvas == null)
        {
            Debug.LogError("Canvas�� �Ҵ���� �ʾҽ��ϴ�.");
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("�� ���� Canvas�� ã�� �� �����ϴ�.");
                return;
            }
        }

        if (canvas.renderMode != RenderMode.ScreenSpaceCamera)
        {
            Debug.LogWarning("Canvas�� Render Mode�� ScreenSpaceCamera�� �ƴմϴ�. ������ �����մϴ�.");
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
        }

        if (mainCamera == null)
        {
            Debug.Log("Main Camera�� ã�� ��...");
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera�� ã�� �� �����ϴ�.");
                return;
            }
            else
            {
                Debug.Log("Main Camera�� �Ҵ�Ǿ����ϴ�: " + mainCamera.name);
            }
        }
        canvas.worldCamera = mainCamera;
        Debug.Log("Camera�� Canvas�� �Ҵ�Ǿ����ϴ�: " + mainCamera.name);
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

        // ���� �г� �ʱ�ȭ
        InitializeSettingsPanel();
    }
    private List<StatUpgrade> selectedUpgrades = new List<StatUpgrade>(); // selectedUpgrades�� Ŭ���� �������� �̵�

    void Start()
    {
        UpdateLevelDisplay(); // ���� ���� �� ���� ǥ�ø� ������Ʈ
        InitializeXpSlider(); // ����ġ �� �ʱ�ȭ

        

        // ������ ��ư �̺�Ʈ ����
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(GameOver);
        }
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
        // BallistaController�� �ν��Ͻ��� ã�� �����մϴ�.
        ballistaController = FindObjectOfType<BallistaController>();
        if (ballistaController == null)
        {
            Debug.LogError("BallistaController Ŭ������ ã�� �� �����ϴ�.");
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

    void InitializeXpSlider()
    {
        if (xpSlider != null)
        {
            xpSlider.maxValue = NextLevelXP;
            xpSlider.value = 0; // �ʱⰪ�� 0���� ����
            if (barImage != null)
            {
                barImage.fillAmount = 0f; // �ʱ�ȭ �� barImage.fillAmount�� 0���� ����
            }
            Canvas.ForceUpdateCanvases();
            Debug.Log("InitializeXpSlider: ����ġ �ٰ� �ʱ�ȭ�Ǿ����ϴ�."); // ����� �޽��� �߰�
        }
    }

    void UpdateExperienceBar()
    {
        if (xpSlider != null)
        {
            xpSlider.value = balInstance.totalExperience;
            if (barImage != null)
            {
                barImage.fillAmount = xpSlider.value / xpSlider.maxValue;
            }
        }
    }
    public void ApplyStatUpgrade(StatUpgrade upgrade)
    {
        // StatManager�� �ν��Ͻ��� ���� ���׷��̵� ����
        if (StatManager.Instance != null)
        {
            StatManager.Instance.ApplyUpgrade(upgrade);
        }
        // Bal Ŭ������ Ư�� ������ Ȱ��ȭ�ϴ� ���� �߰�
        if (balInstance != null)
        {
            switch (upgrade.name)
            {
                case "���� ���� �ο�":
                    balInstance.isDotActive = true;
                    break;
                case "���� ���� �ο�":
                    balInstance.isAoeActive = true;
                    break;
                case "����ü �� ����(�ߺ�)":
                    if (ballistaController != null)
                    {
                        ballistaController.IncreaseArrowCount(); // ����ü �� ����
                    }
                    break;
                case "����ü ���� �ο�":
                    balInstance.isPdActive = true;
                    break;
                case "�ڵ� �ͷ� ����":
                    balInstance.isTurretActive = true;
                    ActivateTurret(); // �ͷ� ������Ʈ Ȱ��ȭ
                    break;
                case "����ü �˹� �ο�":
                    balInstance.knockbackEnabled = true;
                    break;
                case "���� ��� ǥ��":
                    if (ballistaController != null)
                    {
                        ballistaController.SetLineRendererEnabled(true); // LineRenderer Ȱ��ȭ
                    }
                    break;

            }
        }

        
        // �г��� �ݽ��ϴ�.
        CloseLevelUpPopup();


        // ���� ������ �䱸 ����ġ�� ������Ʈ�մϴ�.
        UpdateLevelUpRequirement();
        /*// ����ġ �� �ʱ�ȭ
        if (xpSlider != null)
        {
            xpSlider.value = 0;
            Debug.Log("ApplyStatUpgrade: ����ġ �ٰ� �ʱ�ȭ�Ǿ����ϴ�."); // ����� �޽��� �߰�
        }*/
        // ����ġ �� �ʱ�ȭ
        balInstance.totalExperience = 0f;
        UpdateExperienceBar();
        // Ư�� ������ ���� �Ϲ� �������� ����
        if ((Level - 1) % 20 == 0)
        {
            levelUpPopup.SetActive(false);
            overlayPanel.SetActive(false);
            // �ٷ� ���� ������ �Ѿ�� �ʵ��� ����
            NextLevelXP += 10 * Level; // ������ ū �� �߰��Ͽ� ���� ���� �� ������ �������� �ʰ� �� �̰� �����ؾ��� �� !!!!!!!!!!!!!!!!
            return;
        }

    }

    private void ActivateTurret()
    {
        // �ͷ� ������Ʈ�� Ȱ��ȭ�մϴ�.
        if (turretObject != null)
        {
            turretObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Turret object is not assigned in the inspector.");
        }
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
        specialStatUpgrades.Add(new StatUpgrade("���� ���� �ο�", 15, 100, specialButtonPrefabs[0])); // dot
        specialStatUpgrades.Add(new StatUpgrade("���� ���� �ο�", 15, 0, specialButtonPrefabs[1])); // doa
        specialStatUpgrades.Add(new StatUpgrade("����ü �� ����(�ߺ�)", 15, 0, specialButtonPrefabs[2]));
        specialStatUpgrades.Add(new StatUpgrade("����ü ���� �ο�", 15, 0, specialButtonPrefabs[3])); // pd
        specialStatUpgrades.Add(new StatUpgrade("���� ��� ǥ��", 15, 0, specialButtonPrefabs[4])); // ���ذ��
        specialStatUpgrades.Add(new StatUpgrade("�ڵ� �ͷ� ����", 15, 0, specialButtonPrefabs[5]));  // turret
        specialStatUpgrades.Add(new StatUpgrade("����ü �˹� �ο�", 15, 0, specialButtonPrefabs[6])); // knockback 

    }
    void Update()
    {
        // ������ �Ͻ����� ������ ���� �������� Ȯ������ �ʽ��ϴ�.
        if (!isGamePaused && !gameOverPanel.activeSelf)
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

            // ����ġ �� ������Ʈ
            UpdateExperienceBar();
        }

    }

    void UpdateLevelUpRequirement()
    {
        UpdateLevelDisplay(); // ������ �䱸 ������ ������Ʈ�� �� ���� ǥ�õ� ������Ʈ

        if (Level == 1)
        {
            NextLevelXP = 3f; // ���� 1���� ���� 2�� ���� ���
        }
        else if (Level == 2)
        {
            NextLevelXP = 7f; // ���� 2���� ���� 3�� ���� ���
        }
        else if (Level >= 3 && Level < 20)
        {
            // ���� 3���� ���� 20���� �䱸 ����ġ ����
            // 13, 18, 23, 28, ... (+5�� ����)
            NextLevelXP = 13 + (Level - 3) * 5;
        }

        else if (Level >= 20 && Level < 40)
        {
            // ���� 20���� ���� 40���� �䱸 ����ġ ����
            // 143, 150, 157, 164, ... (+7�� ����)
            NextLevelXP = 143 + (Level - 20) * 7;
            //NextLevelXP = 3f;

        }
        else if (Level >= 40)
        {
            // ���� 40 �̻� �䱸 ����ġ ����
            // 336, 344, 352, 360, ... (+8�� ����)
            NextLevelXP = 336 + (Level - 40) * 8;
        }
        // ����ġ �� �ִ밪 ������Ʈ
        InitializeXpSlider();

    }


    public void ShowLevelUpPopup()
    {

        // ������ �˾��� �̹� Ȱ��ȭ�Ǿ� ������ ��ȯ
        if (isLevelUpPopupActive)
        {
            return;
        }
        isLevelUpPopupActive = true; // ������ �˾� Ȱ��ȭ
                                     // Ư�� ������ �� �Ϲ� ������ �˾��� ������ �ʵ��� Ȯ��
        if (specialLevelUpPanel.activeSelf)
        {
            return;
        }

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
        if ((Level - 1) % 20 == 0)  // Ư�� ������ ����
        {
            specialLevelUpPanel.SetActive(true); // Ư�� ������ �г� Ȱ��ȭ
            overlayPanel.SetActive(true);

            levelUpPopup.SetActive(false); // �Ϲ� ������ �г� ��Ȱ��ȭ

            for (int i = 0; i < specialUpgradeButtons.Length; i++)
            {
                if (i < selectedUpgrades.Count)
                {
                    GameObject buttonPrefab = Instantiate(selectedUpgrades[i].buttonPrefab, specialUpgradeButtons[i].transform.position, Quaternion.identity, specialUpgradeButtons[i].transform);
                    //buttonPrefab.transform.localPosition = Vector3.zero; // ��ġ ����
                    //buttonPrefab.transform.localRotation = Quaternion.identity; // ȸ�� ����
                    //buttonPrefab.transform.localScale = Vector3.one; // ũ�� ����
                    SetupButton(buttonPrefab, selectedUpgrades[i]); // ��ư �ʱ�ȭ

                    RectTransform rectTransform = buttonPrefab.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.sizeDelta = new Vector2(250, 375); // ���ϴ� ũ��� ����
                    }

                    // �ؽ�Ʈ ������Ʈ
                    TMP_Text buttonText = buttonPrefab.GetComponentInChildren<TMP_Text>();
                    if (buttonText != null)
                    {
                        buttonText.text = $"{selectedUpgrades[i].name} (+{selectedUpgrades[i].effect})";
                        buttonText.fontSize = 24; // ���ϴ� �ؽ�Ʈ ũ��� ����

                        // �ؽ�Ʈ�� RectTransform ����
                        RectTransform textRectTransform = buttonText.GetComponent<RectTransform>();
                        if (textRectTransform != null)
                        {
                            textRectTransform.sizeDelta = new Vector2(200, 100); // �ؽ�Ʈ ���� ũ�� ����
                            textRectTransform.anchoredPosition = Vector2.zero; // �ؽ�Ʈ ��ġ ����
                        }
                    }
                    else
                    {
                        Debug.LogError("TMP_Text component not found in the button prefab.");
                    }

                    // ��ư�� ������ �߰�
                    Button btn = buttonPrefab.GetComponent<Button>();
                    btn.onClick.RemoveAllListeners();
                    int captureIndex = i; // Ŭ������ ���� �ε��� ����
                    btn.onClick.AddListener(() => ApplyStatUpgrade(selectedUpgrades[captureIndex]));
                    buttonPrefab.SetActive(true);

                    // ī�� �ִϸ��̼� ���
                    CardAnimation cardAnim = buttonPrefab.GetComponent<CardAnimation>();
                    if (cardAnim != null)
                    {
                        cardAnim.card = buttonPrefab;
                        string animationName = GetAnimationName(selectedUpgrades[i].name);
                        StartCoroutine(PlayCardAnimation(cardAnim, animationName));
                    }
                }
                else
                {
                    specialUpgradeButtons[i].gameObject.SetActive(false);
                }
            }
        }
        else // �Ϲ� ������ ó��
        {
            // Ư�� ������ �Ŀ� �Ϲ� ������ �˾��� ������ �ʵ��� Ȯ��
            if (specialLevelUpPanel.activeSelf)
            {
                return;
            }

            overlayPanel.SetActive(true);

            levelUpPopup.SetActive(true); // �Ϲ� ������ �г� Ȱ��ȭ
            specialLevelUpPanel.SetActive(false); // Ư�� ������ �г� ��Ȱ��ȭ

            // ���õ� ���׷��̵忡 �ش��ϴ� ���ο� ��ư�� �����ϰ� ���� ����
            for (int i = 0; i < selectedUpgrades.Count; i++)
            {
                if (i < buttonPositions.Length)
                {
                    GameObject buttonObject = Instantiate(selectedUpgrades[i].buttonPrefab, buttonPositions[i].position, Quaternion.identity, buttonPositions[i]);
                    buttonObject.transform.localPosition = Vector3.zero; // ��ġ ����
                    buttonObject.transform.localRotation = Quaternion.identity; // ȸ�� ����
                    buttonObject.transform.localScale = Vector3.one; // ũ�� ����
                    SetupButton(buttonObject, selectedUpgrades[i]); // ��ư �ʱ�ȭ

                    RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.sizeDelta = new Vector2(200, 300); // ���ϴ� ũ��� ����
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
                    button.GetComponentInChildren<TMP_Text>().fontSize = 18; // ���ϴ� �ؽ�Ʈ ũ��� ����

                    // �ؽ�Ʈ�� RectTransform ����
                    RectTransform textRectTransform = button.GetComponentInChildren<TMP_Text>().GetComponent<RectTransform>();
                    if (textRectTransform != null)
                    {
                        textRectTransform.anchorMin = new Vector2(0.1f, 0.1f);
                        textRectTransform.anchorMax = new Vector2(0.9f, 0.3f);
                        textRectTransform.offsetMin = new Vector2(0, 0);
                        textRectTransform.offsetMax = new Vector2(0, 0);
                    }

                    // �̺�Ʈ ������ ����
                    button.onClick.RemoveAllListeners();
                    int captureIndex = i; // Ŭ������ ���� �ε��� ����
                    button.onClick.AddListener(() => ApplyStatUpgrade(selectedUpgrades[captureIndex]));
                }
            }
            overlayPanel.SetActive(true); // �������� �г� Ȱ��ȭ
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

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = $"{upgrade.name} (+{upgrade.effect})";
            buttonText.fontSize = 24; // ���ϴ� �ؽ�Ʈ ũ��� ����
                                      // �ؽ�Ʈ�� RectTransform ����
                                      // �ؽ�Ʈ�� RectTransform ����
                                      // �ؽ�Ʈ�� RectTransform ����
            RectTransform textRectTransform = buttonText.GetComponent<RectTransform>();
            if (textRectTransform != null)
            {
                textRectTransform.anchorMin = new Vector2(0.1f, 0.1f);
                textRectTransform.anchorMax = new Vector2(0.9f, 0.3f);
                textRectTransform.offsetMin = new Vector2(0, 0);
                textRectTransform.offsetMax = new Vector2(0, 0);
            }
        }
        else
        {
            Debug.LogError("TMP_Text component not found in the button object.");
        }

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
            currentLevel.text = "Lvl: \n" + Level;
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
        specialLevelUpPanel.SetActive(false); // Ư�� ������ �г� ��Ȱ��ȭ
        isLevelUpPopupActive = false; // ������ �˾� ��Ȱ��ȭ

        UpdateLevelDisplay(); // ���� ǥ�ø� ������Ʈ�մϴ�.
                              // ����ġ �� �ִ밪 ������Ʈ


        // ����ġ �� �ʱ�ȭ
        if (xpSlider != null)
        {
            barImage.fillAmount = 0f;
            Canvas.ForceUpdateCanvases(); // UI ���̾ƿ��� ������ �簻��
            Debug.Log("CloseLevelUpPopup: ����ġ �ٰ� �ʱ�ȭ�Ǿ����ϴ�."); // ����� �޽��� �߰�
        }



        if (!gameOverPanel.activeSelf)
        {
            ResumeGame(); // ���� �簳
        }
        UpdateLevelUpRequirement();

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
        UpdateLevelDisplay(); // ���� ǥ�ø� ������Ʈ�մϴ�.

    }

    public void IncrementMonsterKillCount()
    {
        totalMonstersKilled++;
    }


    public void GameOver()
    {
        // ���� �г� ��Ȱ��ȭ
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // �÷��̾��� ���� óġ ��, ������ ����, ���ʽ� ������ PlayerPrefs�� ����
        PlayerPrefs.SetInt("TotalMonstersKilled", totalMonstersKilled);
        PlayerPrefs.SetInt("LevelReached", Level);
        float bonusStats = Mathf.Floor(Level * 0.1f);
        PlayerPrefs.SetFloat("BonusStats", bonusStats);

        // PlayTime ����
        int playTime = (int)Time.timeSinceLevelLoad;
        PlayerPrefs.SetInt("PlayTime", playTime);

        // STATMANAGER�� ���� ������ ����
        StatManager.Instance.SaveStatsToPlayerPrefs();

        // GameOverUI �ʱ�ȭ �� Ȱ��ȭ
        GameOverUI gameOverUI = gameOverPanel.GetComponent<GameOverUI>();
        if (gameOverUI != null)
        {
            gameOverUI.Initialize();
            gameOverUI.ResetPanelSizeAndPosition(); // �г� ũ��� ��ġ�� ��������� ����
        }
        else
        {
            Debug.LogError("GameOverUI ������Ʈ�� ã�� �� �����ϴ�.");
        }

        // ���� ���� �г��� Ȱ��ȭ
        gameOverPanel.SetActive(true);

        // ��� ���� ���߱�
        PauseGame();

        // ��� ������ ��������Ʈ ��Ȱ��ȭ
        Monster[] monsters = FindObjectsOfType<Monster>();  // Monster�� ����
        foreach (Monster monster in monsters)
        {
            SpriteRenderer sr = monster.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = false;
            }
        }

        GameObject[] effects = GameObject.FindGameObjectsWithTag("Effect");
        foreach (GameObject effect in effects)
        {
            effect.SetActive(false);
        }

        // ��� �ʱ�ȭ
        ResetAppliedUpgrades();
    }
    public void ReturnToMainScene()
    {
        Level = 1;
        NextLevelXP = 2.5f;
        balInstance.totalExperience = 0f; // ����ġ �ʱ�ȭ

        StatManager.Instance.ResetUpgrades();
        StatManager.Instance.LoadStatsFromPlayerPrefs();

        Time.timeScale = 1f;
        SceneManager.LoadScene("Main/Main");

        // DontDestroyOnLoad ������Ʈ �ٽ� ����
        DontDestroyOnLoad(gameObject);

        gameOverPanel.SetActive(false);

        // ������ �гΰ� �������� �г� ��Ȱ��ȭ
        levelUpPopup.SetActive(false);
        overlayPanel.SetActive(false);
        specialLevelUpPanel.SetActive(false);

        isGamePaused = false; // ���� �Ͻ����� ���� ����
    }

    // ��� �ʱ�ȭ �޼��� �߰�
    private void ResetAppliedUpgrades()
    {
        if (balInstance != null)
        {
            balInstance.isDotActive = false;
            balInstance.isAoeActive = false;
            balInstance.isPdActive = false;
            balInstance.isTurretActive = false;
            balInstance.knockbackEnabled = false;
        }

        if (ballistaController != null)
        {
            ballistaController.SetLineRendererEnabled(false);
        }

        if (turretObject != null)
        {
            turretObject.SetActive(false);
        }
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

            case "���� ���� �ο�":
                return "UI_20_Dot";
            case "���� ���� �ο�":
                return "UI_20_Doa";
            case "����ü �� ����(�ߺ�)":
                return "UI_20_Arr_Plus";
            case "����ü ���� �ο�":
                return "UI_20_Pd";
            case "���� ��� ǥ��":
                return "UI_20_Direction";
            case "�ڵ� �ͷ� ����":
                return "UI_20_Tur_Spawn";
            case "����ü �˹� �ο�":
                return "UI_20_Knockback";
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

    public void ActivatePanel()
    {
        if (panelToActivate != null)
        {
            panelToActivate.SetActive(true); // ������ �г��� Ȱ��ȭ
            PauseGame(); // ���� �Ͻ� ����
        }
    }
    public void DeactivatePopup()
    {
        if (popupToDeactivate != null)
        {
            popupToDeactivate.SetActive(false); // ������ �˾��� ��Ȱ��ȭ
            ResumeGame(); // ���� �簳
        }
    }

}
