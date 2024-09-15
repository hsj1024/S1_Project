using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterSpawnData
{
    public GameObject prefab; // ���� ������
    public int spawnProbability; // ���� Ȯ��
}

[System.Serializable]
public class SpawnPeriod
{
    public List<MonsterSpawnData> monsters; // �� �ð��뿡 ������ ���� ���
    public float monsterSpawnInterval; // ���� ���� �ֱ�
    public TimeSpan startTime; // ���� �ֱ� ���� �ð�
    public TimeSpan endTime; // ���� �ֱ� �� �ð�
}

public class MonsterSpawnManager : MonoBehaviour
{
    public List<SpawnPeriod> spawnPeriods; // ��� �ð��뿡 ���� ���� ����
    public List<Transform> spawnPoints; // ������ ���� ��ġ ���
    public GameObject batNormalPrefab;
    public GameObject goblinNormalPrefab;
    public GameObject goblinShieldPrefab;
    public GameObject goblinArmorPrefab;
    public GameObject bearNormalPrefab;
    public GameObject treeNormalPrefab;
    public GameObject golemNormalPrefab;
    public GameObject trollNormalPrefab;
    public GameObject specialMonster1; // ����� ���� 1
    public GameObject specialMonster2; // ����� ���� 2
    private float gamePlayTimeInSeconds = 0f; // ���� �÷��� �ð��� �� ����
    public List<Monster> activeMonsters = new List<Monster>();
    private int currentSpawnPointIndex = 0;
    private int lastSpawnPointIndex = -1; // ������ ���� ���� ����Ʈ �ε���
    public GameObject bossMonsterPrefab;
    public GameObject bossClonePrefab; // ���� Ŭ��1 ������
    public GameObject bossClone2Prefab; // ���� Ŭ�� 2 ������
    public GameObject bossClone3Prefab; // ���� Ŭ�� 3 ������
    void Start()
    {
        // ���� ������ ����
        StartCoroutine(SpawnLogic());
        StartCoroutine(SpecialSpawnLogic());
        StartCoroutine(BossSpawnLogic()); // ���� ���� ���� �߰�

        if (bossMonsterPrefab == null)
        {
            Debug.LogError("Boss Monster Prefab is not assigned!");
        }

        // ���� ���� �������� ����Ϸ��� �������� null Ȯ��
        if (bossMonsterPrefab != null)
        {
            // ���� ���� �������� ����� �ڵ�
        }
        else
        {
            Debug.LogError("Boss Monster Prefab is missing or has been destroyed!");
        }
    }

    void Update()
    {
        // �� �����Ӹ��� ����� �ð�(��)�� �߰�
        gamePlayTimeInSeconds += Time.deltaTime;

        // 2������� ����
        if (Input.GetKeyDown(KeyCode.Alpha2)) // '2' Ű�� ������ ��
        {
            Time.timeScale = 2.0f;
        }

        // 5������� ����
        if (Input.GetKeyDown(KeyCode.Alpha5)) // '5' Ű�� ������ ��
        {
            Time.timeScale = 5.0f;
        }

        // �Ϲ� �ӵ�(1���)���� �缳��
        if (Input.GetKeyDown(KeyCode.Alpha1)) // '1' Ű�� ������ ��
        {
            Time.timeScale = 1.0f;
        }
    }

