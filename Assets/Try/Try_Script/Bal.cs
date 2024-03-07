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

    // 선택 스텟 활성화 플래그
    public bool isDotActive = false;
    public bool isAoeActive = false;
    public bool isPdActive = false;

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


}
