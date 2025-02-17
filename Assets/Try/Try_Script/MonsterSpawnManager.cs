using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterSpawnData
{
    public GameObject prefab; // 몬스터 프리팹
    public int spawnProbability; // 스폰 확률
}

[System.Serializable]
public class SpawnPeriod
{
    public List<MonsterSpawnData> monsters; // 이 시간대에 스폰할 몬스터 목록
    public float monsterSpawnInterval; // 몬스터 등장 주기
    public TimeSpan startTime; // 스폰 주기 시작 시간
    public TimeSpan endTime; // 스폰 주기 끝 시간
}

public class MonsterSpawnManager : MonoBehaviour
{
    public List<SpawnPeriod> spawnPeriods; // 모든 시간대에 대한 스폰 설정
    public List<Transform> spawnPoints; // 가능한 스폰 위치 목록
    public GameObject batNormalPrefab;
    public GameObject goblinNormalPrefab;
    public GameObject goblinShieldPrefab;
    public GameObject goblinArmorPrefab;
    public GameObject bearNormalPrefab;
    public GameObject treeNormalPrefab;
    public GameObject golemNormalPrefab;
    public GameObject trollNormalPrefab;
    public GameObject specialMonster1; // 스페셜 몬스터 1
    public GameObject specialMonster2; // 스페셜 몬스터 2
    private float gamePlayTimeInSeconds = 0f; // 게임 플레이 시간을 초 단위
    public List<Monster> activeMonsters = new List<Monster>();
    private int currentSpawnPointIndex = 0;
    private int lastSpawnPointIndex = -1; // 마지막 사용된 스폰 포인트 인덱스
    public GameObject bossMonsterPrefab;
    public BossMonster bossMonsterInstance; // 현재 보스 몬스터 인스턴스 참조
    public GameObject bossClone1Prefab; // 보스 클론1 프리팹
    public GameObject bossClone2Prefab; // 보스 클론 2 프리팹
    public GameObject bossClone3Prefab; // 보스 클론 3 프리팹

    private bool bossClone1Spawned = false;
    private bool bossClone2Spawned = false;
    private bool bossClone3Spawned = false;

    private int bossClone2Count = 0;
    private int bossClone3Count = 0;

