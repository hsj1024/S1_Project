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
    public GameObject treeNormalPrefab;
    public GameObject golemNormalPrefab;
    public GameObject trollNormalPrefab;
    public GameObject specialMonster1; // ����� ���� 1
    public GameObject specialMonster2; // ����� ���� 2
    private float gamePlayTimeInSeconds = 0f; // ���� �÷��� �ð��� �� ����
    public List<Monster> activeMonsters = new List<Monster>();
    private int currentSpawnPointIndex = 0;

    void Start()
    {
        // ���� ������ ����
        StartCoroutine(SpawnLogic());
        StartCoroutine(SpecialSpawnLogic());
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
                // Ư�� �ʸ��� 1���� ���͸� ����
                SpawnMonster(currentPeriod);

                // ������ ���ݿ� ���� ��� �� ���� �������� �̵�
                yield return new WaitForSeconds(currentPeriod.monsterSpawnInterval);
            }
            else
            {
                yield return new WaitForSeconds(1f); // ������ ���� �ֱ⸦ ã�� ���� ��� 1�� ���
            }
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

            // 1���� 8�� �����ʸ� �����ϰ� ���� ����
            do
            {
                randomSpawnPointIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
                spawnPoint = spawnPoints[randomSpawnPointIndex];
            } while (randomSpawnPointIndex == 0 || randomSpawnPointIndex == spawnPoints.Count - 1);

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
                monsterSpawnInterval = 2, // n�ʿ� 1���� ���Ͱ� ����
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
                monsterSpawnInterval = 2,
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
                monsterSpawnInterval = 2,
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
                monsterSpawnInterval = 2,
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
                monsterSpawnInterval = 2,
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
                monsterSpawnInterval = 2,
                startTime = TimeSpan.FromSeconds(600),
                endTime = TimeSpan.FromSeconds(720)
            },
        };
    }

    void Awake()
    {
        InitializeSpawnPeriods();
    }
}
