using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bal : MonoBehaviour
{
    public static Bal Instance { get; private set; }

    public int Dmg = 10;
    public float Rt = 1.0f;
    public int As = 25;
    public int Chc = 0;
    public float Chd = 120.0f;

    public int Dot = 5; // 지속 피해량 기본 값 5 dps
    public int Aoe = 10; // 범위 피해량 기본 값 10
    public float Pd = 50.0f;

    public float XPM = 1;

    public int TurretDmg = 5;
    public float TurretRt = 2.0f;
    public int TurretAs = 25;

    public bool isDotActive = false;
    public bool isAoeActive = false;
    public bool isPdActive = false;

    public bool isTurretActive = false;

    public bool knockbackEnabled = false;

    public float ArrowSpeed = 1.0f;

    public float totalExperience = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        Dot = 5; // 기본 지속 피해량 설정
        Aoe = 10; // 기본 범위 피해량 설정
        Pd = 50.0f;

        isDotActive = false;
        isAoeActive = false;
        isPdActive = false;

        TurretDmg = 5;
        TurretRt = 2.0f;
        TurretAs = 25;
        isTurretActive = false;

        knockbackEnabled = false;
    }

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

    public void AddExperience(float xp)
    {
        totalExperience += xp;
        //Debug.Log($"Added XP: {xp}, Total experience now: {totalExperience}");
    }

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

    public void ToggleTurret()
    {
        isTurretActive = !isTurretActive;
    }

    public void ToggleKnockback()
    {
        knockbackEnabled = !knockbackEnabled;
        Debug.Log("Knockback enabled state: " + knockbackEnabled);
    }
}
