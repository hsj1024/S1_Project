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
    public BossMonster bossMonsterInstance; // ���� ���� ���� �ν��Ͻ� ����
    public GameObject bossClone1Prefab; // ���� Ŭ��1 ������
    public GameObject bossClone2Prefab; // ���� Ŭ�� 2 ������
    public GameObject bossClone3Prefab; // ���� Ŭ�� 3 ������

    private bool bossClone1Spawned = false;
    private bool bossClone2Spawned = false;
    private bool bossClone3Spawned = false;

    private int bossClone2Count = 0;
    private int bossClone3Count = 0;

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

        bossClone2Count = 0;
        bossClone3Count = 0;
    }

    void Update()
    {
        // �� �����Ӹ��� ����� �ð�(��)�� �߰�
        gamePlayTimeInSeconds += Time.deltaTime;

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
        if (spawnPoints.Count < 9) return; // ���� ����Ʈ�� 9�� �̸��� ��� ����

        int[] specialSpawnIndices = { 0, 8 };
        int selectedSpawnIndex = specialSpawnIndices[UnityEngine.Random.Range(0, specialSpawnIndices.Length)];
        Transform spawnPoint = spawnPoints[selectedSpawnIndex];

        GameObject[] specialPrefabs = { specialMonster1, specialMonster2 };
        GameObject prefabToSpawn = specialPrefabs[UnityEngine.Random.Range(0, specialPrefabs.Length)];

        GameObject spawnedMonster = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);

        // ����� ���� �÷��� ����
        Monster specialMonster = spawnedMonster.GetComponent<Monster>();
        if (specialMonster != null)
        {
            specialMonster.isSpecialMonster = true;
        }
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
                       monsterSpawnInterval = 0.8f, // 1�ʿ� n���� ���Ͱ� ����
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
                       monsterSpawnInterval = 0.8f,
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
                       monsterSpawnInterval = 0.9f,
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
                       monsterSpawnInterval = 1f,
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
                       monsterSpawnInterval = 0.9f,
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
                       monsterSpawnInterval = 0.9f,
                       startTime = TimeSpan.FromSeconds(600),
                       endTime = TimeSpan.FromSeconds(720)
                   },
               };
           }
    
         
           void Awake()
           {
               InitializeSpawnPeriods();
           }

   
    IEnumerator BossSpawnLogic()
    {
        // 720�� ���� ���
        yield return new WaitForSeconds(720f);

        // 4�� ���� ����Ʈ ��������
        if (spawnPoints.Count > 4 && bossMonsterPrefab != null)
        {
            Transform spawnPoint = spawnPoints[4];

            // ���̶�Ű���� ��Ȱ��ȭ�� ���� �������� Ȱ��ȭ�ϸ� ����
            bossMonsterPrefab.SetActive(true); // ��Ȱ��ȭ�� ���¿��� Ȱ��ȭ

            GameObject bossMonster = Instantiate(bossMonsterPrefab, spawnPoint.position, Quaternion.identity);
            bossMonsterInstance = bossMonster.GetComponent<BossMonster>();

            if (bossMonsterInstance != null)
            {
                Debug.Log("Boss Monster ���� ����");
                // ���� ���� BGM ����
                AudioManager.Instance.StartBossBattle();
                
            }
            else
            {
                Debug.LogError("BossMonster ������Ʈ�� ã�� �� �����ϴ�!");
            }
        }
        else
        {
            Debug.LogError("BossMonsterPrefab �Ǵ� spawnPoints�� �������� �ʾҽ��ϴ�.");
        }
    }



    public void SpawnBossClone1()
    {
        if (bossClone1Spawned) return;
        if (spawnPoints.Count < 2) return;

        int[] specialSpawnIndices = { 0, spawnPoints.Count - 1 };
        int selectedSpawnIndex = specialSpawnIndices[UnityEngine.Random.Range(0, specialSpawnIndices.Length)];
        Transform spawnPoint = spawnPoints[selectedSpawnIndex];

        if (bossClone1Prefab != null)
        {
            GameObject bossClone1 = Instantiate(bossClone1Prefab, spawnPoint.position, Quaternion.identity);

            if (bossClone1 != null)
            {
                bossClone1.SetActive(true); // Ŭ�� Ȱ��ȭ

                BossClone1 cloneScript = bossClone1.GetComponent<BossClone1>();
                if (cloneScript != null)
                {
                    cloneScript.SetBoss(bossMonsterInstance); // ���� ���� ����
                }

                bossClone1Spawned = true;
            }
        }
        else
        {
            Debug.LogError("Boss Clone 1 Prefab is not assigned!");
        }
    }


    public void SpawnBossClones2()
    {
        if (bossClone2Spawned) return; // �̹� ���� Ŭ��2�� �����Ǿ����� ����

        int[] spawnIndices = { 2, 4, 6 }; // ���� ����Ʈ ����
        bossClone2Count = spawnIndices.Length; // ������ Ŭ�� ���� ī��Ʈ ����

        foreach (int index in spawnIndices)
        {
            if (index < spawnPoints.Count)
            {
                Transform spawnPoint = spawnPoints[index];

                // ���̶�Ű���� ��Ȱ��ȭ�� ����Ŭ��2 �������� Ȱ��ȭ�ϸ� ����
                bossClone2Prefab.SetActive(true); // ��Ȱ��ȭ�� ���¿��� Ȱ��ȭ

                GameObject bossClone2 = Instantiate(bossClone2Prefab, spawnPoint.position, Quaternion.identity);
                BossClone2 cloneScript = bossClone2.GetComponent<BossClone2>();
                if (cloneScript != null && bossMonsterInstance != null)
                {
                    cloneScript.SetBoss(bossMonsterInstance); // ���� ���� ����
                }
            }
        }

        // BossMonster���� Ŭ�� �� ����
        if (bossMonsterInstance != null)
        {
            bossMonsterInstance.SetBossClone2Count(bossClone2Count);
        }

        bossClone2Spawned = true; // ���� Ŭ��2 ���� �Ϸ�
    }


    public void SpawnBossClones3()
    {
        if (bossClone3Spawned) return; // �̹� ���� Ŭ��3�� �����Ǿ����� ����

        int[] spawnIndices = { 2 }; // ���� ����Ʈ ����
        bossClone3Count = spawnIndices.Length; // ������ Ŭ�� ���� ī��Ʈ ����

        foreach (int index in spawnIndices)
        {
            if (index < spawnPoints.Count)
            {
                Transform spawnPoint = spawnPoints[index];

                // ���̶�Ű���� ��Ȱ��ȭ�� ����Ŭ��3 �������� Ȱ��ȭ�ϸ� ����
                bossClone3Prefab.SetActive(true); // ��Ȱ��ȭ�� ���¿��� Ȱ��ȭ

                GameObject bossClone3 = Instantiate(bossClone3Prefab, spawnPoint.position, Quaternion.identity);
                BossClone3 cloneScript = bossClone3.GetComponent<BossClone3>();
                if (cloneScript != null && bossMonsterInstance != null)
                {
                    cloneScript.SetBoss(bossMonsterInstance); // ���� ���� ����
                }
            }
        }   

        // BossMonster���� Ŭ�� �� ����
        if (bossMonsterInstance != null)
        {
            bossMonsterInstance.SetBossClone3Count(bossClone3Count);
        }

        bossClone3Spawned = true; // ���� Ŭ��3 ���� �Ϸ�
    }
    


}