    IEnumerator SpawnLogic()
    {
        while (true)
        {
            float currentTime = gamePlayTimeInSeconds; // ���� ���� �÷��� �ð�
            SpawnPeriod currentPeriod = FindSpawnPeriod(currentTime);
            if (currentPeriod != null)
            {
                // 1�ʿ� ���� ���� ���͸� ����
                for (int i = 0; i < currentPeriod.monsterSpawnInterval; i++)
                {
                    SpawnMonster(currentPeriod);
                }
            }

            // 1�� ��� �� ���� �������� �̵�
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator SpecialSpawnLogic()
    {
        // 2:00���� ������ ����
        while (gamePlayTimeInSeconds < 120)
        {
            yield return null;
        }

        while (true)
        {
            // 20�ʸ��� ���͸� ����
            SpawnSpecialMonster();
            yield return new WaitForSeconds(20f);
        }
    }

    SpawnPeriod FindSpawnPeriod(float gamePlayTime)
    {
        foreach (var period in spawnPeriods)
        {
            // TimeSpan ��� ���� �÷��� �ð�(��)�� �������� ��
            float startTimeInSeconds = (float)period.startTime.TotalSeconds;
            float endTimeInSeconds = (float)period.endTime.TotalSeconds;

            if (gamePlayTime >= startTimeInSeconds && gamePlayTime < endTimeInSeconds)
            {
                return period;
            }
        }
        return null;
    }

    void SpawnMonster(SpawnPeriod period)
    {
        // �ð��뿡 ���� ���� �� �ϳ��� �������� ����
        var monsterData = period.monsters[UnityEngine.Random.Range(0, period.monsters.Count)];
        int probabilityRoll = UnityEngine.Random.Range(0, 100);
        if (probabilityRoll <= monsterData.spawnProbability)
        {
            int randomSpawnPointIndex;
            Transform spawnPoint;

            // ������ ���� ����Ʈ �ε����� �����ϰ� ���� ����
            do
            {
                randomSpawnPointIndex = UnityEngine.Random.Range(1, spawnPoints.Count - 1);
            } while (randomSpawnPointIndex == lastSpawnPointIndex);

            spawnPoint = spawnPoints[randomSpawnPointIndex];
            lastSpawnPointIndex = randomSpawnPointIndex; // ������ ���� ����Ʈ �ε��� ������Ʈ

            GameObject prefabToSpawn = monsterData.prefab;
            GameObject spawnedMonster = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
            //Debug.Log("Spawned " + spawnedMonster.name + " at " + spawnPoint.position);

            // Bal �ν��Ͻ� ��������
            Bal balInstance = FindObjectOfType<Bal>();
        }
    }

    void SpawnSpecialMonster()
    {
        if (spawnPoints.Count < 2) return; // ���� ����Ʈ�� 2�� �̸��� ��� ����

        // 1���� 8�� �������� �ε���
        int[] specialSpawnIndices = { 0, spawnPoints.Count - 1 };
        int selectedSpawnIndex = specialSpawnIndices[UnityEngine.Random.Range(0, specialSpawnIndices.Length)];
        Transform spawnPoint = spawnPoints[selectedSpawnIndex];

        GameObject[] specialPrefabs = { specialMonster1, specialMonster2 }; // ���ϴ� ���������� ��ü�ϼ���
        GameObject prefabToSpawn = specialPrefabs[UnityEngine.Random.Range(0, specialPrefabs.Length)];

        GameObject spawnedMonster = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
        //Debug.Log("Special Spawned " + spawnedMonster.name + " at " + spawnPoint.position);

        // Bal �ν��Ͻ� ��������
        Bal balInstance = FindObjectOfType<Bal>();
    }
    /*
        void InitializeSpawnPeriods()
        {
            spawnPeriods = new List<SpawnPeriod>
            {
                // 00:00 - 00:59
                new SpawnPeriod
                {
                    monsters = new List<MonsterSpawnData>
                    {
                        new MonsterSpawnData { prefab = batNormalPrefab, spawnProbability = 60 },
                        new MonsterSpawnData { prefab = goblinNormalPrefab, spawnProbability = 40 }
                    },
                    monsterSpawnInterval = 0.7f, // 1�ʿ� n���� ���Ͱ� ����
                    startTime = TimeSpan.FromSeconds(0),
                    endTime = TimeSpan.FromSeconds(59)
                },
                // 01:00 - 01:59
                new SpawnPeriod
                {
                    monsters = new List<MonsterSpawnData>
                    {
                        new MonsterSpawnData { prefab = batNormalPrefab, spawnProbability = 20 },
                        new MonsterSpawnData { prefab = goblinNormalPrefab, spawnProbability = 20 },
                        new MonsterSpawnData { prefab = goblinArmorPrefab, spawnProbability = 60 }
                    },
                    monsterSpawnInterval = 0.7f,
                    startTime = TimeSpan.FromSeconds(60),
                    endTime = TimeSpan.FromSeconds(119)
                },
                // 02:00 - 04:59
                new SpawnPeriod
                {
                    monsters = new List<MonsterSpawnData>
                    {
                        new MonsterSpawnData { prefab = goblinArmorPrefab, spawnProbability = 40 },
                        new MonsterSpawnData { prefab = bearNormalPrefab, spawnProbability = 10 },
                        new MonsterSpawnData { prefab = batNormalPrefab, spawnProbability = 10 },
                        new MonsterSpawnData { prefab = treeNormalPrefab, spawnProbability = 40 }
                    },
                    monsterSpawnInterval = 1.0f,
                    startTime = TimeSpan.FromSeconds(120),
                    endTime = TimeSpan.FromSeconds(299)
                },
                // 05:00 - 07:59
                new SpawnPeriod
                {
                    monsters = new List<MonsterSpawnData>
                    {
                        new MonsterSpawnData { prefab = goblinShieldPrefab, spawnProbability = 20 },
                        new MonsterSpawnData { prefab = bearNormalPrefab, spawnProbability = 60 },
                        new MonsterSpawnData { prefab = batNormalPrefab, spawnProbability = 10 },
                        new MonsterSpawnData { prefab = treeNormalPrefab, spawnProbability = 10 }
                    },
                    monsterSpawnInterval = 1.0f,
                    startTime = TimeSpan.FromSeconds(300),
                    endTime = TimeSpan.FromSeconds(479)
                },
                // 08:00 - 09:59
                new SpawnPeriod
                {
                    monsters = new List<MonsterSpawnData>
                    {
                        new MonsterSpawnData { prefab = goblinShieldPrefab, spawnProbability = 30 },
                        new MonsterSpawnData { prefab = bearNormalPrefab, spawnProbability = 30 },
                        new MonsterSpawnData { prefab = golemNormalPrefab, spawnProbability = 10 },
                        new MonsterSpawnData { prefab = treeNormalPrefab, spawnProbability = 20 },
                        new MonsterSpawnData { prefab = trollNormalPrefab, spawnProbability = 10 }
                    },
                    monsterSpawnInterval = 1.0f,
                    startTime = TimeSpan.FromSeconds(480),
                    endTime = TimeSpan.FromSeconds(599)
                },
                // 10:00 - 12:00
                new SpawnPeriod
                {
                    monsters = new List<MonsterSpawnData>
                    {
                        new MonsterSpawnData { prefab = treeNormalPrefab, spawnProbability = 20 },
                        new MonsterSpawnData { prefab = golemNormalPrefab, spawnProbability = 40 },
                        new MonsterSpawnData { prefab = trollNormalPrefab, spawnProbability = 40 }
                    },
                    monsterSpawnInterval = 1.0f,
                    startTime = TimeSpan.FromSeconds(600),
                    endTime = TimeSpan.FromSeconds(720)
                },
            };
        }

        void Awake()
        {
            InitializeSpawnPeriods();
        }*/

    IEnumerator BossSpawnLogic()
    {
        // 720�� ���� ���
        yield return new WaitForSeconds(5f);

        // ������ BGM ���
        AudioManager.Instance.StartBossBattle();

        // 4�� ���� ����Ʈ ��������
        if (spawnPoints.Count > 4)
        {
            Transform spawnPoint = spawnPoints[4];

            if (bossMonsterPrefab != null)
            {
                GameObject bossClone = Instantiate(bossMonsterPrefab, spawnPoint.position, Quaternion.identity);

                // ���� Ŭ�� ���� ���� �ʿ��� �߰� ����
            }
            else
            {
                Debug.LogError("bossMonsterPrefab is null. Please assign the prefab in the inspector.");
            }
        }
        else
        {
            Debug.LogError("Insufficient spawn points available. Ensure that there are enough spawn points in the list.");
        }
    }

    

    public void SpawnBossClone1()
    {
        if (spawnPoints.Count < 2) return; // ���� ����Ʈ�� 2�� �̸��̸� ����

        int[] specialSpawnIndices = { 0, spawnPoints.Count - 1 };
        int selectedSpawnIndex = specialSpawnIndices[UnityEngine.Random.Range(0, specialSpawnIndices.Length)];
        Transform spawnPoint = spawnPoints[selectedSpawnIndex];

        if (bossClonePrefab != null)
        {
            // Instantiate the boss clone prefab
            GameObject bossClone = Instantiate(bossClonePrefab, spawnPoint.position, Quaternion.identity);

            // �߰� ����� �α�
            if (bossClone != null)
            {
                Debug.Log("Boss Clone 1 instantiated at: " + bossClone.transform.position);
                Debug.Log("Boss Clone 1 active status: " + bossClone.activeSelf);

                // Try to get the BossClone1 script component
                BossClone1 cloneScript = bossClone.GetComponent<BossClone1>();
                if (cloneScript != null)
                {
                    cloneScript.SetBoss(this.GetComponent<BossMonster>());
                    Debug.Log("Boss Clone 1 has been successfully initialized.");
                }
                else
                {
                    Debug.LogError("Failed to find BossClone1 script on instantiated prefab.");
                }
            }
            else
            {
                Debug.LogError("Boss Clone 1 was not instantiated correctly.");
            }
        }
        else
        {
            Debug.LogError("Boss Clone 1 Prefab is null!");
        }
    }



    public void SpawnBossClones2()
    {
        int[] spawnIndices = { 1, 3, 5 };

        for (int i = 0; i < spawnIndices.Length; i++)
        {
            Transform spawnPoint = spawnPoints[spawnIndices[i]];

            if (bossClone2Prefab != null)
            {
                GameObject bossClone2 = Instantiate(bossClone2Prefab, spawnPoint.position, Quaternion.identity);

                // ���� Ŭ��2 �ʱ�ȭ ���� �߰�
                BossClone2 cloneScript = bossClone2.GetComponent<BossClone2>();
                if (cloneScript != null)
                {
                    cloneScript.SetBoss(this.GetComponent<BossMonster>());
                }
            }
            else
            {
                Debug.LogError("Boss Clone 2 Prefab is not assigned!");
            }
        }
    }

    public void SpawnBossClones3()
    {
        int[] spawnIndices = { 1, 3, 5 };

        for (int i = 0; i < spawnIndices.Length; i++)
        {
            Transform spawnPoint = spawnPoints[spawnIndices[i]];

            if (bossClone3Prefab != null)
            {
                GameObject bossClone3 = Instantiate(bossClone3Prefab, spawnPoint.position, Quaternion.identity);

                // ���� Ŭ��3 �ʱ�ȭ ���� �߰�
                BossClone3 cloneScript = bossClone3.GetComponent<BossClone3>();
                if (cloneScript != null)
                {
                    cloneScript.SetBoss(this.GetComponent<BossMonster>());
                }
            }
            else
            {
                Debug.LogError("Boss Clone 3 Prefab is not assigned!");
            }
        }
    }
}
