using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableTile : MonoBehaviour
{
    public float initHp;
    public float breakHp;
    public bool tileBreak = false;
    public bool damaged = false;
    private float hp;

    private SpriteRenderer spriteRdr;

    public void InitializeBreakableTile()
    {
        hp = initHp;

        spriteRdr = GetComponent<SpriteRenderer>();
        spriteRdr.color = new Color(1, 1, 1, hp);
    }

    public void BreakableTileDamage()
    {
        if (damaged)
            return;

        hp -= breakHp;
        spriteRdr.color = new Color(1, 1, 1, hp);

        if (hp <= 0)
        {
            tileBreak = true;
            gameObject.SetActive(false);
        }

        damaged = true;
    }
}