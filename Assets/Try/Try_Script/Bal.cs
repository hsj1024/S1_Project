using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bal : MonoBehaviour
{
    public static Bal Instance { get; private set; }

    public float Dmg = 10;
    public float Rt = 1.0f;
    public int As = 25;
    public float Chc = 0;
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

    [SerializeField]
    public int numberOfArrows = 1; // 인스펙터에서 조절 가능한 화살 갯수

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

        Dot = 5; // 기본 지속 피해량 설정
        Aoe = 10; // 기본 범위 피해량 설정
        Pd = 50.0f;
        Chc = 0;
        Chd = 120;

        isDotActive = false;
        isAoeActive = false;
        isPdActive = false;

        TurretDmg = 5;
        TurretRt = 2.0f;
        TurretAs = 25;
        isTurretActive = false;

        knockbackEnabled = false;
    }

    void Start()
    {

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
        //Debug.Log("Knockback enabled state: " + knockbackEnabled);
    }

    void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontSize = 40; // 글씨 크기 조정
        guiStyle.normal.textColor = Color.white; // 글자 색상

        float x = 20; // 왼쪽 여백 조정
        float y = Screen.height - 1050; // 아래 여백 조정

        GUI.Label(new Rect(x, y, 700, 200), "Dmg: " + Dmg, guiStyle);
        GUI.Label(new Rect(x, y + 40, 700, 200), "Rt: " + Rt, guiStyle);
        GUI.Label(new Rect(x, y + 80, 700, 200), "As: " + As, guiStyle);
        GUI.Label(new Rect(x, y + 120, 700, 200), "Chc: " + Chc, guiStyle);
        GUI.Label(new Rect(x, y + 160, 700, 200), "Chd: " + Chd, guiStyle);
        GUI.Label(new Rect(x, y + 200, 700, 200), "Dot: " + (isDotActive ? "T" : "F"), guiStyle);
        GUI.Label(new Rect(x, y + 240, 700, 200), "Aoe: " + (isAoeActive ? "T" : "F"), guiStyle);
        GUI.Label(new Rect(x, y + 280, 700, 200), "Pd: " + (isPdActive ? "T" : "F"), guiStyle);
        GUI.Label(new Rect(x, y + 320, 700, 200), "Tur: " + (isTurretActive ? "T" : "F"), guiStyle);
        GUI.Label(new Rect(x, y + 360, 700, 200), "Knock: " + (knockbackEnabled ? "T" : "F"), guiStyle);
        GUI.Label(new Rect(x, y + 400, 700, 200), "T.Ex: " + totalExperience, guiStyle);
        GUI.Label(new Rect(x, y + 440, 700, 200), "N.A: " + numberOfArrows, guiStyle);
    }


}