    void Start()
    {
        // 스폰 로직을 시작
        StartCoroutine(SpawnLogic());
        StartCoroutine(SpecialSpawnLogic());
        StartCoroutine(BossSpawnLogic()); // 보스 스폰 로직 추가

        if (bossMonsterPrefab == null)
        {
            Debug.LogError("Boss Monster Prefab is not assigned!");
        }

        // 보스 몬스터 프리팹을 사용하려는 시점에서 null 확인
        if (bossMonsterPrefab != null)
        {
            // 보스 몬스터 프리팹을 사용할 코드
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
        // 매 프레임마다 경과한 시간(초)을 추가
        gamePlayTimeInSeconds += Time.deltaTime;

    }

    IEnumerator SpawnLogic()
    {
        while (true)
        {
            float currentTime = gamePlayTimeInSeconds; // 현재 게임 플레이 시간
            SpawnPeriod currentPeriod = FindSpawnPeriod(currentTime);
            if (currentPeriod != null)
            {
                // 1초에 여러 마리 몬스터를 스폰
                for (int i = 0; i < currentPeriod.monsterSpawnInterval; i++)
                {
                    SpawnMonster(currentPeriod);
                }
            }

            // 1초 대기 후 다음 스폰으로 이동
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator SpecialSpawnLogic()
    {
        // 2:00부터 스폰을 시작
        while (gamePlayTimeInSeconds < 120)
        {
            yield return null;
        }

        while (true)
        {
            // 20초마다 몬스터를 스폰
            SpawnSpecialMonster();
            yield return new WaitForSeconds(20f);
        }
    }

    SpawnPeriod FindSpawnPeriod(float gamePlayTime)
    {
        foreach (var period in spawnPeriods)
        {
            // TimeSpan 대신 게임 플레이 시간(초)을 기준으로 비교
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
        // 시간대에 따른 몬스터 중 하나를 무작위로 선택
        var monsterData = period.monsters[UnityEngine.Random.Range(0, period.monsters.Count)];
        int probabilityRoll = UnityEngine.Random.Range(0, 100);
        if (probabilityRoll <= monsterData.spawnProbability)
        {
            int randomSpawnPointIndex;
            Transform spawnPoint;

            // 마지막 스폰 포인트 인덱스를 제외하고 랜덤 스폰
            do
            {
                randomSpawnPointIndex = UnityEngine.Random.Range(1, spawnPoints.Count - 1);
            } while (randomSpawnPointIndex == lastSpawnPointIndex);

            spawnPoint = spawnPoints[randomSpawnPointIndex];
            lastSpawnPointIndex = randomSpawnPointIndex; // 마지막 스폰 포인트 인덱스 업데이트

            GameObject prefabToSpawn = monsterData.prefab;
            GameObject spawnedMonster = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
            //Debug.Log("Spawned " + spawnedMonster.name + " at " + spawnPoint.position);

            // Bal 인스턴스 가져오기
            Bal balInstance = FindObjectOfType<Bal>();
        }
    }

    void SpawnSpecialMonster()
    {
        if (spawnPoints.Count < 9) return; // 스폰 포인트가 9개 미만인 경우 리턴

        int[] specialSpawnIndices = { 0, 8 };
        int selectedSpawnIndex = specialSpawnIndices[UnityEngine.Random.Range(0, specialSpawnIndices.Length)];
        Transform spawnPoint = spawnPoints[selectedSpawnIndex];

        GameObject[] specialPrefabs = { specialMonster1, specialMonster2 };
        GameObject prefabToSpawn = specialPrefabs[UnityEngine.Random.Range(0, specialPrefabs.Length)];

        GameObject spawnedMonster = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);

        // 스페셜 몬스터 플래그 설정
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
                       monsterSpawnInterval = 0.8f, // 1초에 n마리 몬스터가 스폰
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
        // 720초 동안 대기
        yield return new WaitForSeconds(720f);

        // 4번 스폰 포인트 가져오기
        if (spawnPoints.Count > 4 && bossMonsterPrefab != null)
        {
            Transform spawnPoint = spawnPoints[4];

            // 하이라키에서 비활성화된 보스 프리팹을 활성화하며 스폰
            bossMonsterPrefab.SetActive(true); // 비활성화된 상태에서 활성화

            GameObject bossMonster = Instantiate(bossMonsterPrefab, spawnPoint.position, Quaternion.identity);
            bossMonsterInstance = bossMonster.GetComponent<BossMonster>();

            if (bossMonsterInstance != null)
            {
                Debug.Log("Boss Monster 스폰 성공");
                // 보스 전투 BGM 시작
                AudioManager.Instance.StartBossBattle();
                
            }
            else
            {
                Debug.LogError("BossMonster 컴포넌트를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogError("BossMonsterPrefab 또는 spawnPoints가 설정되지 않았습니다.");
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
                bossClone1.SetActive(true); // 클론 활성화

                BossClone1 cloneScript = bossClone1.GetComponent<BossClone1>();
                if (cloneScript != null)
                {
                    cloneScript.SetBoss(bossMonsterInstance); // 보스 참조 설정
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
        if (bossClone2Spawned) return; // 이미 보스 클론2가 스폰되었으면 리턴

        int[] spawnIndices = { 2, 4, 6 }; // 스폰 포인트 설정
        bossClone2Count = spawnIndices.Length; // 스폰된 클론 수로 카운트 설정

        foreach (int index in spawnIndices)
        {
            if (index < spawnPoints.Count)
            {
                Transform spawnPoint = spawnPoints[index];

                // 하이라키에서 비활성화된 보스클론2 프리팹을 활성화하며 스폰
                bossClone2Prefab.SetActive(true); // 비활성화된 상태에서 활성화

                GameObject bossClone2 = Instantiate(bossClone2Prefab, spawnPoint.position, Quaternion.identity);
                BossClone2 cloneScript = bossClone2.GetComponent<BossClone2>();
                if (cloneScript != null && bossMonsterInstance != null)
                {
                    cloneScript.SetBoss(bossMonsterInstance); // 보스 참조 전달
                }
            }
        }

        // BossMonster에게 클론 수 전달
        if (bossMonsterInstance != null)
        {
            bossMonsterInstance.SetBossClone2Count(bossClone2Count);
        }

        bossClone2Spawned = true; // 보스 클론2 스폰 완료
    }


    public void SpawnBossClones3()
    {
        if (bossClone3Spawned) return; // 이미 보스 클론3이 스폰되었으면 리턴

        int[] spawnIndices = { 2 }; // 스폰 포인트 설정
        bossClone3Count = spawnIndices.Length; // 스폰된 클론 수로 카운트 설정

        foreach (int index in spawnIndices)
        {
            if (index < spawnPoints.Count)
            {
                Transform spawnPoint = spawnPoints[index];

                // 하이라키에서 비활성화된 보스클론3 프리팹을 활성화하며 스폰
                bossClone3Prefab.SetActive(true); // 비활성화된 상태에서 활성화

                GameObject bossClone3 = Instantiate(bossClone3Prefab, spawnPoint.position, Quaternion.identity);
                BossClone3 cloneScript = bossClone3.GetComponent<BossClone3>();
                if (cloneScript != null && bossMonsterInstance != null)
                {
                    cloneScript.SetBoss(bossMonsterInstance); // 보스 참조 전달
                }
            }
        }   

        // BossMonster에게 클론 수 전달
        if (bossMonsterInstance != null)
        {
            bossMonsterInstance.SetBossClone3Count(bossClone3Count);
        }

        bossClone3Spawned = true; // 보스 클론3 스폰 완료
    }
    


}
