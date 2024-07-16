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
    public GameObject treeNormalPrefab;
    public GameObject golemNormalPrefab;
    public GameObject trollNormalPrefab;
    public GameObject specialMonster1; // 스페셜 몬스터 1
    public GameObject specialMonster2; // 스페셜 몬스터 2
    private float gamePlayTimeInSeconds = 0f; // 게임 플레이 시간을 초 단위
    public List<Monster> activeMonsters = new List<Monster>();
    private int currentSpawnPointIndex = 0;

    void Start()
    {
        // 스폰 로직을 시작
        StartCoroutine(SpawnLogic());
        StartCoroutine(SpecialSpawnLogic());
    }

    void Update()
    {
        // 매 프레임마다 경과한 시간(초)을 추가
        gamePlayTimeInSeconds += Time.deltaTime;

        // 2배속으로 설정
        if (Input.GetKeyDown(KeyCode.Alpha2)) // '2' 키를 눌렀을 때
        {
            Time.timeScale = 2.0f;
        }

        // 5배속으로 설정
        if (Input.GetKeyDown(KeyCode.Alpha5)) // '5' 키를 눌렀을 때
        {
            Time.timeScale = 5.0f;
        }

        // 일반 속도(1배속)으로 재설정
        if (Input.GetKeyDown(KeyCode.Alpha1)) // '1' 키를 눌렀을 때
        {
            Time.timeScale = 1.0f;
        }
    }

    IEnumerator SpawnLogic()
    {
        while (true)
        {
            float currentTime = gamePlayTimeInSeconds; // 현재 게임 플레이 시간
            SpawnPeriod currentPeriod = FindSpawnPeriod(currentTime);
            if (currentPeriod != null)
            {
                // 특정 초마다 1마리 몬스터를 스폰
                SpawnMonster(currentPeriod);

                // 설정된 간격에 따라 대기 후 다음 스폰으로 이동
                yield return new WaitForSeconds(currentPeriod.monsterSpawnInterval);
            }
            else
            {
                yield return new WaitForSeconds(1f); // 적절한 스폰 주기를 찾지 못한 경우 1초 대기
            }
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

            // 1번과 8번 스포너를 제외하고 랜덤 스폰
            do
            {
                randomSpawnPointIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
                spawnPoint = spawnPoints[randomSpawnPointIndex];
            } while (randomSpawnPointIndex == 0 || randomSpawnPointIndex == spawnPoints.Count - 1);

            GameObject prefabToSpawn = monsterData.prefab;

            GameObject spawnedMonster = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
            //Debug.Log("Spawned " + spawnedMonster.name + " at " + spawnPoint.position);

            // Bal 인스턴스 가져오기
            Bal balInstance = FindObjectOfType<Bal>();
        }
    }

    void SpawnSpecialMonster()
    {
        if (spawnPoints.Count < 2) return; // 스폰 포인트가 2개 미만인 경우 리턴

        // 1번과 8번 스포너의 인덱스
        int[] specialSpawnIndices = { 0, spawnPoints.Count - 1 };
        int selectedSpawnIndex = specialSpawnIndices[UnityEngine.Random.Range(0, specialSpawnIndices.Length)];
        Transform spawnPoint = spawnPoints[selectedSpawnIndex];

        GameObject[] specialPrefabs = { specialMonster1, specialMonster2 }; // 원하는 프리팹으로 교체하세요
        GameObject prefabToSpawn = specialPrefabs[UnityEngine.Random.Range(0, specialPrefabs.Length)];

        GameObject spawnedMonster = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
        //Debug.Log("Special Spawned " + spawnedMonster.name + " at " + spawnPoint.position);

        // Bal 인스턴스 가져오기
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
                monsterSpawnInterval = 2, // n초에 1마리 몬스터가 스폰
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
