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
    public int monsterSpawnInterval; // ���� ���� �ֱ� (�� ����)
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
    public GameObject wolfNormalPrefab1;
    public GameObject wolfNormalPrefab2;
    public GameObject treeNormalPrefab;
    public GameObject golemNormalPrefab;
    public GameObject trollNormalPrefab;
    private float gamePlayTimeInSeconds = 0f; // ���� �÷��� �ð��� �� ����
    public List<Monster> activeMonsters = new List<Monster>();
    private int currentSpawnPointIndex = 0;

    void Start()
    {
        // ���� ������ ����
        StartCoroutine(SpawnLogic());
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
                // �ʴ� �����Ǿ�� �ϴ� ���� ���� ���� ���͸� ����
                int monstersToSpawnThisSecond = currentPeriod.monsterSpawnInterval; // ���� 'monsterSpawnInterval'�� �ʴ� ���� ���� ���� �ǹ�

                for (int i = 0; i < monstersToSpawnThisSecond; i++)
                {
                    SpawnMonster(currentPeriod);
                }

                yield return new WaitForSeconds(1f); // 1�� ��� �� ���� �������� �̵�
            }
            else
            {
                yield return new WaitForSeconds(1f); // ������ ���� �ֱ⸦ ã�� ���� ��� 1�� ���
            }
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
            // ���⿡�� spawnPoints ����Ʈ���� ���� ��ġ�� �����մϴ�.
            int randomSpawnPointIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
            Transform spawnPoint = spawnPoints[randomSpawnPointIndex]; // �� ������ �����Ǿ��� �� �ֽ��ϴ�.

            GameObject prefabToSpawn; // �� ���� ������ �����Ǿ��� �� �ֽ��ϴ�.

            // ���� ������ ���, �����ϰ� ������ �ϳ��� ����
            if (monsterData.prefab.name == "Wolf_Normal")
            {
                prefabToSpawn = ChooseRandomWolfPrefab();
            }
            else
            {
                prefabToSpawn = monsterData.prefab;
            }

            // ������ ������ �ν��Ͻ��� �����մϴ�.
            GameObject spawnedMonster = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity); // �� ���� ������ �ߺ��Ǿ��� �� �ֽ��ϴ�.
            //Debug.Log("Spawned " + spawnedMonster.name + " at " + spawnPoint.position);
        }
    }



    // �� ������ �� �����ϰ� �ϳ��� �����ϴ� �޼���
    GameObject ChooseRandomWolfPrefab()
    {
        if (UnityEngine.Random.Range(0, 2) == 0) // 0 �Ǵ� 1�� �����ϰ� ��ȯ
        {
            return wolfNormalPrefab1;
        }
        else
        {
            return wolfNormalPrefab2;
        }
    }



    void InitializeSpawnPeriods()
    {
        spawnPeriods = new List<SpawnPeriod>
        {
            // 00:00 - 01:59
            new SpawnPeriod
            {
                monsters = new List<MonsterSpawnData>
                {
                    new MonsterSpawnData { prefab = batNormalPrefab, spawnProbability = 60 },
                    new MonsterSpawnData { prefab = goblinNormalPrefab, spawnProbability = 40 }
                },
                monsterSpawnInterval = 1, // 1�ʿ� 1���� ���Ͱ� ����
                startTime = TimeSpan.FromSeconds(0),
                endTime = TimeSpan.FromSeconds(119) 
            },
        // 02:00 - 04:59
        new SpawnPeriod
        {
            monsters = new List<MonsterSpawnData>
            {
                new MonsterSpawnData { prefab = batNormalPrefab, spawnProbability = 20 },
                new MonsterSpawnData { prefab = goblinNormalPrefab, spawnProbability = 20 },
                new MonsterSpawnData { prefab = goblinArmorPrefab, spawnProbability = 60 }
            },
            monsterSpawnInterval = 3,
            startTime = TimeSpan.FromSeconds(120),
            endTime = TimeSpan.FromSeconds(299)
        },
        // 05:00 - 08:59
        new SpawnPeriod
        {
            monsters = new List<MonsterSpawnData>
            {
                new MonsterSpawnData { prefab = goblinArmorPrefab, spawnProbability = 50 },
                new MonsterSpawnData { prefab = bearNormalPrefab, spawnProbability = 40 },
                new MonsterSpawnData { prefab = wolfNormalPrefab1, spawnProbability = 10 }
            },
            monsterSpawnInterval = 4,
            startTime = TimeSpan.FromSeconds(300),
            endTime = TimeSpan.FromSeconds(539)
        },
        //9:00 - 13:59
       new SpawnPeriod
        {
            monsters = new List<MonsterSpawnData>
            {
                new MonsterSpawnData { prefab = goblinShieldPrefab, spawnProbability = 20 },
                new MonsterSpawnData { prefab = bearNormalPrefab, spawnProbability = 60 },
                new MonsterSpawnData { prefab = wolfNormalPrefab1, spawnProbability = 10 },
                new MonsterSpawnData { prefab = treeNormalPrefab, spawnProbability = 10 }
            },
            monsterSpawnInterval = 5,
            startTime = TimeSpan.FromSeconds(540),
            endTime = TimeSpan.FromSeconds(839)
        },
       //14:00 - 17:59
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
            monsterSpawnInterval = 6,
            startTime = TimeSpan.FromSeconds(840),
            endTime = TimeSpan.FromSeconds(1079)
        },
        // 18:00 - 19:59
        new SpawnPeriod
        {
            monsters = new List<MonsterSpawnData>
            {
                new MonsterSpawnData { prefab = treeNormalPrefab, spawnProbability = 20 },
                new MonsterSpawnData { prefab = golemNormalPrefab, spawnProbability = 40 },
                new MonsterSpawnData { prefab = trollNormalPrefab, spawnProbability = 40 }
            },
            monsterSpawnInterval = 5,
            startTime = TimeSpan.FromSeconds(1080),
            endTime = TimeSpan.FromSeconds(1199)
        },
    };
    }

    void Awake()
    {
        InitializeSpawnPeriods();
    }
}
