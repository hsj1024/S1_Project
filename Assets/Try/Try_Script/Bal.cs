using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bal : MonoBehaviour
{
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

    // ���� ���� Ȱ��ȭ �÷���
    public bool isDotActive = false;
    public bool isAoeActive = false;
    public bool isPdActive = false;

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


}
