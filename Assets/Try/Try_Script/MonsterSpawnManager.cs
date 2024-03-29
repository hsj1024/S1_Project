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
    public int monsterSpawnInterval; // 몬스터 등장 주기 (초 단위)
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
    public GameObject wolfNormalPrefab1;
    public GameObject wolfNormalPrefab2;
    public GameObject treeNormalPrefab;
    public GameObject golemNormalPrefab;
    public GameObject trollNormalPrefab;
    private float gamePlayTimeInSeconds = 0f; // 게임 플레이 시간을 초 단위

    private int currentSpawnPointIndex = 0;

    void Start()
    {
        // 스폰 로직을 시작
        StartCoroutine(SpawnLogic());
    }

    void Update()
    {
        // 매 프레임마다 경과한 시간(초)을 추가
        gamePlayTimeInSeconds += Time.deltaTime;
    }

    IEnumerator SpawnLogic()
    {
        float lastSpawnTime = 0f; // 마지막 스폰 시간 초기화

        while (true)
        {
            float currentTime = gamePlayTimeInSeconds; // 현재 게임 플레이 시간

            SpawnPeriod currentPeriod = FindSpawnPeriod(currentTime);
            if (currentPeriod != null)
            {
                float timeSinceLastSpawn = currentTime - lastSpawnTime;

                if (timeSinceLastSpawn >= currentPeriod.monsterSpawnInterval)
                {
                    SpawnMonster(currentPeriod);
                    // 다음 스폰 시간을 고정된 간격으로 설정
                    lastSpawnTime += currentPeriod.monsterSpawnInterval;

                    // 현재 시간과 다음 스폰 시간 사이의 시간 차이 계산
                    float nextSpawnDelay = lastSpawnTime - currentTime;
                    yield return new WaitForSeconds(nextSpawnDelay);
                }
                else
                {
                    // 다음 스폰까지의 남은 시간만큼 대기
                    yield return new WaitForSeconds(currentPeriod.monsterSpawnInterval - timeSinceLastSpawn);
                }
            }
            else
            {
                yield return new WaitForSeconds(1f); // 적절한 스폰 주기를 찾지 못한 경우 대기
            }
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
            int randomSpawnPointIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
            Transform spawnPoint = spawnPoints[randomSpawnPointIndex];

            GameObject prefabToSpawn;

            // 늑대 몬스터인 경우, 랜덤하게 프리팹 하나를 선택
            if (monsterData.prefab.name == "Wolf_Normal")
            {
                prefabToSpawn = ChooseRandomWolfPrefab();
            }
            else
            {
                prefabToSpawn = monsterData.prefab;
            }

            GameObject spawnedMonster = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
            Debug.Log("Spawned " + spawnedMonster.name + " at " + spawnPoint.position);
        }
    }


    // 두 프리팹 중 랜덤하게 하나를 선택하는 메서드
    GameObject ChooseRandomWolfPrefab()
    {
        if (UnityEngine.Random.Range(0, 2) == 0) // 0 또는 1을 랜덤하게 반환
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
                monsterSpawnInterval = 3, // 3초마다 몬스터가 스폰
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
            monsterSpawnInterval = 10,
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
            monsterSpawnInterval = 12,
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
            monsterSpawnInterval = 15,
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
            monsterSpawnInterval = 18,
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
            monsterSpawnInterval = 15,
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
