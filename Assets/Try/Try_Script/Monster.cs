using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public string monsterName;
    public int hp;
    public int speed;
    public int xp;

    
    public void Initialize(string name, int health, int moveSpeed, int experience)
    {
        monsterName = name;
        hp = health;
        speed = moveSpeed;
        xp = experience;
    }


    void Update()
    {
        
        if (transform.position.x >= -2 && transform.position.x <= 2.2)
        {
            MoveDown();
        }
    }


    void MoveDown()
    {
        float speedScale = 0.04f; //원래 속도 비율 
        transform.position += Vector3.down * speed * speedScale * Time.deltaTime;

       if (transform.position.y <= -4.5f)
        {
            Destroy(gameObject);
        }
    }
}
