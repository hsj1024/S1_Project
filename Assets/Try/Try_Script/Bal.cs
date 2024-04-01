using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bal : MonoBehaviour
{
    // 기본 스텟
    public int Dmg = 10; // 피해량
    public float Rt = 1.0f; // 재장전 시간
    public int As = 25; // 투사체 속도 
    public int Chc = 0; // 치명타 확률
    public float Chd = 120.0f; // 치명타 피해량

    // 게임 진행 중 선택 스텟
    public int Dot = 5; // 지속 피해량
    public int Aoe = 10; // 범위 피해량
    public float Pd = 50.0f; // 관통 피해량

 
    public float XPM = 1; // 경험치 배수

    // 터렛 스텟
    public int TurretDmg = 5; // 터렛 피해량
    public float TurretRt = 2.0f; // 터렛 재장전 시간
    public int TurretAs = 25; // 터렛 투사체 속도

    // 선택 스텟 활성화 플래그
    public bool isDotActive = false;
    public bool isAoeActive = false;
    public bool isPdActive = false;

    // 터렛 스텟 활성화 플래그
    public bool isTurretActive = false;


    // 서정 추가
    // 발리스타와 화살 관련 추가 스탯
    public float BallistaReloadTime = 2.0f; // 발리스타 재장전 시간
    public float ArrowSpeed = 1.0f; // 화살 속도

    void Start()
    {
        // 선택 스텟 초기화
        Dot = 0;
        Aoe = 0;
        Pd = 0.0f;

        // 선택 스텟 활성화 플래그 초기화
        isDotActive = false;
        isAoeActive = false;
        isPdActive = false;

        // 터렛 스텟 초기화
        TurretDmg = 5; 
        TurretRt = 2.0f; 
        TurretAs = 25; 
        isTurretActive = false; 
    }

    // 관통 피해량 계산
    public float CalculatePiercingDamage()
    {
        if (isPdActive)
        {
            return Dmg * (Pd / 100.0f);
        }
        else
        {
            return 0.0f;
        }
    }

    // 선택 스텟 활성화 메서드
    public void ActivateDot()
    {
        isDotActive = true;
    }

    public void ActivateAoe()
    {
        isAoeActive = true;
    }

    public void ActivatePd()
    {
        isPdActive = true;
    }

    public void ActivateTurret()
    {
        isTurretActive = true;
    }



}
