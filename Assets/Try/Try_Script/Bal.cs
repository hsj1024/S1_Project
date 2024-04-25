using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bal : MonoBehaviour
{

    public static Bal Instance { get; private set; } // �̱��� �ν��Ͻ��� ������ �� �ִ� ���� �Ӽ�

    // �⺻ ����
    public int Dmg = 10; // ���ط�
    public float Rt = 1.0f; // ������ �ð�
    public int As = 25; // ����ü �ӵ� 
    public int Chc = 0; // ġ��Ÿ Ȯ��
    public float Chd = 120.0f; // ġ��Ÿ ���ط�

    // ���� ���� �� ���� ����
    public int Dot = 5; // ���� ���ط�
    public int Aoe = 10; // ���� ���ط�
    public float Pd = 50.0f; // ���� ���ط�

 
    public float XPM = 1; // ����ġ ���

    // �ͷ� ����
    public int TurretDmg = 5; // �ͷ� ���ط�
    public float TurretRt = 2.0f; // �ͷ� ������ �ð�
    public int TurretAs = 25; // �ͷ� ����ü �ӵ�

    // ���� ���� Ȱ��ȭ �÷���
    public bool isDotActive = false;
    public bool isAoeActive = false;
    public bool isPdActive = false;

    // �ͷ� ���� Ȱ��ȭ �÷���
    public bool isTurretActive = false;


    // ���� �߰�
    // �߸���Ÿ�� ȭ�� ���� �߰� ����
    
    public float ArrowSpeed = 1.0f; // ȭ�� �ӵ�

    public float totalExperience = 0; // ���� ����ġ

    /*public float TotalExperience
    {
        get { return totalExperience; }
        set { totalExperience = value; }
    }*/

    void Update()
    {

    }



    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            //Destroy(gameObject); // �̹� �ν��Ͻ��� �����Ѵٸ� �ߺ� �ν��Ͻ��� ����
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ���� ����Ǿ �ı����� �ʵ��� ����
        }
    }
    void Start()
    {
        // ���� ���� �ʱ�ȭ
        Dot = 0;
        Aoe = 0;
        Pd = 0.0f;

        // ���� ���� Ȱ��ȭ �÷��� �ʱ�ȭ
        isDotActive = false;
        isAoeActive = false;
        isPdActive = false;

        // �ͷ� ���� �ʱ�ȭ
        TurretDmg = 5; 
        TurretRt = 2.0f; 
        TurretAs = 25; 
        isTurretActive = true; 
    }



    // ���� ���ط� ���
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


    // ����ġ ���� �޼��� �߰�
    public void AddExperience(float xp)
    {
        totalExperience += xp;
        Debug.Log($"Added XP: {xp}, Total experience now: {totalExperience}");
    }

    // ���� ���� Ȱ��ȭ �޼���
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
